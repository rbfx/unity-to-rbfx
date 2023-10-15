using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class MeshReference : IEquatable<MeshReference>
    {
        public struct MaterialAndTopology
        {
            public string Material;
            public UnityEngine.MeshTopology Topology;
        }

        public IList<MaterialAndTopology> Materials { get; } = new List<MaterialAndTopology>();

        public IList<LodLevel> Levels { get; } = new List<LodLevel>();

        public MeshReference(LODGroup lodGroup, ExportOrchestrator orchestrator)
        {
            var worldToLocalMatrix = lodGroup.transform.worldToLocalMatrix;
            var lods = lodGroup.GetLODs();
            foreach (var lod in lods)
            {
                Levels.Add(new LodLevel(lod, worldToLocalMatrix, orchestrator));
            }

            Dictionary<MaterialAndTopology, int> materialMap = new Dictionary<MaterialAndTopology, int>();
            foreach (var level in Levels)
            {
                foreach (var renderer in level.Renderers)
                {
                    if (renderer.Mesh != null)
                    {
                        for (var submeshIndex = 0; submeshIndex < renderer.Materials.Count; submeshIndex++)
                        {
                            var material = renderer.Materials[submeshIndex];
                            var key = new MaterialAndTopology
                                { Material = material, Topology = renderer.Mesh.GetTopology(submeshIndex) };
                            if (!materialMap.TryGetValue(key, out var index))
                            {
                                index = Materials.Count;
                                Materials.Add(key);
                                materialMap[key] = index;
                            }
                        }
                    }
                }
            }
        }

        public Mesh FirstMesh
        {
            get
            {
                foreach (var lodLevel in Levels)
                foreach (var renderer in lodLevel.Renderers)
                    if (renderer.Mesh != null)
                        return renderer.Mesh;

                return null;
            }
        }

        public static bool operator ==(MeshReference left, MeshReference right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MeshReference left, MeshReference right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MeshReference)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach (var lodLevel in Levels)
            {
                hashCode = Helpers.CombineHashCodes(hashCode, lodLevel);
            }
            return hashCode;
        }

        public bool Equals(MeshReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Levels.Count != other.Levels.Count) return false;
            for (var i = 0; i < Levels.Count; ++i)
                if (!Levels[i].Equals(other.Levels[i]))
                    return false;
            return true;
        }

        public class Renderer : IEquatable<Renderer>
        {
            public Mesh Mesh;
            public Matrix4x4 Transform;
            public IList<string> Materials { get; } = new List<string>();

            public Renderer(UnityEngine.Renderer renderer, Matrix4x4 worldToLocalMatrix, ExportOrchestrator orchestrator)
            {
                Transform = worldToLocalMatrix* renderer.localToWorldMatrix;
                if (renderer is MeshRenderer meshRenderer)
                {
                    Mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
                    if (Mesh != null)
                    {
                        var materials = meshRenderer.sharedMaterials;
                        for (int i = 0; i < Mesh.subMeshCount; ++i)
                        {
                            Materials.Add(orchestrator.ScheduleExport(materials.Length > i ? materials[i]: null));
                        }
                    }
                }
            }

            public static bool operator ==(Renderer left, Renderer right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Renderer left, Renderer right)
            {
                return !Equals(left, right);
            }


            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Renderer)obj);
            }

            public override int GetHashCode()
            {
                int meshHashCode = GetMeshHashCode(Mesh);
                var hash = Helpers.CombineHashCodes(Transform, meshHashCode);
                foreach (var material in Materials)
                {
                    hash = Helpers.CombineHashCodes(hash, material != null ? material.GetHashCode() : 0);
                }
                return hash;
            }

            public bool Equals(Renderer other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                if (Materials.Count != other.Materials.Count)
                    return false;
                for (var index = 0; index < Materials.Count; index++)
                {
                    if (Materials[index] != other.Materials[index])
                        return false;
                }

                return Transform == other.Transform && Equals(Mesh, other.Mesh);
            }
        }

        public static int GetMeshHashCode(UnityEngine.Mesh mesh)
        {
            if (mesh != null)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh, out string guid, out long localId))
                {
                    return Helpers.CombineHashCodes(guid, localId);
                }
            }

            return 0;
        }


        public class LodLevel : IEquatable<LodLevel>
        {
            public float ScreenRelativeTransitionHeight;

            public IList<Renderer> Renderers { get; } = new List<Renderer>();

            public LodLevel(UnityEngine.LOD lod, Matrix4x4 worldToLocalMatrix, ExportOrchestrator orchestrator)
            {
                ScreenRelativeTransitionHeight = lod.screenRelativeTransitionHeight;
                foreach (var renderer in lod.renderers.Where(_=>_!=null))
                {
                    var item = new Renderer(renderer, worldToLocalMatrix, orchestrator);
                    if (item.Mesh != null)
                    {
                        Renderers.Add(item);
                    }
                }
            }

            public static bool operator ==(LodLevel left, LodLevel right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(LodLevel left, LodLevel right)
            {
                return !Equals(left, right);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((LodLevel)obj);
            }

            public override int GetHashCode()
            {
                var hash = ScreenRelativeTransitionHeight.GetHashCode();
                foreach (var renderer in Renderers) hash = Helpers.CombineHashCodes(hash, renderer);
                return hash;
            }

            public bool Equals(LodLevel other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                if (!ScreenRelativeTransitionHeight.Equals(other.ScreenRelativeTransitionHeight)) return false;
                for (var i = 0; i < Renderers.Count; ++i)
                    if (!Renderers[i].Equals(other.Renderers[i]))
                        return false;
                return true;
            }
        }
    }
}