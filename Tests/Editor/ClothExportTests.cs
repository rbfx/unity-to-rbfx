using System.Collections.Generic;
using System.Linq;
using MagicaFakes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityToRebelFork.Editor;

namespace UnityToRebelFork
{
    /// <summary>The cloth section of the manifest: what is read from Magica Cloth 2 components (here stand-ins with its names) and written for the Toe engine.</summary>
    [TestFixture]
    public class ClothExportTests
    {
        private readonly List<GameObject> _made = new List<GameObject>();
        private string _typeName;

        [SetUp]
        public void PointAtTheStandIns()
        {
            _typeName = ClothExport.ClothTypeName;
            ClothExport.ClothTypeName = typeof(MagicaCloth).FullName;
        }

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

        /// <summary>A character with a pelvis and a skirt of three chains (each root with two bones below it) hanging from it.</summary>
        private (GameObject Root, GameObject Pelvis, List<Transform> Roots) Skirted()
        {
            var root = Make("Girl");
            var pelvis = Make("pelvis", root.transform, new Vector3(0f, 1f, 0f));
            var roots = new List<Transform>();
            for (int i = 0; i < 3; i++)
            {
                var parent = pelvis;
                for (int level = 1; level <= 3; level++)
                {
                    parent = Make($"skirt_{i}_0{level}", parent.transform, level == 1 ? new Vector3(i * 0.1f, 0f, 0f) : new Vector3(0f, -0.1f, 0f));
                    if (level == 1) roots.Add(parent.transform);
                }
            }
            return (root, pelvis, roots);
        }

        private static MagicaCloth AddCloth(GameObject host, List<Transform> roots, int connection = 2)
        {
            var cloth = host.AddComponent<MagicaCloth>();
            cloth.SerializeData.rootBones = roots;
            cloth.SerializeData.connectionMode = connection;
            return cloth;
        }

        private static JObject Collect(GameObject root, List<string> warnings = null) => ClothExport.Collect(root.transform, warnings == null ? (System.Action<string>)(_ => { }) : warnings.Add);

        [Test]
        public void ACharacterWithoutMagicaCloth_HasNoClothSection()
        {
            var (root, _, _) = Skirted();
            Assert.IsNull(Collect(root));
        }

        [Test]
        public void ABoneCloth_IsWrittenWithItsRootsInOrder_AndItsParameters()
        {
            var (root, pelvis, roots) = Skirted();
            var host = Make("Cloth_Skirt", root.transform);
            var cloth = AddCloth(host, roots);
            cloth.SerializeData.gravity = 3.5f;
            cloth.SerializeData.inertiaConstraint.depthInertia = 0.7f;
            cloth.SerializeData.tetherConstraint.distanceCompression = 0.3f;
            cloth.SerializeData.angleLimitConstraint.useAngleLimit = true;

            var json = Collect(root);
            var skirt = (JObject)json["cloths"][0];
            Assert.AreEqual("Cloth_Skirt", (string)skirt["name"]);
            Assert.AreEqual("BoneCloth", (string)skirt["type"]);
            CollectionAssert.AreEqual(new[] { "skirt_0_01", "skirt_1_01", "skirt_2_01" }, skirt["roots"].Select(t => (string)t).ToArray());
            Assert.AreEqual("SequentialLoopMesh", (string)skirt["connection"]);
            Assert.AreEqual(3.5f, (float)skirt["gravity"], 1e-6f);
            Assert.AreEqual(0.7f, (float)skirt["inertia"]["depthInertia"], 1e-6f);
            Assert.AreEqual(0.3f, (float)skirt["tetherCompression"], 1e-6f);
            Assert.IsTrue((bool)skirt["angleLimit"]["use"]);
            Assert.IsTrue((bool)skirt["inertia"]["movementSpeedLimit"]["use"]);
            Assert.AreEqual(5f, (float)skirt["inertia"]["movementSpeedLimit"]["value"], 1e-6f);
            Assert.AreEqual(0f, (float)skirt["gravityDirection"][0], 1e-6f);
            Assert.AreEqual(-1f, (float)skirt["gravityDirection"][1], 1e-6f);
            Assert.AreEqual(0.1f, (float)skirt["stabilizationTime"], 1e-6f);
        }

        [TestCase(0, "Line")]
        [TestCase(1, "AutomaticMesh")]
        [TestCase(2, "SequentialLoopMesh")]
        [TestCase(3, "SequentialNonLoopMesh")]
        public void TheConnectionModes_AreWrittenByName(int mode, string expected)
        {
            var (root, _, roots) = Skirted();
            AddCloth(Make("Cloth", root.transform), roots, mode);
            Assert.AreEqual(expected, (string)Collect(root)["cloths"][0]["connection"]);
        }

        [Test]
        public void ACurve_IsAConstant_OrSixteenSamplesMultipliedByItsValue()
        {
            var (root, _, roots) = Skirted();
            var cloth = AddCloth(Make("Cloth", root.transform), roots);
            cloth.SerializeData.damping.value = 0.1f;
            cloth.SerializeData.distanceConstraint.stiffness.useCurve = true;
            cloth.SerializeData.distanceConstraint.stiffness.value = 2f;
            cloth.SerializeData.distanceConstraint.stiffness.curve = AnimationCurve.Linear(0f, 1f, 1f, 0.5f);

            var json = Collect(root)["cloths"][0];
            Assert.AreEqual(JTokenType.Float, json["damping"].Type);
            Assert.AreEqual(0.1f, (float)json["damping"], 1e-6f);
            var curve = json["distanceStiffness"];
            Assert.AreEqual(16, curve["samples"].Count());
            Assert.AreEqual(2f, (float)curve["samples"][0], 1e-5f);
            Assert.AreEqual(1f, (float)curve["samples"][15], 1e-5f);
        }

        [Test]
        public void TheColliders_AreWrittenInTheSpaceOfTheirBone_WithTheScaleBaked_AndListedByTheCloth()
        {
            var (root, pelvis, roots) = Skirted();
            var thigh = Make("thigh_l", pelvis.transform);
            thigh.transform.localScale = new Vector3(2f, 2f, 2f);
            var capsule = thigh.AddComponent<MagicaCapsuleCollider>();
            capsule.direction = 1;
            capsule.center = new Vector3(0f, 0.1f, 0f);
            capsule.SetSize(new Vector3(0.05f, 0.05f, 0.4f));
            var sphere = pelvis.AddComponent<MagicaSphereCollider>();
            sphere.SetSize(new Vector3(0.12f, 0f, 0f));
            var cloth = AddCloth(Make("Cloth", root.transform), roots);
            cloth.SerializeData.colliderCollisionConstraint.colliderList.Add(capsule);
            cloth.SerializeData.colliderCollisionConstraint.colliderList.Add(sphere);

            var json = Collect(root);
            var colliders = json["colliders"].Cast<JObject>().ToDictionary(c => (string)c["bone"]);
            var thighJson = colliders["thigh_l"];
            Assert.AreEqual("Capsule", (string)thighJson["shape"]);
            Assert.AreEqual("Y", (string)thighJson["direction"]);
            Assert.AreEqual(0.1f, (float)thighJson["size"][0], 1e-5f);  // 0.05 x the scale 2
            Assert.AreEqual(0.8f, (float)thighJson["size"][2], 1e-5f);  // The length too.
            Assert.AreEqual(0.2f, (float)thighJson["center"][1], 1e-5f);
            Assert.AreEqual("Sphere", (string)colliders["pelvis"]["shape"]);
            Assert.AreEqual(0.12f, (float)colliders["pelvis"]["size"][0], 1e-5f);

            var listed = json["cloths"][0]["collision"]["colliders"].Select(t => (string)t).ToList();
            CollectionAssert.AreEquivalent(new[] { (string)thighJson["name"], (string)colliders["pelvis"]["name"] }, listed);
            Assert.AreEqual("Point", (string)json["cloths"][0]["collision"]["mode"]);
        }

        [Test]
        public void ACapsuleWithSeparateRadii_KeepsBoth()
        {
            var (root, pelvis, roots) = Skirted();
            var capsule = pelvis.AddComponent<MagicaCapsuleCollider>();
            capsule.radiusSeparation = true;
            capsule.SetSize(new Vector3(0.08f, 0.04f, 0.5f));
            AddCloth(Make("Cloth", root.transform), roots);
            var size = Collect(root)["colliders"][0]["size"];
            Assert.AreEqual(0.08f, (float)size[0], 1e-5f);
            Assert.AreEqual(0.04f, (float)size[1], 1e-5f);

            capsule.radiusSeparation = false;
            size = Collect(root)["colliders"][0]["size"];
            Assert.AreEqual(0.08f, (float)size[1], 1e-5f, "without separation the end radius is the start radius");
        }

        [Test]
        public void ACollider_ThatIsListedButNotBelowTheCharacter_IsWarnedAbout()
        {
            var (root, _, roots) = Skirted();
            var stranger = Make("stranger").AddComponent<MagicaSphereCollider>();
            var cloth = AddCloth(Make("Cloth", root.transform), roots);
            cloth.SerializeData.colliderCollisionConstraint.colliderList.Add(stranger);
            var warnings = new List<string>();
            var json = Collect(root, warnings);
            Assert.AreEqual(0, json["cloths"][0]["collision"]["colliders"].Count());
            Assert.AreEqual(1, warnings.Count);
            StringAssert.Contains("not exported", warnings[0]);
        }

        [Test]
        public void AMeshCloth_AndAClothWithoutRoots_AreLeftOut_WithWarnings()
        {
            var (root, _, roots) = Skirted();
            var mesh = AddCloth(Make("MeshCloth", root.transform), roots);
            mesh.SerializeData.clothType = 0;
            AddCloth(Make("Empty", root.transform), new List<Transform>());
            AddCloth(Make("Good", root.transform), roots);
            var warnings = new List<string>();
            var json = Collect(root, warnings);
            Assert.AreEqual(1, json["cloths"].Count());
            Assert.AreEqual("Good", (string)json["cloths"][0]["name"]);
            Assert.AreEqual(2, warnings.Count);
            StringAssert.Contains("Mesh Cloth", warnings[0]);
        }

        [Test]
        public void ADisabledCloth_IsLeftOut()
        {
            var (root, _, roots) = Skirted();
            var cloth = AddCloth(Make("Cloth", root.transform), roots);
            cloth.enabled = false;
            Assert.IsNull(Collect(root));
        }

        [Test]
        public void BonesPaintedInTheEditor_BecomeFixedAndInvalidBones()
        {
            var (root, _, roots) = Skirted();
            var host = Make("Cloth", root.transform);
            var cloth = AddCloth(host, roots);
            var middle = root.transform.Find("pelvis/skirt_0_01/skirt_0_02");
            var tip = root.transform.Find("pelvis/skirt_0_01/skirt_0_02/skirt_0_03");
            var selection = cloth.serializeData2.selectionData;
            selection.userEdit = true;
            selection.positions = new[] { host.transform.InverseTransformPoint(middle.position), host.transform.InverseTransformPoint(tip.position) };
            selection.attributes = new[] { new VertexAttribute { Value = 1 }, new VertexAttribute { Value = 0 } };

            var skirt = Collect(root)["cloths"][0];
            CollectionAssert.AreEqual(new[] { "skirt_0_02" }, skirt["fixed"].Select(t => (string)t).ToArray());
            CollectionAssert.AreEqual(new[] { "skirt_0_03" }, skirt["invalid"].Select(t => (string)t).ToArray());
        }

        [Test]
        public void NoPaintAttributes_AreWritten_WhenTheSelectionWasNotEditedByHand()
        {
            var (root, _, roots) = Skirted();
            AddCloth(Make("Cloth", root.transform), roots);
            var skirt = Collect(root)["cloths"][0];
            Assert.IsNull(skirt["fixed"]);
            Assert.IsNull(skirt["invalid"]);
        }

        [Test]
        public void TheManifestSection_IsValidJson_WithInvariantNumbers()
        {
            var old = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
                var (root, _, roots) = Skirted();
                AddCloth(Make("Cloth", root.transform), roots).SerializeData.gravity = 4.25f;
                string text = ClothExport.ToJson(Collect(root), "  ");
                StringAssert.Contains("\"gravity\": 4.25", text);
                Assert.AreEqual(4.25f, (float)JObject.Parse(text)["cloths"][0]["gravity"], 1e-6f);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = old;
            }
        }
    }
}
