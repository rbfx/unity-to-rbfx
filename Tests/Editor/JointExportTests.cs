using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityToRebelFork.Editor;

namespace UnityToRebelFork
{
    /// <summary>Unity joints as rbfx constraints: the type and the limits, the two frames, and the node the other body is named by.</summary>
    [TestFixture]
    public class JointExportTests
    {
        private readonly List<GameObject> _made = new List<GameObject>();

        [TearDown]
        public void Clean()
        {
            foreach (var go in _made)
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            _made.Clear();
        }

        private GameObject Body(string name, Vector3 position, Transform parent = null)
        {
            var go = new GameObject(name);
            _made.Add(go);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.AddComponent<Rigidbody>();
            return go;
        }

        private static PrefabVisitor Visitor()
        {
            var settings = ScriptableObject.CreateInstance<ExportSettings>();
            return new PrefabVisitor(new Lazy<ExportOrchestrator>(() => null), settings);
        }

        private static List<string> NoWarnings(out Action<string> warn)
        {
            var list = new List<string>();
            warn = list.Add;
            return list;
        }

        private static UnityToRebelFork.Editor.Constraint ConstraintOf(Node node) => node.Components.OfType<UnityToRebelFork.Editor.Constraint>().Single();

        private static void AssertVector(Vector3 expected, Vector3 actual, float tolerance = 1e-4f)
            => Assert.AreEqual(0f, Vector3.Distance(expected, actual), tolerance, $"{expected} vs {actual}");

        // ---- Frames ----

        [Test]
        public void TheFrameOfAHinge_HasItsZAxisAlongTheAxis_AndTheOneOfAConeTwistItsXAxis()
        {
            foreach (var axis in new[] { Vector3.right, Vector3.up, Vector3.forward, new Vector3(1, 2, -3).normalized, -Vector3.up })
            {
                AssertVector(axis, JointExporter.AxisAsZ(axis, null) * Vector3.forward);
                AssertVector(axis, JointExporter.AxisAsX(axis, null) * Vector3.right);
            }

            // The hint says where Y goes, as far as it is perpendicular to the axis.
            var frame = JointExporter.AxisAsX(Vector3.right, new Vector3(0.3f, 0, 1));
            AssertVector(Vector3.forward, frame * Vector3.up);
            AssertVector(Vector3.Cross(Vector3.right, Vector3.forward), frame * Vector3.forward);
        }

        // ---- Hinge ----

        [Test]
        public void AHingeKeepsItsAnchorsAxisAndLimits_AndNamesTheNodeOfTheOtherBody()
        {
            var root = new GameObject("root");
            _made.Add(root);
            var a = Body("a", new Vector3(0, 2, 0), root.transform);
            var b = Body("b", new Vector3(3, 2, 0), root.transform);
            a.transform.rotation = Quaternion.Euler(10, 20, 30);
            b.transform.rotation = Quaternion.Euler(-40, 5, 15);
            var hinge = a.AddComponent<HingeJoint>();
            hinge.connectedBody = b.GetComponent<Rigidbody>();
            hinge.autoConfigureConnectedAnchor = false;
            hinge.anchor = new Vector3(0.5f, 0, 0);
            hinge.connectedAnchor = new Vector3(-2.5f, 0, 0);
            hinge.axis = Vector3.up;
            hinge.useLimits = true;
            hinge.limits = new JointLimits { min = -20, max = 45 };

            var tree = Visitor().Visit(root, isRoot: true);
            var nodeA = tree.Children.Single(n => n.Name == "a");
            var nodeB = tree.Children.Single(n => n.Name == "b");
            var constraint = ConstraintOf(nodeA);

            Assert.AreEqual(ConstraintType.Hinge, constraint.ConstraintType);
            AssertVector(new Vector3(0.5f, 0, 0), constraint.Position);
            AssertVector(new Vector3(-2.5f, 0, 0), constraint.OtherBodyPosition);
            Assert.AreEqual(new Vector2(-20, 0), constraint.LowLimit);
            Assert.AreEqual(new Vector2(45, 0), constraint.HighLimit);
            Assert.AreEqual(nodeB.Id, constraint.OtherBodyNodeID);

            // Both frames turn about the same axis in the world.
            var worldAxisA = a.transform.rotation * (constraint.Rotation * Vector3.forward);
            var worldAxisB = b.transform.rotation * (constraint.OtherBodyRotation * Vector3.forward);
            AssertVector(a.transform.rotation * Vector3.up, worldAxisA);
            AssertVector(worldAxisA, worldAxisB);
            Assert.IsTrue(constraint.DisableCollision, "Unity's default is that jointed bodies do not collide.");
        }

        [Test]
        public void TheOtherBodyIsNamedWhetherItsNodeComesBeforeOrAfter()
        {
            var root = new GameObject("root");
            _made.Add(root);
            var first = Body("first", Vector3.zero, root.transform);
            var second = Body("second", Vector3.right, root.transform);
            first.AddComponent<HingeJoint>().connectedBody = second.GetComponent<Rigidbody>();
            second.AddComponent<HingeJoint>().connectedBody = first.GetComponent<Rigidbody>();

            var tree = Visitor().Visit(root, isRoot: true);
            var nodeFirst = tree.Children.Single(n => n.Name == "first");
            var nodeSecond = tree.Children.Single(n => n.Name == "second");
            Assert.AreEqual(nodeSecond.Id, ConstraintOf(nodeFirst).OtherBodyNodeID, "The other node comes later in the walk.");
            Assert.AreEqual(nodeFirst.Id, ConstraintOf(nodeSecond).OtherBodyNodeID, "The other node came earlier.");
        }

        [Test]
        public void AHingeWithoutAConnectedBody_IsJoinedToTheWorldAtItsWorldAnchor()
        {
            var root = new GameObject("root");
            _made.Add(root);
            root.transform.rotation = Quaternion.Euler(0, 90, 0);
            var a = Body("a", new Vector3(1, 0, 0), root.transform);
            var hinge = a.AddComponent<HingeJoint>();
            hinge.autoConfigureConnectedAnchor = false;
            hinge.connectedAnchor = new Vector3(5, 6, 7); // A world position without a connected body.
            hinge.axis = Vector3.forward;

            var constraint = ConstraintOf(Visitor().Visit(root, isRoot: true).Children.Single());
            Assert.AreEqual(0, constraint.OtherBodyNodeID);
            AssertVector(new Vector3(5, 6, 7), constraint.OtherBodyPosition);
            // The frame of the world is the world frame: its Z axis is the joint axis in the world.
            AssertVector(a.transform.rotation * Vector3.forward, constraint.OtherBodyRotation * Vector3.forward);
        }

        [Test]
        public void AHingeThatMayTurnAllTheWayRound_HasNoLimit()
        {
            var root = new GameObject("root");
            _made.Add(root);
            var a = Body("a", Vector3.zero, root.transform);
            a.AddComponent<HingeJoint>();
            var constraint = ConstraintOf(Visitor().Visit(root, isRoot: true).Children.Single());
            // 0 to 0 would lock a hinge: the whole circle is no limit.
            Assert.AreEqual(-180f, constraint.LowLimit.x);
            Assert.AreEqual(180f, constraint.HighLimit.x);
        }

        // ---- The other joints ----

        [Test]
        public void ACharacterJoint_IsAConeTwistWithTheTwistAndTheLargerSwing()
        {
            var warnings = NoWarnings(out var warn);
            var go = Body("limb", Vector3.zero);
            var joint = go.AddComponent<CharacterJoint>();
            joint.axis = Vector3.right;
            joint.swingAxis = Vector3.up;
            joint.lowTwistLimit = new SoftJointLimit { limit = -30 };
            joint.highTwistLimit = new SoftJointLimit { limit = 20 };
            joint.swing1Limit = new SoftJointLimit { limit = 40 };
            joint.swing2Limit = new SoftJointLimit { limit = 25 };

            var export = JointExporter.Export(joint, warn);
            Assert.AreEqual(ConstraintType.ConeTwist, export.Constraint.ConstraintType);
            Assert.AreEqual(new Vector2(30, 40), export.Constraint.HighLimit);
            AssertVector(Vector3.right, export.Constraint.Rotation * Vector3.right);
            AssertVector(Vector3.up, export.Constraint.Rotation * Vector3.up);
            Assert.IsTrue(warnings.Any(w => w.Contains("swing limits differ")), string.Join("; ", warnings));
        }

        [Test]
        public void AFixedJoint_IsAConeTwistThatDoesNotMove()
        {
            var go = Body("part", Vector3.zero);
            var export = JointExporter.Export(go.AddComponent<FixedJoint>(), _ => { });
            Assert.AreEqual(ConstraintType.ConeTwist, export.Constraint.ConstraintType);
            Assert.AreEqual(Vector2.zero, export.Constraint.HighLimit);
        }

        [Test]
        public void ASpringJoint_BecomesAPointJoint_AndSaysWhatIsLost()
        {
            var warnings = NoWarnings(out var warn);
            var go = Body("bob", Vector3.zero);
            var export = JointExporter.Export(go.AddComponent<SpringJoint>(), warn);
            Assert.AreEqual(ConstraintType.Point, export.Constraint.ConstraintType);
            Assert.IsTrue(warnings.Any(w => w.Contains("spring")));
        }

        [Test]
        public void ABrokenOffJointCannotBreak_ButSaysSo()
        {
            var warnings = NoWarnings(out var warn);
            var go = Body("p", Vector3.zero);
            var joint = go.AddComponent<FixedJoint>();
            joint.breakForce = 100f;
            JointExporter.Export(joint, warn);
            Assert.IsTrue(warnings.Any(w => w.Contains("break force")));
        }

        // ---- Configurable joints ----

        private static ConfigurableJoint Locked(GameObject go)
        {
            var joint = go.AddComponent<ConfigurableJoint>();
            joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;
            return joint;
        }

        [Test]
        public void AConfigurableJointThatIsLockedEverywhere_IsAConeTwistThatDoesNotMove()
        {
            var export = JointExporter.Export(Locked(Body("x", Vector3.zero)), _ => { });
            Assert.AreEqual(ConstraintType.ConeTwist, export.Constraint.ConstraintType);
            Assert.AreEqual(Vector2.zero, export.Constraint.HighLimit);
        }

        [Test]
        public void AConfigurableJointThatTurnsAboutOneAxis_IsAHingeAboutThatAxis()
        {
            var go = Body("door", Vector3.zero);
            var joint = Locked(go);
            joint.axis = Vector3.right;
            joint.secondaryAxis = Vector3.up;
            joint.angularYMotion = ConfigurableJointMotion.Limited; // About the joint's Y axis.
            joint.angularYLimit = new SoftJointLimit { limit = 70 };

            var export = JointExporter.Export(joint, _ => { });
            Assert.AreEqual(ConstraintType.Hinge, export.Constraint.ConstraintType);
            Assert.AreEqual(new Vector2(-70, 0), export.Constraint.LowLimit);
            Assert.AreEqual(new Vector2(70, 0), export.Constraint.HighLimit);
            AssertVector(Vector3.up, export.Constraint.Rotation * Vector3.forward);
        }

        [Test]
        public void AConfigurableJointThatTurnsAboutItsXAxis_UsesTheLowAndHighLimitsOfX()
        {
            var joint = Locked(Body("door", Vector3.zero));
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.lowAngularXLimit = new SoftJointLimit { limit = -10 };
            joint.highAngularXLimit = new SoftJointLimit { limit = 80 };

            var export = JointExporter.Export(joint, _ => { });
            Assert.AreEqual(ConstraintType.Hinge, export.Constraint.ConstraintType);
            Assert.AreEqual(new Vector2(-10, 0), export.Constraint.LowLimit);
            Assert.AreEqual(new Vector2(80, 0), export.Constraint.HighLimit);
            AssertVector(Vector3.right, export.Constraint.Rotation * Vector3.forward);
        }

        [Test]
        public void AConfigurableJointThatSlidesAlongOneAxis_IsASliderAlongThatAxis()
        {
            var joint = Locked(Body("drawer", Vector3.zero));
            joint.axis = Vector3.right;
            joint.secondaryAxis = Vector3.up;
            joint.zMotion = ConfigurableJointMotion.Limited; // Along the joint's Z axis, which is -... Cross(right, up) = forward.
            joint.linearLimit = new SoftJointLimit { limit = 0.4f };

            var export = JointExporter.Export(joint, _ => { });
            Assert.AreEqual(ConstraintType.Slider, export.Constraint.ConstraintType);
            Assert.AreEqual(new Vector2(-0.4f, 0), export.Constraint.LowLimit);
            Assert.AreEqual(new Vector2(0.4f, 0), export.Constraint.HighLimit);
            AssertVector(Vector3.forward, export.Constraint.Rotation * Vector3.right); // The slider's X axis.
        }

        [Test]
        public void AConfigurableJointThatMovesAlongTwoAxes_HasNoEquivalent()
        {
            var warnings = NoWarnings(out var warn);
            var joint = Locked(Body("free", Vector3.zero));
            joint.xMotion = joint.yMotion = ConfigurableJointMotion.Free;
            Assert.IsNull(JointExporter.Export(joint, warn));
            Assert.IsTrue(warnings.Any(w => w.Contains("no equivalent")));
        }

        // ---- Through the visitor and the file ----

        [Test]
        public void AJointToABodyOutsideTheExportedTree_BecomesAJointToTheWorld()
        {
            var outside = Body("outside", new Vector3(10, 0, 0));
            outside.transform.rotation = Quaternion.Euler(0, 30, 0);
            var root = new GameObject("root");
            _made.Add(root);
            var a = Body("a", Vector3.zero, root.transform);
            var hinge = a.AddComponent<HingeJoint>();
            hinge.connectedBody = outside.GetComponent<Rigidbody>();
            hinge.autoConfigureConnectedAnchor = false;
            hinge.connectedAnchor = new Vector3(-1, 0, 0);

            LogAssert.Expect(LogType.Warning, new Regex("not exported with it"));
            var constraint = ConstraintOf(Visitor().Visit(root, isRoot: true).Children.Single());
            Assert.AreEqual(0, constraint.OtherBodyNodeID);
            AssertVector(outside.transform.TransformPoint(new Vector3(-1, 0, 0)), constraint.OtherBodyPosition);
        }

        [Test]
        public void ASceneWritesTheConstraintWithRbfxsAttributeNames()
        {
            var root = new GameObject("root");
            _made.Add(root);
            var a = Body("a", Vector3.zero, root.transform);
            var b = Body("b", Vector3.right, root.transform);
            var hinge = a.AddComponent<HingeJoint>();
            hinge.connectedBody = b.GetComponent<Rigidbody>();
            hinge.useLimits = true;
            hinge.limits = new JointLimits { min = 0, max = 90 };

            var tree = Visitor().Visit(root, isRoot: true);
            var scene = new Scene();
            scene.Children.Add(tree);
            string xml = new PrefabSceneSerializer().Visit(scene).ToString();

            StringAssert.Contains("_typeName=\"Constraint\"", xml);
            StringAssert.Contains("\"Constraint Type\"", xml);
            StringAssert.Contains("Hinge", xml);
            StringAssert.Contains("\"Other Body NodeID\"", xml);
            StringAssert.Contains("\"High Limit\"", xml);
        }
    }
}
