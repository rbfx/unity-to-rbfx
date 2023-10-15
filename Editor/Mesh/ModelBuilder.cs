using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class ModelBuilder
    {
        private readonly ExportSettings settings;
        private const int MASK_POSITION = 0x1;
        private const int MASK_NORMAL = 0x2;
        private const int MASK_COLOR = 0x4;
        private const int MASK_TANGENT = 0x80;

        public const uint Magic2 = 0x32444d55;

        public ModelBuilder(ExportSettings settings)
        {
            this.settings = settings;
        }

        private IEnumerable<int> GetBoneVertices(BoneWeight[] boneWeights, int boneIndex, float threshold = 0.01f)
        {
            if (boneWeights == null)
                yield break;
            for (var index = 0; index < boneWeights.Length; index++)
            {
                var boneWeight = boneWeights[index];
                var useVertex = boneWeight.boneIndex0 == boneIndex && boneWeight.weight0 >= threshold
                                || boneWeight.boneIndex1 == boneIndex && boneWeight.weight1 >= threshold
                                || boneWeight.boneIndex2 == boneIndex && boneWeight.weight2 >= threshold
                                || boneWeight.boneIndex3 == boneIndex && boneWeight.weight3 >= threshold;
                if (useVertex) yield return index;
            }
        }

        public string DecorateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            if (!settings.ASCIIOnly)
                return name;

            return Uri.EscapeDataString(name);
        }

        public void WriteMesh(BinaryWriter writer, IMeshSource mesh)
        {
            float minX, minY, minZ;
            float maxX, maxY, maxZ;
            maxX = maxY = maxZ = float.MinValue;
            minX = minY = minZ = float.MaxValue;

            var indexBuffer = new List<int>(1024);
            writer.Write(Magic2);
            var positions = mesh.Vertices;
            {
                // Vertex buffer
                uint numVertexBuffers = 1;
                writer.Write(numVertexBuffers);
                for (var vbIndex = 0; vbIndex < numVertexBuffers; ++vbIndex)
                {
                    var elements = WriteVertexBufferHeader(writer, mesh, positions);

                    var morphableVertexRangeStartIndex = 0;
                    var morphableVertexCount = 0;
                    if (mesh.MorphTargets.Count > 0)
                    {
                        morphableVertexCount = positions.Count;
                    }
                    writer.Write(morphableVertexRangeStartIndex);
                    writer.Write(morphableVertexCount);
                    for (var index = 0; index < positions.Count; ++index)
                        for (var i = 0; i < elements.Count; ++i)
                            elements[i].Write(writer, index);
                    for (var subMeshIndex = 0; subMeshIndex < mesh.SubMeshCount; ++subMeshIndex)
                    {
                        var meshGeometry = mesh.GetGeomtery(subMeshIndex);
                        for (var lodIndex = 0; lodIndex < meshGeometry.NumLods; lodIndex++)
                        {
                            var indices = meshGeometry.GetIndices(lodIndex);
                            indexBuffer.AddRange(indices);
                            foreach (var index in indices)
                            {
                                if (index > positions.Count)
                                    throw new InvalidOperationException();
                            }
                        }
                    }

                    for (var i = 0; i < positions.Count; ++i)
                    {
                        if (minX > positions[i].x)
                            minX = positions[i].x;
                        if (minY > positions[i].y)
                            minY = positions[i].y;
                        if (minZ > positions[i].z)
                            minZ = positions[i].z;
                        if (maxX < positions[i].x)
                            maxX = positions[i].x;
                        if (maxY < positions[i].y)
                            maxY = positions[i].y;
                        if (maxZ < positions[i].z)
                            maxZ = positions[i].z;
                    }
                }
            }

            {
                // Index buffer
                uint numIndexBuffers = 1;
                writer.Write(numIndexBuffers);
                writer.Write(indexBuffer.Count);
                if (!indexBuffer.Any(_ => _ > 65535))
                {
                    writer.Write(2);
                    for (var i = 0; i < indexBuffer.Count; ++i)
                        writer.Write((ushort)indexBuffer[i]);
                }
                else
                {
                    writer.Write(4);
                    for (var i = 0; i < indexBuffer.Count; ++i)
                        writer.Write((uint)indexBuffer[i]);
                }
            }

            writer.Write(mesh.SubMeshCount);
            var indexOffset = 0;
            for (var subMeshIndex = 0; subMeshIndex < mesh.SubMeshCount; ++subMeshIndex)
            {
                var numberOfBoneMappingEntries = 0;
                writer.Write(numberOfBoneMappingEntries);

                var geomtery = mesh.GetGeomtery(subMeshIndex);

                var numberOfLODLevels = geomtery.NumLods;
                writer.Write(numberOfLODLevels);
                for (var lodIndex = 0; lodIndex < numberOfLODLevels; lodIndex++)
                {
                    var lodIndices = geomtery.GetIndices(lodIndex);
                    writer.Write((float)geomtery.GetLodDistance(lodIndex));
                    switch (geomtery.Topology)
                    {
                        case MeshTopology.Lines:
                            writer.Write((uint)PrimitiveType.LINE_LIST);
                            break;
                        case MeshTopology.LineStrip:
                            writer.Write((uint)PrimitiveType.LINE_STRIP);
                            break;
                        case MeshTopology.Points:
                            writer.Write((uint)PrimitiveType.POINT_LIST);
                            break;
                        default:
                            writer.Write((uint)PrimitiveType.TRIANGLE_LIST);
                            break;
                    }
                    writer.Write((uint)0); //vb
                    writer.Write((uint)0); //ib
                    writer.Write((uint)indexOffset); //Draw range: index start
                    var indicesPerLod = lodIndices.Count;
                    writer.Write((uint)indicesPerLod); //Draw range: index count
                    indexOffset += indicesPerLod;
                }
            }
            if (indexOffset != indexBuffer.Count)
                throw new InvalidOperationException();

            var morphTargets = mesh.MorphTargets;
            writer.Write(morphTargets.Count);
            foreach (var morphTarget in morphTargets)
            {
                writer.WriteStringSZ(morphTarget.Name); // Name of morph

                writer.Write(1); // Number of affected vertex buffers

                writer.Write(0); // Vertex buffer index, starting from 0

                var mask = 0;
                if (morphTarget.Vertices.Any(_ => _ != Vector3.zero))
                    mask |= MASK_POSITION;
                if (morphTarget.Normals.Any(_ => _ != Vector3.zero))
                    mask |= MASK_NORMAL;
                if (morphTarget.Tangents.Any(_ => _ != Vector3.zero))
                    mask |= MASK_TANGENT;
                writer.Write(mask); // Vertex element mask for morph data. Only positions, normals & tangents are supported.

                var indices = new List<int>();

                for (var index = 0; index < morphTarget.Vertices.Count; index++)
                {
                    if (morphTarget.Vertices[index] != Vector3.zero)
                    {
                        indices.Add(index);
                        continue;
                    }
                    if (morphTarget.Normals[index] != Vector3.zero)
                    {
                        indices.Add(index);
                        continue;
                    }
                    if (morphTarget.Tangents[index] != Vector3.zero)
                    {
                        indices.Add(index);
                        continue;
                    }
                }

                writer.Write(indices.Count); // Vertex count
                foreach (var index in indices)
                {
                    writer.Write(index);
                    if (0 != (mask & MASK_POSITION))
                    {
                        writer.Write(morphTarget.Vertices[index]);
                    }
                    if (0 != (mask & MASK_NORMAL))
                    {
                        writer.Write(morphTarget.Normals[index]);
                    }
                    if (0 != (mask & MASK_TANGENT))
                    {
                        writer.Write(morphTarget.Tangents[index]);
                    }
                }
            }

            var bones = BuildBones(mesh);
            var numOfBones = bones.Length;
            writer.Write(numOfBones);
            var boneIndex = 0;
            foreach (var bone in bones)
            {
                writer.WriteStringSZ(DecorateName(bone.name));
                writer.Write(bone.parent); //Parent
                writer.Write(bone.actualPos);
                writer.Write(bone.actualRot);
                writer.Write(bone.actualScale);

                var d = new[]
                {
                    bone.binding.m00, bone.binding.m01, bone.binding.m02, bone.binding.m03,
                    bone.binding.m10, bone.binding.m11, bone.binding.m12, bone.binding.m13,
                    bone.binding.m20, bone.binding.m21, bone.binding.m22, bone.binding.m23
                };
                foreach (var v in d) writer.Write(v);

                using (var e = GetBoneVertices(mesh.BoneWeights, boneIndex).GetEnumerator())
                {
                    if (!e.MoveNext())
                    {
                        writer.Write((byte)3);
                        //R
                        writer.Write(0.1f);
                        //BBox
                        writer.Write(new Vector3(-0.1f, -0.1f, -0.1f));
                        writer.Write(new Vector3(0.1f, 0.1f, 0.1f));
                    }
                    else
                    {
                        var binding = bone.binding;
                        //binding = binding.inverse;
                        var center = binding.MultiplyPoint(positions[e.Current]);
                        var min = center;
                        var max = center;

                        while (e.MoveNext())
                        {
                            var originalPosition = positions[e.Current];
                            var p = binding.MultiplyPoint(originalPosition);
                            if (p.x < min.x) min.x = p.x;
                            if (p.y < min.y) min.y = p.y;
                            if (p.z < min.z) min.z = p.z;
                            if (p.x > max.x) max.x = p.x;
                            if (p.y > max.y) max.y = p.y;
                            if (p.z > max.z) max.z = p.z;
                        }

                        writer.Write((byte)3);
                        //R
                        writer.Write(MathF.Max(max.magnitude, min.magnitude));
                        //BBox
                        writer.Write(min);
                        writer.Write(max);
                    }
                }

                ++boneIndex;
            }

            if (minX > maxX)
            {
                writer.Write(-0.1f);
                writer.Write(-0.1f);
                writer.Write(-0.1f);
                writer.Write(0.1f);
                writer.Write(0.1f);
                writer.Write(0.1f);
            }
            else
            {
                writer.Write(minX);
                writer.Write(minY);
                writer.Write(minZ);
                writer.Write(maxX);
                writer.Write(maxY);
                writer.Write(maxZ);
            }
        }
        public class Urho3DBone
        {
            public string name;
            public int parent;
            public Vector3 actualPos = Vector3.zero;
            public Quaternion actualRot = Quaternion.identity;
            public Vector3 actualScale = Vector3.one;
            public Matrix4x4 binding = Matrix4x4.identity;

            public override string ToString()
            {
                return name + " (Parent: " + parent + ")";
            }
        }

        private Urho3DBone[] BuildBones(IMeshSource skinnedMeshRenderer)
        {
            if (skinnedMeshRenderer == null || skinnedMeshRenderer.BonesCount == 0)
                return new Urho3DBone[0];
            //var unityBones = skinnedMeshRenderer.bones;
            var bones = new Urho3DBone[skinnedMeshRenderer.BonesCount];
            for (var index = 0; index < bones.Length; index++)
            {
                var bone = new Urho3DBone();
                var unityBone = skinnedMeshRenderer.GetBoneTransform(index);
                var parentIndex = skinnedMeshRenderer.GetBoneParent(index);
                if (parentIndex.HasValue)
                    bone.parent = parentIndex.Value;
                else
                    bone.parent = index;

                bone.name = string.IsNullOrWhiteSpace(unityBone?.name) ? "bone" + index : unityBone.name;
                //if (bone.parent != 0)
                //{
                if (unityBone != null)
                {
                    bone.actualPos = unityBone.localPosition;
                    bone.actualRot = unityBone.localRotation;
                    bone.actualScale = unityBone.localScale;
                }
                //}
                //else
                //{
                //    bone.actualPos = unityBone.position;
                //    bone.actualRot = unityBone.rotation;
                //    bone.actualScale = unityBone.lossyScale;
                //}

                bone.binding = skinnedMeshRenderer.GetBoneBindPose(index);
                bones[index] = bone;
            }

            return bones;
        }

        private List<MeshStreamWriter> WriteVertexBufferHeader(BinaryWriter writer, IMeshSource mesh, IList<Vector3> positions)
        {
            var normals = mesh.Normals;
            var colors = settings.VertexColors ? mesh.Colors : Array.Empty<Color32>();
            var tangents = mesh.Tangents;
            var boneWeights = mesh.BoneWeights;
            var uvs = mesh.TexCoords0;
            var uvs2 = mesh.TexCoords1;
            var uvs3 = mesh.TexCoords2;
            var uvs4 = mesh.TexCoords3;

            writer.Write(positions.Count);
            var elements = new List<MeshStreamWriter>();

            //if (positions.Count > 0) We should always have positions!
            elements.Add(new MeshVector3Stream(positions, VertexElementSemantic.SEM_POSITION));

            if (normals.Count > 0)
            {
                //var n = new Vector3[normals.Count];
                //for (var index = 0; index < normals.Count; index++)
                //{
                //    n[index] = normals[index].normalized;
                //}
                //elements.Add(new MeshVector3Stream(n, VertexElementSemantic.SEM_NORMAL));
                elements.Add(new MeshVector3Stream(normals, VertexElementSemantic.SEM_NORMAL));
            }

            if (boneWeights.Length > 0)
            {
                var indices = new Vector4[boneWeights.Length];
                var weights = new Vector4[boneWeights.Length];
                for (var i = 0; i < boneWeights.Length; ++i)
                {
                    indices[i] = new Vector4(boneWeights[i].boneIndex0, boneWeights[i].boneIndex1,
                        boneWeights[i].boneIndex2, boneWeights[i].boneIndex3);
                    weights[i] = new Vector4(boneWeights[i].weight0, boneWeights[i].weight1,
                        boneWeights[i].weight2, boneWeights[i].weight3);
                }

                elements.Add(new MeshVector4Stream(weights, VertexElementSemantic.SEM_BLENDWEIGHTS));
                elements.Add(new MeshUByte4Stream(indices, VertexElementSemantic.SEM_BLENDINDICES));
            }

            if (colors.Count > 0) elements.Add(new MeshColor32Stream(colors, VertexElementSemantic.SEM_COLOR));
            if (tangents.Count > 0)
                elements.Add(new MeshVector4Stream(FlipW(tangents), VertexElementSemantic.SEM_TANGENT));
            if (uvs.Count > 0)
                elements.Add(new MeshUVStream(uvs, VertexElementSemantic.SEM_TEXCOORD));
            if (uvs2.Count > 0)
                elements.Add(new MeshUVStream(uvs2, VertexElementSemantic.SEM_TEXCOORD, 1));
            if (uvs3.Count > 0)
                elements.Add(new MeshUVStream(uvs3, VertexElementSemantic.SEM_TEXCOORD, 2));
            if (uvs4.Count > 0)
                elements.Add(new MeshUVStream(uvs4, VertexElementSemantic.SEM_TEXCOORD, 3));
            writer.Write(elements.Count);
            for (var i = 0; i < elements.Count; ++i)
                writer.Write(elements[i].Element);
            return elements;
        }

        private Vector4[] FlipW(IList<Vector4> tangents)
        {
            var res = new Vector4[tangents.Count];
            for (var index = 0; index < tangents.Count; index++)
            {
                var tangent = tangents[index];
                res[index] = new Vector4(tangent.x, tangent.y, tangent.z, -tangent.w);
            }

            return res;
        }

        public enum PrimitiveType
        {
            TRIANGLE_LIST = 0,
            LINE_LIST,
            POINT_LIST,
            TRIANGLE_STRIP,
            LINE_STRIP,
            TRIANGLE_FAN
        }

        internal abstract class MeshStreamWriter
        {
            public int Element;
            public abstract void Write(BinaryWriter writer, int index);
        }

        internal class MeshVector3Stream : MeshStreamWriter
        {
            private readonly IList<Vector3> positions;

            public MeshVector3Stream(IList<Vector3> positions, VertexElementSemantic sem, int index = 0)
            {
                this.positions = positions;
                Element = (int)VertexElementType.TYPE_VECTOR3 | ((int)sem << 8) | (index << 16);
            }

            public override void Write(BinaryWriter writer, int index)
            {
                writer.Write(positions[index].x);
                writer.Write(positions[index].y);
                writer.Write(positions[index].z);
            }
        }

        internal class MeshUVStream : MeshStreamWriter
        {
            private readonly IList<Vector2> positions;

            public MeshUVStream(IList<Vector2> positions, VertexElementSemantic sem, int index = 0)
            {
                this.positions = positions;
                Element = (int)VertexElementType.TYPE_VECTOR2 | ((int)sem << 8) | (index << 16);
            }

            public override void Write(BinaryWriter writer, int index)
            {
                writer.Write(positions[index].x);
                writer.Write(1.0f - positions[index].y);
            }
        }

        internal class MeshVector2Stream : MeshStreamWriter
        {
            private readonly Vector2[] positions;

            public MeshVector2Stream(Vector2[] positions, VertexElementSemantic sem, int index = 0)
            {
                this.positions = positions;
                Element = (int)VertexElementType.TYPE_VECTOR2 | ((int)sem << 8) | (index << 16);
            }

            public override void Write(BinaryWriter writer, int index)
            {
                writer.Write(positions[index].x);
                writer.Write(positions[index].y);
            }
        }

        internal class MeshVector4Stream : MeshStreamWriter
        {
            private readonly Vector4[] positions;

            public MeshVector4Stream(Vector4[] positions, VertexElementSemantic sem, int index = 0)
            {
                this.positions = positions;
                Element = (int)VertexElementType.TYPE_VECTOR4 | ((int)sem << 8) | (index << 16);
            }

            public override void Write(BinaryWriter writer, int index)
            {
                writer.Write(positions[index].x);
                writer.Write(positions[index].y);
                writer.Write(positions[index].z);
                writer.Write(positions[index].w);
            }
        }

        internal class MeshColor32Stream : MeshStreamWriter
        {
            private readonly IList<Color32> colors;

            public MeshColor32Stream(IList<Color32> colors, VertexElementSemantic sem, int index = 0)
            {
                this.colors = colors;
                Element = (int)VertexElementType.TYPE_UBYTE4_NORM | ((int)sem << 8) | (index << 16);
            }

            public override void Write(BinaryWriter writer, int index)
            {
                writer.Write(colors[index].r);
                writer.Write(colors[index].g);
                writer.Write(colors[index].b);
                writer.Write(colors[index].a);
            }
        }

        internal class MeshUByte4Stream : MeshStreamWriter
        {
            private readonly Vector4[] positions;

            public MeshUByte4Stream(Vector4[] positions, VertexElementSemantic sem, int index = 0)
            {
                this.positions = positions;
                Element = (int)VertexElementType.TYPE_UBYTE4 | ((int)sem << 8) | (index << 16);
            }

            public override void Write(BinaryWriter writer, int index)
            {
                writer.Write((byte)positions[index].x);
                writer.Write((byte)positions[index].y);
                writer.Write((byte)positions[index].z);
                writer.Write((byte)positions[index].w);
            }
        }

        public class BoneMap
        {
            private readonly Transform _rootBone;
            private readonly Dictionary<Transform, int> _boneIndices = new Dictionary<Transform, int>();
            private readonly List<Transform> _bones = new List<Transform>();
            private readonly List<int> _parents = new List<int>();
            private readonly List<Matrix4x4> _bindPoses = new List<Matrix4x4>();

            public BoneMap(Transform rootBone)
            {
                _rootBone = rootBone;
            }

            public Matrix4x4[] BindPoses => _bindPoses.ToArray();
            public int Count => _bones.Count;

            public Transform this[int index]
            {
                get
                {
                    return _bones[index];
                }
            }

            public int GetParentIndex(int bone)
            {
                if (bone < 0)
                    return -1;
                return _parents[bone];
            }

            public int AddBone(Transform bone, SkinnedMeshRenderer skinnedMeshRenderer, int boneIndex = -1)
            {
                if (_boneIndices.TryGetValue(bone, out var index))
                    return index;
                int parentIndex = bone != _rootBone ? AddBone(bone.parent, skinnedMeshRenderer, Array.IndexOf(skinnedMeshRenderer.bones, bone.parent)) : -1;
                index = _bones.Count;
                _parents.Add(parentIndex);
                _bones.Add(bone);
                _boneIndices[bone] = index;
                //var skinLocalToMesh = skinnedMeshRenderer.rootBone.localToWorldMatrix;
                if (boneIndex >= 0)
                {
                    var bindpose = skinnedMeshRenderer.sharedMesh.bindposes[boneIndex];
                    //TODO: multiply by difference in bone root bones!
                    _bindPoses.Add(bindpose);
                }
                else
                {
                    _bindPoses.Add(Matrix4x4.identity);
                }
                return index;
            }

            public void AddBones(SkinnedMeshRenderer skinRenderer)
            {
                var bones = skinRenderer.bones;
                for (var i = 0; i < skinRenderer.bones.Length; i++)
                {
                    AddBone(bones[i], skinRenderer, i);
                }

            }

            public Transform[] CreateBoneStructure(GameObject prefab)
            {
                Transform[] newBones = new Transform[_bones.Count];

                //First pass: create bone structre
                for (var index = 0; index < _bones.Count; index++)
                {
                    var transform = _bones[index];
                    var newBone = new GameObject(transform.name).transform;
                    newBones[index] = newBone;
                }
                //Second pass: build hierarchy
                for (var index = 0; index < _bones.Count; index++)
                {
                    var newBone = newBones[index];
                    if (_parents[index] >= 0)
                    {
                        newBone.parent = newBones[_parents[index]];
                    }
                    else
                    {
                        newBone.parent = prefab.transform;
                    }
                }
                //Third pass: place bones
                bool[] bonePlaced = new bool[_bones.Count];
                for (var index = 0; index < _bones.Count; index++)
                {
                    PlaceBone(index, newBones, bonePlaced);
                }

                return newBones;
            }

            private void PlaceBone(int index, Transform[] newBones, bool[] bonePlaced)
            {
                if (bonePlaced[index])
                    return;
                bonePlaced[index] = true;
                if (_parents[index] >= 0 && _parents[index] != index)
                    PlaceBone(_parents[index], newBones, bonePlaced);
                var transform = _bones[index];
                var newBone = newBones[index];
                newBone.position = transform.position;
                newBone.rotation = transform.rotation;
                newBone.localScale = transform.localScale;

            }
        }

        public static Transform GetCommonRoot(IEnumerable<Transform> objects)
        {
            using (var e = objects.GetEnumerator())
            {
                if (!e.MoveNext())
                    return null;
                var root = e.Current;
                while (e.MoveNext())
                {
                    root = GetCommonRoot(root, e.Current);
                    if (root == null)
                        return null;
                }

                return root;
            }
        }

        public static IEnumerable<Transform> WalkToRoot(Transform a)
        {
            while (a != null)
            {
                yield return a;
                a = a.parent;
            }
        }

        public static Transform GetCommonRoot(Transform a, Transform b)
        {
            if (a == null && b == null)
                return null;
            if (a == null)
                return b;
            if (b == null)
                return a;

            var knownParents = new HashSet<Transform>(WalkToRoot(a));
            foreach (var transform in WalkToRoot(b))
            {
                if (knownParents.Contains(transform))
                    return transform;
            }

            return null;
        }

        private class SkeletonExtractor
        {
            private readonly float _boneWeightThreshold;

            public SkeletonExtractor(float boneWeightThreshold)
            {
                _boneWeightThreshold = boneWeightThreshold;
            }

            public void Extract(GameObject gameObject, int exportOption)
            {
                this.allSkinRenderers = gameObject.GetComponents<SkinnedMeshRenderer>()
                    .Concat(gameObject.GetComponentsInChildren<SkinnedMeshRenderer>()).ToList();
                if (allSkinRenderers.Count == 0)
                    return;
                this.rootObject = GetCommonRoot(allSkinRenderers.Select(_ => _.transform));
                var newRootBone = GetCommonRoot(allSkinRenderers.Select(_ => _.rootBone));
                if (newRootBone == null)
                {
                    Debug.LogError("Can't find common root bone from " + string.Join(", ", allSkinRenderers.Select(_ => _.rootBone?.name)));
                    return;
                }
                this.boneMap = new BoneMap(newRootBone);
                foreach (var skinRenderer in allSkinRenderers)
                {
                    boneMap.AddBones(skinRenderer);
                }

                skeletonMesh = new Mesh();
                skeletonMesh.name = newRootBone.name;
                this.bindposes = boneMap.BindPoses;
                skeletonMesh.bindposes = bindposes;
                this.boneWeights = new List<BoneWeight>();
                this.positions = new List<Vector3>();
                this.indices = new List<int>();

                MeshTopology topology = MeshTopology.Triangles;
                switch (exportOption)
                {
                    case 0:
                        break;
                    case 1:
                        BuildLines();
                        topology = MeshTopology.Lines;
                        break;
                    case 2:
                        BuildBoxes();
                        break;

                }

                skeletonMesh.vertices = positions.ToArray();
                //skeletonMesh.uv = new Vector2[] { Vector2.zero, new Vector2(1, 0), new Vector2(0, 2) };
                skeletonMesh.boneWeights = boneWeights.ToArray();
                //skeletonMesh.subMeshCount = 1;
                skeletonMesh.SetIndices(indices, topology, 0);
                if (skeletonMesh.normals == null || skeletonMesh.normals.Length == 0)
                    skeletonMesh.RecalculateNormals();
                if (skeletonMesh.tangents == null || skeletonMesh.tangents.Length == 0)
                    skeletonMesh.RecalculateTangents();
                skeletonMesh.RecalculateBounds();
                //skeletonMesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Count, MeshTopology.Lines), MeshUpdateFlags.Default);

                prefab = new GameObject();
                prefab.transform.position = gameObject.transform.position;
                prefab.transform.rotation = gameObject.transform.rotation;
                prefab.transform.localScale = gameObject.transform.localScale;
                this.geo = new GameObject(newRootBone.name);
                geo.transform.parent = prefab.transform;
                geo.transform.localPosition = Vector3.zero;
                geo.transform.localRotation = Quaternion.identity;
                var skinnedMeshRenderer = geo.AddComponent<SkinnedMeshRenderer>();
                this.boneStructure = boneMap.CreateBoneStructure(prefab);
                if (exportOption == 3)
                {
                    CreateHitBoxes();
                }
                skinnedMeshRenderer.sharedMesh = skeletonMesh;
                skinnedMeshRenderer.rootBone = boneStructure[0];
                skinnedMeshRenderer.bones = boneStructure;
                skinnedMeshRenderer.sharedMaterial = allSkinRenderers[0].sharedMaterial;
            }

            private Bounds?[] CreateBoxes()
            {
                var bounds = new Bounds?[boneMap.Count];
                for (int i = 0; i < boneMap.Count; ++i)
                {
                    var originalBone = boneMap[i];
                    var bboxMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                    var bboxMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                    foreach (var skinRenderer in allSkinRenderers)
                    {
                        var boneIndex = System.Array.IndexOf(skinRenderer.bones, originalBone);
                        if (boneIndex < 0)
                            continue;
                        var mesh = skinRenderer.sharedMesh;
                        var bindPose = mesh.bindposes[boneIndex];
                        var vertices = mesh.vertices;
                        var weights = mesh.boneWeights;
                        //var b = new Bounds();
                        for (int vertIndex = 0; vertIndex < vertices.Length; ++vertIndex)
                        {
                            var w = weights[vertIndex];
                            if ((w.boneIndex0 == boneIndex && w.weight0 > _boneWeightThreshold)
                                || (w.boneIndex1 == boneIndex && w.weight1 > _boneWeightThreshold)
                                || (w.boneIndex2 == boneIndex && w.weight2 > _boneWeightThreshold)
                                || (w.boneIndex3 == boneIndex && w.weight3 > _boneWeightThreshold))
                            {
                                var localPos = bindPose * vertices[vertIndex];
                                bboxMin = Vector3.Min(bboxMin, localPos);
                                bboxMax = Vector3.Max(bboxMax, localPos);
                            }
                        }
                    }

                    if (bboxMax.x > bboxMin.x)
                    {
                        bounds[i] = new Bounds((bboxMax + bboxMin) * 0.5f, bboxMax - bboxMin);
                    }
                }
                return bounds;
            }

            private void CreateHitBoxes()
            {
                var boxes = CreateBoxes();
                for (int i = 0; i < boneStructure.Length; ++i)
                {
                    var bone = boneStructure[i];

                    if (boxes[i] != null)
                    {
                        var hb = new GameObject(bone.name + "_hit_box");
                        hb.transform.parent = bone;
                        hb.transform.localRotation = Quaternion.identity;
                        hb.transform.localPosition = boxes[i].Value.center;
                        hb.transform.localScale = boxes[i].Value.size;
                        var bc = hb.AddComponent<BoxCollider>();
                    }
                }
            }

            private void BuildBoxes()
            {
                var boxes = CreateBoxes();
                for (int i = 0; i < boxes.Length; ++i)
                {
                    if (boxes[i] != null)
                    {
                        var bindpose = bindposes[i].inverse;
                        BuildBox(i, boxes[i].Value, bindpose);
                    }
                }
            }

            private void BuildBox(int boneIndex, Bounds box, Matrix4x4 boneTransform)
            {
                var min = box.min;
                var max = box.max;
                var offset = positions.Count;
                positions.Add(boneTransform * new Vector3(min.x, min.y, min.z));
                positions.Add(boneTransform * new Vector3(max.x, min.y, min.z));
                positions.Add(boneTransform * new Vector3(min.x, max.y, min.z));
                positions.Add(boneTransform * new Vector3(max.x, max.y, min.z));
                positions.Add(boneTransform * new Vector3(min.x, min.y, max.z));
                positions.Add(boneTransform * new Vector3(max.x, min.y, max.z));
                positions.Add(boneTransform * new Vector3(min.x, max.y, max.z));
                positions.Add(boneTransform * new Vector3(max.x, max.y, max.z));
                while (boneWeights.Count < positions.Count)
                {
                    boneWeights.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 });
                }
                indices.Add(offset + 0);
                indices.Add(offset + 3);
                indices.Add(offset + 1);

                indices.Add(offset + 0);
                indices.Add(offset + 2);
                indices.Add(offset + 3);

                indices.Add(offset + 4);
                indices.Add(offset + 5);
                indices.Add(offset + 7);

                indices.Add(offset + 4);
                indices.Add(offset + 7);
                indices.Add(offset + 6);

                indices.Add(offset + 0);
                indices.Add(offset + 1);
                indices.Add(offset + 5);

                indices.Add(offset + 0);
                indices.Add(offset + 5);
                indices.Add(offset + 4);

                indices.Add(offset + 2);
                indices.Add(offset + 7);
                indices.Add(offset + 3);

                indices.Add(offset + 2);
                indices.Add(offset + 6);
                indices.Add(offset + 7);

                indices.Add(offset + 1);
                indices.Add(offset + 3);
                indices.Add(offset + 7);

                indices.Add(offset + 1);
                indices.Add(offset + 7);
                indices.Add(offset + 5);

                indices.Add(offset + 0);
                indices.Add(offset + 4);
                indices.Add(offset + 6);

                indices.Add(offset + 0);
                indices.Add(offset + 6);
                indices.Add(offset + 2);
            }

            private void BuildLines()
            {
                for (var i = 0; i < bindposes.Length; i++)
                {
                    var bindpose = bindposes[i].inverse;
                    positions.Add(new Vector3(bindpose.m03, bindpose.m13, bindpose.m23));
                    boneWeights.Add(new BoneWeight() { boneIndex0 = i, weight0 = 1 });
                    var p = boneMap.GetParentIndex(i);
                    if (p >= 0)
                    {
                        indices.Add(p);
                        indices.Add(i);
                    }
                }
            }
            public GameObject prefab;
            public Mesh skeletonMesh;
            private Matrix4x4[] bindposes;
            private Transform[] boneStructure;
            private GameObject geo;
            private BoneMap boneMap;
            private List<BoneWeight> boneWeights;
            private List<Vector3> positions;
            private List<int> indices;
            private List<SkinnedMeshRenderer> allSkinRenderers;
            private Transform rootObject;
        }

        public enum VertexElementSemantic
        {
            SEM_POSITION = 0,
            SEM_NORMAL,
            SEM_BINORMAL,
            SEM_TANGENT,
            SEM_TEXCOORD,
            SEM_COLOR,
            SEM_BLENDWEIGHTS,
            SEM_BLENDINDICES,
            SEM_OBJECTINDEX,
            MAX_VERTEX_ELEMENT_SEMANTICS
        }

        public enum VertexElementType
        {
            TYPE_INT = 0,
            TYPE_FLOAT,
            TYPE_VECTOR2,
            TYPE_VECTOR3,
            TYPE_VECTOR4,
            TYPE_UBYTE4,
            TYPE_UBYTE4_NORM,
            MAX_VERTEX_ELEMENT_TYPES
        }
    }
}