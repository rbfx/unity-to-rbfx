using System;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>What a Unity joint became: the constraint, and the object whose body it is joined to (null: joined to the world).</summary>
    public sealed class JointExport
    {
        public UnityToRebelFork.Editor.Constraint Constraint;

        /// <summary>The object of <see cref="Joint.connectedBody"/>. Its node id is only known once that node exists (see <see cref="PrefabVisitor"/>).</summary>
        public GameObject Connected;
    }

    /// <summary>
    /// Unity joints as rbfx's <c>Constraint</c> (Point, Hinge, Slider, ConeTwist). rbfx keeps a frame (position and rotation) in the space of each body's node and
    /// holds the two frames together: a hinge turns about the frame's Z axis, a slider moves along its X axis, a cone-twist twists about its X axis. The frames
    /// here are built so that they coincide in the world at the pose the scene is exported in, which is the pose Unity measures joint angles from.
    /// </summary>
    public static class JointExporter
    {
        /// <summary>The limits of a hinge that may turn all the way round: the whole circle is no limit (a limit of 0 to 0 would lock it).</summary>
        private static readonly Vector2 FreeHingeLow = new Vector2(-180f, 0f);
        private static readonly Vector2 FreeHingeHigh = new Vector2(180f, 0f);

        /// <summary>
        /// Exports a joint, or returns null (after calling <paramref name="warn"/> with the reason) when there is no equivalent. Never throws for a joint it
        /// does not know.
        /// </summary>
        public static JointExport Export(Joint joint, Action<string> warn)
        {
            if (joint == null) return null;
            string who = $"{joint.gameObject.name}: {joint.GetType().Name}";
            if (joint.gameObject.GetComponent<Rigidbody>() == null)
            {
                warn($"{who} has no Rigidbody, so it is not exported");
                return null;
            }
            if (joint.connectedBody == null && joint.connectedArticulationBody != null)
            {
                warn($"{who} is joined to an ArticulationBody, which has no equivalent; not exported");
                return null;
            }

            if (!Classify(joint, who, warn, out var plan)) return null;

            var ownWorldRotation = joint.transform.rotation;
            var frame = plan.Frame;

            var constraint = new UnityToRebelFork.Editor.Constraint
            {
                ConstraintType = plan.Type,
                Position = joint.anchor,
                Rotation = frame,
                OtherBodyPosition = joint.connectedAnchor,
                HighLimit = plan.High,
                LowLimit = plan.Low,
                DisableCollision = !joint.enableCollision,
            };

            // The same frame as seen from the other body: in its node's space, or in the world when there is none (rbfx's frame of no body is the world).
            var worldFrame = ownWorldRotation * frame;
            var connected = joint.connectedBody;
            constraint.OtherBodyRotation = connected != null ? Quaternion.Inverse(connected.transform.rotation) * worldFrame : worldFrame;

            if (!float.IsInfinity(joint.breakForce) || !float.IsInfinity(joint.breakTorque))
                warn($"{who}: break force and torque are not exported");
            if (joint.axis == Vector3.zero && plan.Type != ConstraintType.Point)
                warn($"{who}: the joint axis is zero; the frame is the identity");

            return new JointExport { Constraint = constraint, Connected = connected != null ? connected.gameObject : null };
        }

        // ---- What each joint becomes ----

        private struct Plan
        {
            public string Type;
            public Quaternion Frame;
            public Vector2 Low, High;
        }

        private static bool Classify(Joint joint, string who, Action<string> warn, out Plan plan)
        {
            plan = default;
            var axis = Normalized(joint.axis, Vector3.right);

            switch (joint)
            {
                case HingeJoint hinge:
                    plan.Type = ConstraintType.Hinge;
                    plan.Frame = AxisAsZ(axis, null);
                    if (hinge.useLimits)
                    {
                        plan.Low = new Vector2(hinge.limits.min, 0f);
                        plan.High = new Vector2(hinge.limits.max, 0f);
                    }
                    else
                    {
                        plan.Low = FreeHingeLow;
                        plan.High = FreeHingeHigh;
                    }
                    if (hinge.useMotor || hinge.useSpring) warn($"{who}: the motor and spring of a hinge are not exported");
                    return true;

                case CharacterJoint character:
                {
                    plan.Type = ConstraintType.ConeTwist;
                    var swingAxis = Normalized(character.swingAxis, Vector3.up);
                    plan.Frame = AxisAsX(axis, swingAxis);
                    float twist = Mathf.Max(Mathf.Abs(character.lowTwistLimit.limit), Mathf.Abs(character.highTwistLimit.limit));
                    float swing1 = Mathf.Abs(character.swing1Limit.limit), swing2 = Mathf.Abs(character.swing2Limit.limit);
                    if (!Mathf.Approximately(swing1, swing2))
                        warn($"{who}: the two swing limits differ ({swing1}, {swing2}); a cone has one, the larger is used");
                    plan.High = new Vector2(twist, Mathf.Max(swing1, swing2));
                    return true;
                }

                case FixedJoint _:
                    // A locked frame: no swing and no twist.
                    plan.Type = ConstraintType.ConeTwist;
                    plan.Frame = AxisAsX(axis, null);
                    return true;

                case SpringJoint spring:
                    plan.Type = ConstraintType.Point;
                    plan.Frame = Quaternion.identity;
                    warn($"{who}: the spring (spring {spring.spring}, damper {spring.damper}, distance {spring.minDistance} to {spring.maxDistance}) is not exported; the bodies are joined at a point");
                    return true;

                case ConfigurableJoint configurable:
                    return ClassifyConfigurable(configurable, who, warn, out plan);

                default:
                    warn($"{who} has no equivalent in rbfx; not exported");
                    return false;
            }
        }

        private static bool ClassifyConfigurable(ConfigurableJoint joint, string who, Action<string> warn, out Plan plan)
        {
            plan = default;
            if (joint.configuredInWorldSpace || joint.swapBodies)
                warn($"{who}: world space configuration and swapped bodies are ignored");

            var x = Normalized(joint.axis, Vector3.right);
            var y = OrthoTo(x, Normalized(joint.secondaryAxis, Vector3.up));
            var z = Vector3.Cross(x, y);

            var linear = new[] { joint.xMotion, joint.yMotion, joint.zMotion };
            var linearAxes = new[] { x, y, z };
            var angular = new[] { joint.angularXMotion, joint.angularYMotion, joint.angularZMotion };
            int linearFree = 0, angularFree = 0, linearIndex = -1, angularIndex = -1;
            for (int i = 0; i < 3; i++)
            {
                if (linear[i] != ConfigurableJointMotion.Locked) { linearFree++; linearIndex = i; }
                if (angular[i] != ConfigurableJointMotion.Locked) { angularFree++; angularIndex = i; }
            }

            if (linearFree == 0)
            {
                if (angularFree == 0)
                {
                    plan.Type = ConstraintType.ConeTwist;
                    plan.Frame = FromAxes(x, y, z);
                    return true;
                }

                if (angularFree == 1)
                {
                    // One free axis of rotation: a hinge about it. Its limits are those of that axis.
                    plan.Type = ConstraintType.Hinge;
                    var about = angularIndex == 0 ? x : angularIndex == 1 ? y : z;
                    var other = angularIndex == 0 ? y : angularIndex == 1 ? z : x;
                    plan.Frame = AxisAsZ(about, other);
                    if (angular[angularIndex] == ConfigurableJointMotion.Free)
                    {
                        plan.Low = FreeHingeLow;
                        plan.High = FreeHingeHigh;
                    }
                    else if (angularIndex == 0)
                    {
                        plan.Low = new Vector2(joint.lowAngularXLimit.limit, 0f);
                        plan.High = new Vector2(joint.highAngularXLimit.limit, 0f);
                    }
                    else
                    {
                        float limit = angularIndex == 1 ? joint.angularYLimit.limit : joint.angularZLimit.limit;
                        plan.Low = new Vector2(-limit, 0f);
                        plan.High = new Vector2(limit, 0f);
                    }
                    return true;
                }

                // More than one free rotation: a ball. The limits (and the drives) of a configurable joint are not a cone.
                plan.Type = ConstraintType.Point;
                plan.Frame = Quaternion.identity;
                warn($"{who}: {angularFree} rotations are free or limited; exported as a point joint without limits");
                return true;
            }

            if (linearFree == 1)
            {
                plan.Type = ConstraintType.Slider;
                var along = linearAxes[linearIndex];
                var other = linearIndex == 0 ? y : linearIndex == 1 ? z : x;
                plan.Frame = AxisAsX(along, other);
                // Bullet's limits apply only while lower <= upper: a free slider has lower > upper.
                float limit = joint.linearLimit.limit;
                bool limited = linear[linearIndex] == ConfigurableJointMotion.Limited;
                plan.Low = new Vector2(limited ? -limit : 1f, 0f);
                plan.High = new Vector2(limited ? limit : -1f, 0f);
                if (angularFree != 0)
                {
                    plan.Low.y = 1f;
                    plan.High.y = -1f;
                    warn($"{who}: the rotation of a slider is not limited");
                }
                return true;
            }

            warn($"{who}: {linearFree} free linear axes have no equivalent in rbfx; not exported");
            return false;
        }

        // ---- Frames ----
        // Unity is left handed: right = Cross(up, forward) and forward = Cross(right, up). A frame is given by where its axes point.

        /// <summary>The frame whose X, Y and Z axes point along the given directions (orthonormal).</summary>
        public static Quaternion FromAxes(Vector3 x, Vector3 y, Vector3 z) => Quaternion.LookRotation(z, y);

        /// <summary>The frame whose Z axis is <paramref name="axis"/> (a hinge turns about it); <paramref name="hint"/> says roughly where Y points.</summary>
        public static Quaternion AxisAsZ(Vector3 axis, Vector3? hint)
        {
            var y = OrthoTo(axis, hint ?? Vector3.up);
            return FromAxes(Vector3.Cross(y, axis), y, axis);
        }

        /// <summary>The frame whose X axis is <paramref name="axis"/> (a slider moves along it, a cone-twist twists about it); <paramref name="hint"/> says roughly where Y points.</summary>
        public static Quaternion AxisAsX(Vector3 axis, Vector3? hint)
        {
            var y = OrthoTo(axis, hint ?? Vector3.up);
            return FromAxes(axis, y, Vector3.Cross(axis, y));
        }

        private static Vector3 Normalized(Vector3 v, Vector3 fallback) => v.sqrMagnitude < 1e-8f ? fallback : v.normalized;

        /// <summary>A unit vector perpendicular to <paramref name="axis"/>, as near to <paramref name="hint"/> as it can be.</summary>
        private static Vector3 OrthoTo(Vector3 axis, Vector3 hint)
        {
            var y = hint - axis * Vector3.Dot(hint, axis);
            if (y.sqrMagnitude < 1e-6f)
            {
                // The hint lies along the axis: any other direction will do.
                var other = Mathf.Abs(axis.y) < 0.9f ? Vector3.up : Vector3.right;
                y = other - axis * Vector3.Dot(other, axis);
            }
            return y.normalized;
        }
    }
}
