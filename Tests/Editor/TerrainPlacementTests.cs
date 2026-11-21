using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityToRebelFork.Editor;

namespace UnityToRebelFork
{
    /// <summary>A Unity terrain stands by its world position only (it ignores rotation and scale) and rbfx's is centred on its node: where the exported nodes put it.</summary>
    [TestFixture]
    public class TerrainPlacementTests
    {
        private readonly List<GameObject> _made = new List<GameObject>();
        private readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();

        [TearDown]
        public void Clean()
        {
            foreach (var go in _made)
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            foreach (var asset in _assets)
                if (asset != null) UnityEngine.Object.DestroyImmediate(asset);
            _made.Clear();
            _assets.Clear();
        }

        /// <summary>An exporter that takes the terrain data and names its height map, so that the visitor goes on to place the terrain.</summary>
        private sealed class HeightMapExporter : IExporter
        {
            public bool CanExport(object asset) => asset is TerrainData;
            public string EvaluateResourcePath(object asset) => "Terrain/test_height.png";
            public IEnumerable Export(object asset) { yield break; }
        }

        private GameObject MakeTerrain(float size = 64f)
        {
            var data = new TerrainData { heightmapResolution = 33, size = new Vector3(size, 10f, size) };
            _assets.Add(data);
            var go = UnityEngine.Terrain.CreateTerrainGameObject(data);
            _made.Add(go);
            return go;
        }

        private static PrefabVisitor Visitor()
        {
            var settings = ScriptableObject.CreateInstance<ExportSettings>();
            var orchestrator = new ExportOrchestrator(new IExporter[] { new HeightMapExporter() });
            return new PrefabVisitor(new Lazy<ExportOrchestrator>(() => orchestrator), settings);
        }

        [Test]
        public void TheNodeOfATerrainCancelsTheRotationAndScaleOfItsParents()
        {
            var parent = new GameObject("Parent");
            _made.Add(parent);
            parent.transform.rotation = Quaternion.Euler(0f, 30f, 0f);
            parent.transform.localScale = new Vector3(2f, 2f, 2f);
            var terrain = new GameObject("Terrain");
            terrain.transform.SetParent(parent.transform, false);
            terrain.transform.localRotation = Quaternion.Euler(90f, 0f, 180f);
            terrain.transform.localScale = new Vector3(2f, 1f, 3f);

            PrefabVisitor.TerrainNodePlacement(terrain.transform, out var rotation, out var scale);

            // The world transform of the node is parent * node: a translation.
            Assert.AreEqual(0f, Quaternion.Angle(parent.transform.rotation * rotation, Quaternion.identity), 1e-3f);
            Assert.AreEqual(1f, scale.x * parent.transform.lossyScale.x, 1e-4f);
            Assert.AreEqual(1f, scale.y * parent.transform.lossyScale.y, 1e-4f);
            Assert.AreEqual(1f, scale.z * parent.transform.lossyScale.z, 1e-4f);
        }

        [Test]
        public void ATerrainAtTheRootNeedsNoRotationOrScale()
        {
            var terrain = new GameObject("Terrain");
            _made.Add(terrain);
            terrain.transform.rotation = Quaternion.Euler(10f, 20f, 30f);
            terrain.transform.localScale = new Vector3(3f, 3f, 3f);

            PrefabVisitor.TerrainNodePlacement(terrain.transform, out var rotation, out var scale);

            Assert.AreEqual(Quaternion.identity, rotation);
            Assert.AreEqual(Vector3.one, scale);
        }

        [Test]
        public void ARotatedTerrainObjectIsExportedWithoutTheRotationAndItsCentreChildIsAtHalfTheSize()
        {
            var go = MakeTerrain(64f);
            go.transform.position = new Vector3(-128.2f, 2.47f, -94f);
            go.transform.rotation = Quaternion.Euler(180f, 90f, 0f);

            var node = Visitor().Visit(go, true);

            Assert.AreEqual(go.transform.localPosition.x, node.Position.x, 1e-4f);
            Assert.AreEqual(0f, Quaternion.Angle(node.Rotation, Quaternion.identity), 1e-3f);
            Assert.AreEqual(Vector3.one, node.Scale);
            var child = node.Children.Single();
            Assert.AreEqual(32f, child.Position.x, 1e-4f);
            Assert.AreEqual(0f, child.Position.y, 1e-4f);
            Assert.AreEqual(32f, child.Position.z, 1e-4f);
        }

        [Test]
        public void TheTerrainCollidersShapeAndBodyAreOnTheNodeThatHasTheTerrainComponent()
        {
            var go = MakeTerrain();
            Assert.IsNotNull(go.GetComponent<TerrainCollider>());

            var node = Visitor().Visit(go, true);

            var child = node.Children.Single();
            Assert.IsTrue(child.Components.Any(_ => _ is UnityToRebelFork.Editor.Terrain));
            Assert.IsTrue(child.Components.Any(_ => _ is UnityToRebelFork.Editor.CollisionShape shape && shape.ShapeType == ShapeType.Terrain));
            Assert.IsTrue(child.Components.Any(_ => _ is GenericComponent generic && generic.TypeName == "RigidBody"));
            Assert.IsFalse(node.Components.Any(_ => _ is UnityToRebelFork.Editor.CollisionShape));
            Assert.IsFalse(node.Components.Any(_ => _ is GenericComponent generic && generic.TypeName == "RigidBody"));
        }
    }
}
