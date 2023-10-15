using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using static UnityToRebelFork.Editor.MeshReference;

namespace UnityToRebelFork.Editor
{
    public class LODGroupSource : AbstractMeshSource, IMeshSource
    {
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();

        private readonly List<Geometry> _geometries = new List<Geometry>();
        private readonly List<Lod> _lods = new List<Lod>();
        private readonly List<Vector2> _texCoords0 = new List<Vector2>();
        private readonly List<Vector2> _texCoords1 = new List<Vector2>();
        private readonly List<Vector2> _texCoords2 = new List<Vector2>();
        private readonly List<Vector2> _texCoords3 = new List<Vector2>();
        private readonly List<Vector4> _tangents = new List<Vector4>();
        private readonly List<Color32> _colors = new List<Color32>();

        public LODGroupSource(MeshReference meshReference)
        {
            var materialToSubmesh = new Dictionary<MeshReference.MaterialAndTopology, int>();
            var rendererToSubmeshMappings = new Dictionary<MeshInstanceKey, MeshMapping>();

            var lods = meshReference.Levels;
            _lods = lods.Select(_ => new Lod(_)).ToList();

            for (var matIndex = 0; matIndex < meshReference.Materials.Count; matIndex++)
            {
                var material = meshReference.Materials[matIndex];
                materialToSubmesh.Add(material, matIndex);
                _geometries.Add(new Geometry(_lods, material.Topology));
            }

            for (var lodIndex = 0; lodIndex < lods.Count; lodIndex++)
            {
                var lod = lods[lodIndex];
                foreach (var renderer in lod.Renderers.Where(_ => _.Mesh != null))
                {
                    //var transform = renderer.transform.localToWorldMatrix * worldToLocalMatrix;
                    //var transform = renderer.localToWorldMatrix * worldToLocalMatrix;
                    var transform = renderer.Transform;
                    var mesh = renderer.Mesh;
                    if (mesh != null)
                    {
                        var key = new MeshInstanceKey { mesh = mesh, transform = transform };
                        if (!rendererToSubmeshMappings.TryGetValue(key, out var meshMapping))
                        {
                            meshMapping = BuildMapping(null, mesh, transform);
                            rendererToSubmeshMappings[key] = meshMapping;
                        }

                        for (var subIndex = 0; subIndex < mesh.subMeshCount; ++subIndex)
                        {
                            var material = renderer.Materials[subIndex];
                            var topology = mesh.GetTopology(subIndex);
                            var submeshKey = new MeshReference.MaterialAndTopology { Material = material, Topology = topology };
                            var geometry = _geometries[materialToSubmesh[submeshKey]];
                            geometry.AddReference(lodIndex,
                                new SubmeshReference { Mesh = meshMapping, Submesh = subIndex });
                        }
                    }
                }
            }

            if (_vertices.Count > 0)
            {
                var min = _vertices[0];
                var max = _vertices[0];
                foreach (var originalPosition in _vertices)
                {
                    var p = originalPosition;
                    if (p.x < min.x) min.x = p.x;
                    if (p.y < min.y) min.y = p.y;
                    if (p.z < min.z) min.z = p.z;
                    if (p.x > max.x) max.x = p.x;
                    if (p.y > max.y) max.y = p.y;
                    if (p.z > max.z) max.z = p.z;
                }

                var diag = max - min;
                var unitySize = Math.Max(Math.Max(diag.x, diag.y), diag.z);
                var urhoSize = Vector3.Dot(diag, new Vector3(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f));
                foreach (var lod in _lods) lod.SetSizeFactor(unitySize / (urhoSize + 1e-6f));
            }
        }


        public override IList<Vector3> Vertices => _vertices;
        public override IList<Vector3> Normals => _normals;
        public override IList<Vector2> TexCoords0 => _texCoords0;
        public override IList<Vector2> TexCoords1 => _texCoords1;
        public override IList<Vector2> TexCoords2 => _texCoords2;
        public override IList<Vector2> TexCoords3 => _texCoords3;
        public override IList<Vector4> Tangents => _tangents;
        public override IList<Color32> Colors => _colors;

        public override int SubMeshCount => _geometries.Count;

        public override IMeshGeometry GetGeomtery(int subMeshIndex)
        {
            return _geometries[subMeshIndex];
        }

        private MeshMapping BuildMapping(UnityEngine.Renderer renderer, Mesh mesh, Matrix4x4 transform)
        {
            var buildMapping = new MeshMapping(renderer, mesh, transform);
            var startIndex = Vertices.Count;

            var meshSource = buildMapping.Source;
            var expectedTargetCount = _vertices.Count;
            AddData(_vertices, expectedTargetCount, TransformPos(meshSource.Vertices, transform),
                meshSource.Vertices.Count, new Vector3(0, 0, 0));
            AddData(_normals, expectedTargetCount, TransformNorm(meshSource.Normals, transform),
                meshSource.Vertices.Count, new Vector3(0, 1, 0));
            AddData(_tangents, expectedTargetCount, TransformTangent(meshSource.Tangents, transform),
                meshSource.Vertices.Count, new Vector4(1, 0, 0, 1));
            AddData(_colors, expectedTargetCount, meshSource.Colors, meshSource.Vertices.Count,
                new Color32(255, 255, 255, 255));
            AddData(_texCoords0, expectedTargetCount, meshSource.TexCoords0, meshSource.Vertices.Count,
                new Vector2(0, 0));
            AddData(_texCoords1, expectedTargetCount, meshSource.TexCoords1, meshSource.Vertices.Count,
                new Vector2(0, 0));
            AddData(_texCoords2, expectedTargetCount, meshSource.TexCoords2, meshSource.Vertices.Count,
                new Vector2(0, 0));
            AddData(_texCoords3, expectedTargetCount, meshSource.TexCoords3, meshSource.Vertices.Count,
                new Vector2(0, 0));

            for (var i = 0; i < mesh.subMeshCount; ++i)
                buildMapping.AddSubmeshIndices(startIndex, meshSource.GetGeomtery(i).GetIndices(0));

            return buildMapping;
        }

        private IList<Vector3> TransformPos(IList<Vector3> meshSourceVertices, Matrix4x4 transform)
        {
            var res = new Vector3[meshSourceVertices.Count];
            for (var index = 0; index < meshSourceVertices.Count; index++)
            {
                var pos = meshSourceVertices[index];
                res[index] = transform.MultiplyPoint(pos);
            }

            return res;
        }

        private IList<Vector3> TransformNorm(IList<Vector3> meshSourceVertices, Matrix4x4 transform)
        {
            var res = new Vector3[meshSourceVertices.Count];
            for (var index = 0; index < meshSourceVertices.Count; index++)
            {
                var pos = meshSourceVertices[index];
                res[index] = transform.MultiplyVector(pos);
            }

            return res;
        }

        private IList<Vector4> TransformTangent(IList<Vector4> meshSourceVertices, Matrix4x4 transform)
        {
            var res = new Vector4[meshSourceVertices.Count];
            for (var index = 0; index < meshSourceVertices.Count; index++)
            {
                var pos = meshSourceVertices[index];
                var t = transform.MultiplyVector(new Vector3(pos.x, pos.y, pos.z));
                res[index] = new Vector4(t.x, t.y, t.z, pos.w);
            }

            return res;
        }

        private void AddData<T>(List<T> targetContainer, int expectedTargetCount, IList<T> sourceContainer,
            int expectedCount, T defaultValue)
        {
            if (targetContainer.Count == 0 && sourceContainer.Count == 0)
                return;
            while (targetContainer.Count < expectedTargetCount) targetContainer.Add(defaultValue);

            if (sourceContainer.Count == 0)
                for (var i = 0; i < expectedCount; i++)
                    targetContainer.Add(defaultValue);
            else
                targetContainer.AddRange(sourceContainer);
        }


        private struct MeshInstanceKey
        {
            public Matrix4x4 transform;
            public Mesh mesh;
        }

        private class MeshMapping
        {
            public readonly List<List<int>> Indices = new List<List<int>>();

            public MeshMapping(UnityEngine.Renderer renderer, Mesh mesh, Matrix4x4 transform)
            {
                Transform = transform;
                Source = new MeshSource(mesh, renderer as SkinnedMeshRenderer);
            }

            public MeshSource Source { get; }

            public Matrix4x4 Transform { get; }

            public void AddSubmeshIndices(int startIndex, IEnumerable<int> getIndices)
            {
                Indices.Add(getIndices.Select(_ => _ + startIndex).ToList());
            }
        }

        private class SubmeshReference
        {
            public MeshMapping Mesh;
            public int Submesh;
        }

        private class Geometry : IMeshGeometry
        {
            private readonly List<Lod> _lods;
            private readonly List<List<int>> _refs;

            public Geometry(List<Lod> lods, MeshTopology topology)
            {
                _lods = lods;
                _refs = lods.Select(_ => new List<int>()).ToList();
                Topology = topology;
            }

            public Bounds? Bounds => null;

            public int NumLods
            {
                get
                {
                    if (_lods.Count == 0)
                        return 0;
                    if (_lods[_lods.Count - 1].ScreenRelativeTransitionHeight < 1e-6f)
                        return _lods.Count;
                    return _lods.Count + 1;
                }
            }

            public MeshTopology Topology { get; }

            public void AddReference(int lodIndex, SubmeshReference reference)
            {
                _refs[lodIndex].AddRange(reference.Mesh.Indices[reference.Submesh]);
            }

            public IList<int> GetIndices(int lod)
            {
                if (lod >= _refs.Count)
                    return Array.Empty<int>();
                return _refs[lod];
            }

            public float GetLodDistance(int lod)
            {
                if (lod == 0)
                    return 0;
                return _lods[lod - 1].GetDistance();
            }
        }

        private class Lod
        {
            private readonly MeshReference.LodLevel _lod;
            private float _sizeFactor = 1;

            public Lod(MeshReference.LodLevel lod)
            {
                _lod = lod;
            }

            public float ScreenRelativeTransitionHeight => _lod.ScreenRelativeTransitionHeight;

            public float GetDistance()
            {
                return _sizeFactor * 1.0f / Math.Max(_lod.ScreenRelativeTransitionHeight, 1e-6f);
            }

            public void SetSizeFactor(float size)
            {
                _sizeFactor = Math.Max(1e-6f, size);
            }
        }
    }
    public class MeshSource : AbstractMeshSource, IMeshSource
    {
        private readonly Mesh _mesh;
        private readonly SkinnedMeshRenderer _skin;

        public MeshSource(Mesh mesh, SkinnedMeshRenderer skin = null)
        {
            _mesh = mesh;
            _skin = skin;

            MorphTargets = new List<IMorphTarget>(mesh.blendShapeCount);
            if (mesh.blendShapeCount > 0)
            {
                Debug.LogWarning(mesh.name + " has blendshapes");
            }
            for (int i = 0; i < mesh.blendShapeCount; ++i)
            {
                var numFrames = mesh.GetBlendShapeFrameCount(i);
                if (numFrames == 1)
                {
                    MorphTargets.Add(new MorphTarget(mesh.GetBlendShapeName(i), mesh, i, 0));
                }
                else
                {
                    for (int frame = 0; frame < numFrames; ++frame)
                    {
                        MorphTargets.Add(new MorphTarget(mesh.GetBlendShapeName(i) + "_" + frame, mesh, i, frame));
                    }
                }
            }
        }

        public override int BonesCount
        {
            get
            {
                if (_skin == null)
                    return 0;
                return _skin.bones.Length;
            }
        }

        public override IList<Vector3> Vertices => _mesh.vertices;

        public override IList<Vector3> Normals => _mesh.normals;
        public override IList<Color32> Colors => _mesh.colors32;
        public override IList<Vector4> Tangents => _mesh.tangents;
        public override BoneWeight[] BoneWeights => _mesh.boneWeights;
        public override IList<Vector2> TexCoords0 => _mesh.uv;
        public override IList<Vector2> TexCoords1 => _mesh.uv2;
        public override IList<Vector2> TexCoords2 => _mesh.uv3;
        public override IList<Vector2> TexCoords3 => _mesh.uv4;
        public override IList<IMorphTarget> MorphTargets { get; }

        public override int SubMeshCount => _mesh.subMeshCount;

        public override Transform GetBoneTransform(int index)
        {
            return _skin != null ? _skin.bones[index] : null;
        }

        public override int? GetBoneParent(int index)
        {
            if (_skin != null)
            {
                var skinBone = _skin.bones[index];
                if (skinBone == null)
                    return null;
                var boneParent = skinBone.parent;
                if (boneParent == null)
                    return null;
                for (var i = 0; i < _skin.bones.Length; ++i)
                    if (_skin.bones[i] == boneParent)
                        return i;
                return null;
            }

            return null;
        }

        public override Matrix4x4 GetBoneBindPose(int index)
        {
            if (_skin == null)
                return base.GetBoneBindPose(index);
            return _skin.sharedMesh.bindposes[index];
        }

        public override IMeshGeometry GetGeomtery(int subMeshIndex)
        {
            return new Geometry(_mesh, subMeshIndex);
        }
        public class MorphTarget : IMorphTarget
        {
            public MorphTarget(string name, Mesh mesh, int targetIndex, int frameIndex)
            {
                Name = name;
                var numVerts = mesh.vertices.Length;
                var vertices = new Vector3[numVerts];
                var normals = new Vector3[numVerts];
                var tangents = new Vector3[numVerts];
                mesh.GetBlendShapeFrameVertices(targetIndex, frameIndex, vertices, normals, tangents);
                Vertices = vertices;
                Normals = normals;
                Tangents = tangents;
            }

            public string Name { get; set; }
            public IList<Vector3> Vertices { get; }
            public IList<Vector3> Normals { get; set; }
            public IList<Vector3> Tangents { get; set; }
        }

        private class Geometry : IMeshGeometry
        {
            private readonly Mesh _mesh;
            private readonly int _submesh;

            public Geometry(Mesh mesh, int submesh)
            {
                _mesh = mesh;
                _submesh = submesh;
                Bounds = _mesh.GetSubMesh(_submesh).bounds;
            }

            public Bounds? Bounds { get; }

            public int NumLods => 1;

            public MeshTopology Topology => _mesh.GetSubMesh(_submesh).topology;

            public IList<int> GetIndices(int lod)
            {
                if (lod == 0)
                    return _mesh.GetIndices(_submesh);
                return Array.Empty<int>();
            }

            public float GetLodDistance(int lod)
            {
                return 0;
            }
        }
    }
}