using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>A <see cref="JiggleChain"/> as the manifest holds it: plain values, so that what is written can be tested outside the editor.</summary>
    public sealed class JiggleChainData
    {
        public List<string> Bones = new List<string>();
        public float Stiffness;
        public float Damping;
        public float Gravity;
        public float Radius;
        public float MaxAngle;
        public float TailLength;
        public JiggleTailDirection TailDirection;

        // A bone spring (Magica Cloth 2's BoneSpring): the root moves too, held by a spring. Written only when there is one.
        public float SpringPower;
        public float LimitDistance = 0.05f;
        public float NormalLimitRatio = 1f;
        public float SpringNoise;
        public string NormalAxis = "Y";
        public List<string> CollisionBones = new List<string>();
        public float PushLimit;

        public bool IsSpring => SpringPower > 0f || CollisionBones.Count > 0 || PushLimit > 0f;
    }

    /// <summary>A <see cref="JiggleCollider"/> as the manifest holds it, in the space of the bone with the bone's scale baked in.</summary>
    public sealed class JiggleColliderData
    {
        public string Bone;
        public JiggleColliderShape Shape;
        public Vector3 Center;
        public JiggleColliderAxis Axis;
        public float Height;
        public float Radius;
    }

    /// <summary>
    /// The <c>jiggle</c> section of the character manifest (<c>Characters/&lt;name&gt;.json</c>, Toe target): the chains and colliders that the Toe engine's Toe.JiggleBones reads
    /// (<c>JiggleConfigResource</c>). Bone names are written as the model files write them (<see cref="Uri.EscapeDataString"/>), so the engine finds the bones by name.
    /// </summary>
    public static class JiggleExport
    {
        /// <summary>Finds the chains and colliders below <paramref name="root"/> (the character). <paramref name="warn"/> is told what is wrong, one line each.</summary>
        public static void Collect(Transform root, List<JiggleChainData> chains, List<JiggleColliderData> colliders, Action<string> warn)
        {
            foreach (var chain in root.GetComponentsInChildren<JiggleChain>(true))
            {
                if (!chain.enabled) continue;
                var bones = new List<Transform>();
                chain.CollectBones(bones, out bool branched);
                if (branched)
                    warn($"The chain starting at '{chain.name}' ends at '{bones[bones.Count - 1].name}': that bone has several children. Put a Jiggle Chain on each branch.");
                var data = new JiggleChainData
                {
                    Stiffness = chain.stiffness,
                    Damping = chain.damping,
                    Gravity = chain.gravity,
                    Radius = chain.radius,
                    MaxAngle = chain.maxAngle,
                    TailLength = chain.tailLength,
                    TailDirection = chain.tailDirection,
                };
                foreach (var bone in bones) data.Bones.Add(Uri.EscapeDataString(bone.name));
                chains.Add(data);
            }

            // A chain inside another chain's bones is a second chain on the same bones: say so.
            var seen = new Dictionary<string, string>();
            foreach (var chain in chains)
            {
                foreach (var bone in chain.Bones)
                {
                    if (seen.TryGetValue(bone, out var other) && other != chain.Bones[0])
                        warn($"The bone '{bone}' is in the chains of '{other}' and '{chain.Bones[0]}': it would be turned twice.");
                    else seen[bone] = chain.Bones[0];
                }
            }

            foreach (var collider in root.GetComponentsInChildren<JiggleCollider>(true))
            {
                if (!collider.enabled) continue;
                if (collider.radius <= 0f)
                {
                    warn($"The Jiggle Collider on '{collider.name}' has no radius and is left out.");
                    continue;
                }
                colliders.Add(ColliderData(root, collider));
            }
        }

        /// <summary>The collider with the scale of its bone (relative to the character) baked into center, radius and height, as the engine's nodes have no scale of the bone to apply.</summary>
        public static JiggleColliderData ColliderData(Transform root, JiggleCollider collider)
        {
            var scale = Divide(collider.transform.lossyScale, root.lossyScale);
            var axis = JiggleCollider.AxisVector(collider.axis);
            float along = Mathf.Abs(Vector3.Dot(scale, axis));
            // The thickness of the capsule is the largest scale across its axis.
            var across = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            float thick = collider.shape == JiggleColliderShape.Sphere
                ? Mathf.Max(across.x, Mathf.Max(across.y, across.z))
                : Mathf.Max(axis.x > 0.5f ? 0f : across.x, Mathf.Max(axis.y > 0.5f ? 0f : across.y, axis.z > 0.5f ? 0f : across.z));
            return new JiggleColliderData
            {
                Bone = Uri.EscapeDataString(collider.name),
                Shape = collider.shape,
                Center = Vector3.Scale(collider.center, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z))),
                Axis = collider.axis,
                Height = collider.height * along,
                Radius = collider.radius * thick,
            };
        }

        private static Vector3 Divide(Vector3 a, Vector3 b)
            => new Vector3(Mathf.Approximately(b.x, 0f) ? a.x : a.x / b.x, Mathf.Approximately(b.y, 0f) ? a.y : a.y / b.y, Mathf.Approximately(b.z, 0f) ? a.z : a.z / b.z);

        /// <summary>The value of the manifest's <c>jiggle</c> key: an object with <c>chains</c> and <c>colliders</c>. Numbers do not depend on the culture of the machine.</summary>
        public static string ToJson(IList<JiggleChainData> chains, IList<JiggleColliderData> colliders, string indent)
        {
            string Q(string s) => "\"" + (s ?? string.Empty).Replace("\\", "/").Replace("\"", "\\\"") + "\"";
            string F(float v) => v.ToString("R", CultureInfo.InvariantCulture);
            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"{indent}  \"chains\": [");
            for (int i = 0; i < chains.Count; i++)
            {
                var c = chains[i];
                json.Append($"{indent}    {{ \"bones\": [{string.Join(", ", c.Bones.ConvertAll(Q))}], \"stiffness\": {F(c.Stiffness)}, \"damping\": {F(c.Damping)}, \"gravity\": {F(c.Gravity)}, " +
                            $"\"radius\": {F(c.Radius)}, \"maxAngle\": {F(c.MaxAngle)}, \"tailLength\": {F(c.TailLength)}, \"tailDirection\": {Q(c.TailDirection.ToString())}");
                if (c.IsSpring)
                    json.Append($", \"springPower\": {F(c.SpringPower)}, \"limitDistance\": {F(c.LimitDistance)}, \"normalLimitRatio\": {F(c.NormalLimitRatio)}, \"springNoise\": {F(c.SpringNoise)}, " +
                                $"\"normalAxis\": {Q(c.NormalAxis)}, \"collisionBones\": [{string.Join(", ", c.CollisionBones.ConvertAll(Q))}], \"pushLimit\": {F(c.PushLimit)}");
                json.Append(" }");
                json.AppendLine(i + 1 < chains.Count ? "," : string.Empty);
            }
            json.AppendLine($"{indent}  ],");
            json.AppendLine($"{indent}  \"colliders\": [");
            for (int i = 0; i < colliders.Count; i++)
            {
                var c = colliders[i];
                json.Append($"{indent}    {{ \"bone\": {Q(c.Bone)}, \"shape\": {Q(c.Shape.ToString().ToLowerInvariant())}, \"center\": [{F(c.Center.x)}, {F(c.Center.y)}, {F(c.Center.z)}], " +
                            $"\"axis\": {Q(c.Axis.ToString())}, \"height\": {F(c.Height)}, \"radius\": {F(c.Radius)} }}");
                json.AppendLine(i + 1 < colliders.Count ? "," : string.Empty);
            }
            json.AppendLine($"{indent}  ]");
            json.Append(indent + "}");
            return json.ToString();
        }
    }
}
