using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityToRebelFork.Editor.Particles;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// Exports a Unity <see cref="ParticleSystem"/> as a particle graph effect (<c>Particle/&lt;prefab path&gt;/&lt;name&gt;.xml</c>): the settings are read by
    /// <see cref="ParticleSystemReader"/> and turned into nodes by <see cref="ParticleSystemConverter"/>. The material and the mesh of its renderer are
    /// exported too. What could not be converted is logged and kept in <see cref="ExportContext.EffectReports"/>.
    /// </summary>
    public class ParticleSystemExporter : ExporterBase<ParticleSystem>
    {
        private readonly Lazy<ExportOrchestrator> _orchestrator;

        public ParticleSystemExporter(Lazy<ExportOrchestrator> orchestrator, NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings)
            : base(nameCollisionResolver, context, settings)
        {
            _orchestrator = orchestrator;
        }

        public override string EvaluateResourcePath(ParticleSystem system)
        {
            // The prefab (or scene) the system is in, and where in it: the names of the objects from the root, with their positions among their siblings.
            string assetPath = AssetDatabase.GetAssetPath(system);
            if (string.IsNullOrEmpty(assetPath)) assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(system);
            if (string.IsNullOrEmpty(assetPath)) assetPath = system.gameObject.scene.path;
            if (string.IsNullOrEmpty(assetPath)) assetPath = "Loose";

            var names = new System.Collections.Generic.List<string>();
            for (var t = system.transform; t != null; t = t.parent)
                names.Insert(0, SafeFileName(t.name) + (t.parent == null ? "" : "." + t.GetSiblingIndex()));
            string folder = ReplaceExtension(GetRelPathFromAssetPath(assetPath), "");
            return "Particle/" + folder + "/" + string.Join("_", names) + ".xml";
        }

        /// <summary>
        /// Whether the system only exists to be driven by another one: a birth or death sub emitter of a system of the same hierarchy. Its particles are
        /// made by the effect of that system (as another layer of it), so it gets no emitter of its own.
        /// </summary>
        public static bool IsDrivenChild(ParticleSystem system)
        {
            foreach (var other in system.transform.root.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (other == system || !other.subEmitters.enabled) continue;
                var module = other.subEmitters;
                for (int i = 0; i < module.subEmittersCount; i++)
                {
                    if (module.GetSubEmitterSystem(i) != system) continue;
                    var type = module.GetSubEmitterType(i);
                    if (type == ParticleSystemSubEmitterType.Birth || type == ParticleSystemSubEmitterType.Death) return true;
                }
            }
            return false;
        }

        /// <summary>The settings of a system and of its sub emitters (each with the material and the mesh of its renderer, exported on the way).</summary>
        private ParticleSystemData ReadData(ParticleSystem system, System.Collections.Generic.HashSet<ParticleSystem> parents)
        {
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            string material = null, mesh = null;
            bool distortion = false;
            if (renderer != null)
            {
                var sharedMaterial = renderer.sharedMaterial;
                distortion = sharedMaterial != null && MaterialTextureRules.IsDistortion(sharedMaterial.shader.name, sharedMaterial.name);
                if (sharedMaterial != null && !distortion)
                {
                    ParticleShaders.Add(sharedMaterial.shader);
                    material = _orchestrator.Value.ScheduleExport(sharedMaterial);
                }
                if (renderer.renderMode == ParticleSystemRenderMode.Mesh && renderer.mesh != null)
                    mesh = _orchestrator.Value.ScheduleExport(renderer.mesh);
            }

            var data = ParticleSystemReader.Read(system, material, mesh);
            if (distortion)
            {
                // The material bends what is behind the particles with a normal map; Toe draws no such thing (yet), and a normal map is no color.
                data.Render = UnityToRebelFork.Editor.Particles.RenderMode.None;
                data.Unsupported.Add("the material '" + renderer.sharedMaterial.name + "' is a distortion: the particles are not drawn");
            }
            var module = system.subEmitters;
            if (!module.enabled || module.subEmittersCount == 0 || !parents.Add(system)) return data;
            for (int i = 0; i < module.subEmittersCount; i++)
            {
                var child = module.GetSubEmitterSystem(i);
                if (child == null) continue;
                if (parents.Contains(child))
                {
                    data.Unsupported.Add("a sub emitter is one of the systems that drive it");
                    continue;
                }
                var childData = ReadData(child, parents);
                // The sub emitter spawns at the particles of its parent: where its own object is does not matter, except as an offset of its shape.
                var local = system.transform.InverseTransformPoint(child.transform.position);
                if (local.sqrMagnitude > 1e-6f || Quaternion.Angle(system.transform.rotation, child.transform.rotation) > 0.1f)
                    childData.Unsupported.Add("the position and rotation of the sub emitter's object are not applied");
                data.SubEmitters.Add(new SubEmitterData
                {
                    Child = childData,
                    Type = (SubEmitterType)(int)module.GetSubEmitterType(i),
                    Inherit = (int)module.GetSubEmitterProperties(i) & 31,
                    Probability = module.GetSubEmitterEmitProbability(i),
                });
            }
            parents.Remove(system);
            return data;
        }

        public override IEnumerable Export(ParticleSystem system)
        {
            yield return system.name;

            var data = ReadData(system, new System.Collections.Generic.HashSet<ParticleSystem>());
            var converted = ParticleSystemConverter.Convert(data);
            string path = EvaluateResourcePath(system);
            foreach (var warning in converted.Warnings)
                Debug.LogWarning($"[ParticleSystemExporter] {path}: {warning}");
            context.EffectReports.Add(new EffectReport { Path = path, Name = system.name, Warnings = converted.Warnings.ToArray(), Nodes = converted.NodesUsed.ToArray() });

            using (var stream = _settings.CreateFile(path))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(converted.Xml);
        }
    }

    /// <summary>What the exporter wrote for one particle system, and what it could not convert.</summary>
    [Serializable]
    public class EffectReport
    {
        public string Path;
        public string Name;
        public string[] Warnings;
        public string[] Nodes;
    }
}
