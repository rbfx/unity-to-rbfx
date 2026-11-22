using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityToRebelFork.Editor;

namespace UnityToRebelFork
{
    /// <summary>Jiggle chains and colliders for the Toe engine: what the exporter collects from a character and writes into its manifest.</summary>
    [TestFixture]
    public class JiggleExportTests
    {
        [Serializable]
        private class ChainJson
        {
            public string[] bones;
            public float stiffness;
            public float damping;
            public float gravity;
            public float radius;
            public float maxAngle;
            public float tailLength;
            public string tailDirection;
        }

        [Serializable]
        private class ColliderJson
        {
            public string bone;
            public string shape;
            public float[] center;
            public string axis;
            public float height;
            public float radius;
        }

        [Serializable]
        private class SectionJson
        {
            public ChainJson[] chains;
            public ColliderJson[] colliders;
        }

        private readonly List<GameObject> _made = new List<GameObject>();

        [TearDown]
        public void Clean()
        {
            foreach (var go in _made)
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            _made.Clear();
        }

        private GameObject Make(string name, Transform parent = null, Vector3? position = null)
        {
            var go = new GameObject(name);
            _made.Add(go);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = position ?? Vector3.zero;
            return go;
        }

        /// <summary>A character with a pelvis and a skirt chain of three bones.</summary>
        private (GameObject Root, GameObject Pelvis, GameObject[] Skirt) Skirted(string firstBone = "skirt_01")
        {
            var root = Make("Girl");
            var pelvis = Make("pelvis", root.transform, new Vector3(0f, 1f, 0f));
            var skirt = new GameObject[3];
            var parent = pelvis;
            for (int i = 0; i < 3; i++)
            {
                skirt[i] = Make(i == 0 ? firstBone : "skirt_0" + (i + 1), parent.transform, new Vector3(0f, -0.1f, 0f));
                parent = skirt[i];
            }
            return (root, pelvis, skirt);
        }

        private static (List<JiggleChainData> Chains, List<JiggleColliderData> Colliders, List<string> Warnings) Collect(GameObject root)
        {
            var chains = new List<JiggleChainData>();
            var colliders = new List<JiggleColliderData>();
            var warnings = new List<string>();
            JiggleExport.Collect(root.transform, chains, colliders, warnings.Add);
            return (chains, colliders, warnings);
        }

        [Test]
        public void AChain_IsTheRootBoneAndTheBonesBelowItOneByOne()
        {
            var (root, _, skirt) = Skirted();
            var chain = skirt[0].AddComponent<JiggleChain>();
            chain.stiffness = 0.4f;
            chain.tailDirection = JiggleTailDirection.Down;
            chain.tailLength = 0.07f;

            var (chains, _, warnings) = Collect(root);
            Assert.AreEqual(1, chains.Count);
            CollectionAssert.AreEqual(new[] { "skirt_01", "skirt_02", "skirt_03" }, chains[0].Bones);
            Assert.AreEqual(0.4f, chains[0].Stiffness);
            Assert.AreEqual(JiggleTailDirection.Down, chains[0].TailDirection);
            Assert.AreEqual(0.07f, chains[0].TailLength);
            Assert.IsEmpty(warnings);
        }

        [Test]
        public void ABoneWithSeveralChildren_EndsTheChain_WithAWarning()
        {
            var (root, _, skirt) = Skirted();
            Make("extra", skirt[1].transform);
            skirt[0].AddComponent<JiggleChain>();

            var (chains, _, warnings) = Collect(root);
            CollectionAssert.AreEqual(new[] { "skirt_01", "skirt_02" }, chains[0].Bones);
            Assert.AreEqual(1, warnings.Count);
            StringAssert.Contains("several children", warnings[0]);
        }

        [Test]
        public void ADisabledComponent_IsLeftOut()
        {
            var (root, pelvis, skirt) = Skirted();
            skirt[0].AddComponent<JiggleChain>().enabled = false;
            var collider = pelvis.AddComponent<JiggleCollider>();
            collider.enabled = false;

            var (chains, colliders, _) = Collect(root);
            Assert.IsEmpty(chains);
            Assert.IsEmpty(colliders);
        }

        [Test]
        public void BoneNames_AreWrittenAsTheModelFilesWriteThem()
        {
            var (root, _, skirt) = Skirted(firstBone: "skirt 01");
            skirt[0].AddComponent<JiggleChain>();

            var (chains, _, _) = Collect(root);
            Assert.AreEqual("skirt%2001", chains[0].Bones[0]);
        }

        [Test]
        public void TheScaleOfABone_IsBakedIntoItsCollider()
        {
            var root = Make("Girl");
            var thigh = Make("thigh_l", root.transform);
            thigh.transform.localScale = new Vector3(2f, 2f, 2f);
            var collider = thigh.AddComponent<JiggleCollider>();
            collider.shape = JiggleColliderShape.Capsule;
            collider.axis = JiggleColliderAxis.Y;
            collider.center = new Vector3(0f, 0.1f, 0f);
            collider.radius = 0.05f;
            collider.height = 0.4f;

            var data = Collect(root).Colliders.Single();
            Assert.AreEqual("thigh_l", data.Bone);
            Assert.AreEqual(0.1f, data.Radius, 1e-5f);
            Assert.AreEqual(0.8f, data.Height, 1e-5f);
            Assert.AreEqual(0.2f, data.Center.y, 1e-5f);
        }

        [Test]
        public void AColliderWithoutARadius_IsLeftOut_WithAWarning()
        {
            var root = Make("Girl");
            var bone = Make("head", root.transform);
            bone.AddComponent<JiggleCollider>().radius = 0f;

            var (_, colliders, warnings) = Collect(root);
            Assert.IsEmpty(colliders);
            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void TheManifestSection_IsJsonTheEngineReads()
        {
            var (root, pelvis, skirt) = Skirted();
            var chain = skirt[0].AddComponent<JiggleChain>();
            chain.tailDirection = JiggleTailDirection.AwayFromParent;
            var collider = pelvis.AddComponent<JiggleCollider>();
            collider.shape = JiggleColliderShape.Sphere;
            collider.center = new Vector3(0f, 0.01f, 0.02f);
            collider.radius = 0.15f;

            var (chains, colliders, _) = Collect(root);
            string json = JiggleExport.ToJson(chains, colliders, "  ");
            var section = JsonUtility.FromJson<SectionJson>(json);

            Assert.AreEqual(1, section.chains.Length);
            CollectionAssert.AreEqual(new[] { "skirt_01", "skirt_02", "skirt_03" }, section.chains[0].bones);
            Assert.AreEqual("AwayFromParent", section.chains[0].tailDirection);
            Assert.AreEqual(0.3f, section.chains[0].stiffness, 1e-6f);
            Assert.AreEqual(1, section.colliders.Length);
            Assert.AreEqual("pelvis", section.colliders[0].bone);
            Assert.AreEqual("sphere", section.colliders[0].shape);
            CollectionAssert.AreEqual(new[] { 0f, 0.01f, 0.02f }, section.colliders[0].center);
            Assert.AreEqual(0.15f, section.colliders[0].radius, 1e-6f);
        }

        [Test]
        public void TheNumbers_DoNotDependOnTheCultureOfTheMachine()
        {
            var old = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
                var (root, _, skirt) = Skirted();
                skirt[0].AddComponent<JiggleChain>().stiffness = 0.35f;
                var (chains, colliders, _) = Collect(root);
                string json = JiggleExport.ToJson(chains, colliders, string.Empty);
                StringAssert.Contains("\"stiffness\": 0.35,", json);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = old;
            }
        }

        [Test]
        public void TheSetup_AddsAChainToEachRootBoneOfARule_AndNotToTheBonesInsideIt()
        {
            var (root, pelvis, _) = Skirted();
            var side = Make("skirt_side_01", pelvis.transform);
            Make("skirt_side_02", side.transform);
            var head = Make("head", root.transform);
            Make("harir_l_01", head.transform);
            Make("breast_l", pelvis.transform);

            int added = JiggleSetup.AddChains(root, JiggleSetupOptions.Default());

            Assert.AreEqual(4, added); // skirt_01, skirt_side_01, harir_l_01, breast_l
            Assert.IsNotNull(_made.First(g => g.name == "skirt_01").GetComponent<JiggleChain>());
            Assert.IsNull(_made.First(g => g.name == "skirt_02").GetComponent<JiggleChain>());
            Assert.IsNull(_made.First(g => g.name == "skirt_side_02").GetComponent<JiggleChain>());
            var breast = _made.First(g => g.name == "breast_l").GetComponent<JiggleChain>();
            Assert.AreEqual(JiggleTailDirection.AwayFromParent, breast.tailDirection);
            Assert.AreEqual(0, JiggleSetup.AddChains(root, JiggleSetupOptions.Default())); // a second time adds nothing
        }
    }
}
