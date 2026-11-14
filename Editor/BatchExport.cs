using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using Rbfx.LightInject;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// Command line export of a character and the animations baked onto it, and of props:
    /// <code>
    /// Unity -batchmode -quit -projectPath &lt;project&gt; -executeMethod UnityToRebelFork.Editor.BatchExport.Run
    ///       -exportOut &lt;Data folder&gt; [-exportCharacter &lt;Assets/...prefab&gt; [-exportClips &lt;list file&gt;]] [-exportProps &lt;list file&gt;]
    ///       [-exportScene &lt;Assets/...unity&gt;]
    /// </code>
    /// A scene is written as &lt;its asset path&gt;.scene with everything it uses; its prefab instances are written inline (see
    /// <see cref="ExportSettings.InlinePrefabs"/>) and [ExportComponent] behaviours become components.
    /// A list file names one asset path per line (# starts a comment). The character's enabled skinned meshes are exported with
    /// their materials and textures, the clips (FBX files or animation clips) are baked onto its skeleton, and
    /// Characters/&lt;name&gt;.json lists what was written. Each prop (a prefab, or the root object of a model file) is exported
    /// as a prefab at the path of its asset (see <see cref="PrefabExporter"/>), with the meshes, materials and textures it uses.
    /// The editor exits with 1 if anything failed.
    /// </summary>
    public static class BatchExport
    {
        /// <summary>Exports for rbfx: plain rbfx content, no Toe-only files (whatever -exportTarget says).</summary>
        public static void RunRbfx() => RunTarget(ExportTarget.Rbfx);

        /// <summary>Exports for Toe: also writes the humanoid map, effect reports and, with -normalize -toe, normalized humanoid clips.</summary>
        public static void RunToe() => RunTarget(ExportTarget.Toe);

        /// <summary>The target comes from -exportTarget rbfx|toe (rbfx if it is missing).</summary>
        public static void Run() => RunTarget(null);

        private static void RunTarget(ExportTarget? forced)
        {
            int exitCode = 1;
            try
            {
                exitCode = Export(Environment.GetCommandLineArgs(), forced);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            Debug.Log($"[BatchExport] finished with exit code {exitCode}");
            EditorApplication.Exit(exitCode);
        }

        private static string Arg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }

        public static int Export(string[] args) => Export(args, null);

        public static int Export(string[] args, ExportTarget? forcedTarget)
        {
            var targetArg = Arg(args, "-exportTarget");
            if (targetArg != null && !string.Equals(targetArg, "toe", StringComparison.OrdinalIgnoreCase) && !string.Equals(targetArg, "rbfx", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("-exportTarget is rbfx or toe, not " + targetArg);
            var target = forcedTarget ?? (string.Equals(targetArg, "toe", StringComparison.OrdinalIgnoreCase) ? ExportTarget.Toe : ExportTarget.Rbfx);
            bool normalize = Array.Exists(args, a => string.Equals(a, "-normalize", StringComparison.OrdinalIgnoreCase));
            var toeCli = Arg(args, "-toe");
            var outDir = Arg(args, "-exportOut") ?? throw new ArgumentException("-exportOut is missing");
            var characterPath = Arg(args, "-exportCharacter");
            var clipList = Arg(args, "-exportClips");
            var propList = Arg(args, "-exportProps");
            var scenePath = Arg(args, "-exportScene");
            var effectList = Arg(args, "-exportEffects");
            if (characterPath == null && propList == null && scenePath == null && effectList == null)
                throw new ArgumentException("Give -exportCharacter, -exportProps, -exportEffects or -exportScene.");
            // Numbers in the written files must not depend on the machine's culture.
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var character = characterPath == null ? null
                : AssetDatabase.LoadAssetAtPath<GameObject>(characterPath) ?? throw new FileNotFoundException("No prefab at " + characterPath);

            var settings = ScriptableObject.CreateInstance<ExportSettings>();
            // Opening a scene unloads unused assets, which would destroy these settings.
            settings.hideFlags = HideFlags.DontUnloadUnusedAsset;
            settings.path = outDir;
            settings.Target = target;
            settings.NormalizeHumanoidClips = normalize && target == ExportTarget.Toe;
            settings.ToeCliPath = toeCli;
            settings.InlinePrefabs = scenePath != null;
            settings.SceneEnvironment = scenePath == null; // The game lights its levels (sky, zone and fog) itself.
            var context = new ExportContext(character) { HumanoidRig = character };
            // A character without a humanoid avatar plays generic clips: sample them on its rig, which puts the armature above the root bone into it.
            context.BakeClipsOnRig = character != null && HumanoidMapExporter.Build(character) == null;
            var container = new Rbfx.LightInject.ServiceContainer();
            RebelForkInstaller.Install(container, settings, context);
            var orchestrator = container.GetInstance<ExportOrchestrator>();

            // The meshes the character shows (Synty prefabs carry every variant of a pack, with all but one switched off).
            var models = new List<ModelEntry>();
            string rootBone = null;
            foreach (var skin in character != null ? character.GetComponentsInChildren<SkinnedMeshRenderer>(true) : Array.Empty<SkinnedMeshRenderer>())
            {
                if (!skin.enabled || !IsActive(skin.transform, character.transform) || skin.sharedMesh == null) continue;
                var model = orchestrator.ScheduleExport(skin.sharedMesh);
                var materials = skin.sharedMaterials.Select(m => orchestrator.ScheduleExport(m)).ToList();
                // Custom shaders (Synty's) do not translate to rbfx materials: the game can build one from these textures.
                var albedo = skin.sharedMaterials.Select(m => ExportTexture(orchestrator, m, AlbedoNames)).ToList();
                var normal = skin.sharedMaterials.Select(m => ExportTexture(orchestrator, m, NormalNames)).ToList();
                models.Add(new ModelEntry { Model = model, Materials = materials, Albedo = albedo, Normal = normal });
                rootBone ??= skin.rootBone != null ? skin.rootBone.name : null;
                Debug.Log($"[BatchExport] model {skin.name} -> {model}");
            }

            var animations = new List<(string Name, string Path)>();
            if (!string.IsNullOrEmpty(clipList))
            {
                foreach (var raw in System.IO.File.ReadAllLines(clipList))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    var clips = AssetDatabase.LoadAllAssetsAtPath(line).OfType<AnimationClip>()
                        .Where(c => !c.name.StartsWith("__preview__")).ToList();
                    if (clips.Count == 0) Debug.LogError("[BatchExport] no animation clip in " + line);
                    foreach (var clip in clips)
                    {
                        var path = orchestrator.ScheduleExport(clip);
                        animations.Add((HumanoidClipBaker.ClipName(clip), path));
                        Debug.Log($"[BatchExport] clip {line} -> {path}");
                    }
                }
            }

            // Props are Unity prefabs (or a model file's root object) and come out as rbfx prefabs, with their meshes, materials and textures.
            int missingProps = 0;
            if (!string.IsNullOrEmpty(propList))
            {
                foreach (var raw in System.IO.File.ReadAllLines(propList))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(line);
                    if (prefab == null)
                    {
                        Debug.LogError("[BatchExport] no prefab at " + line);
                        missingProps++;
                        continue;
                    }
                    if (!HasRenderer(prefab))
                    {
                        Debug.LogError("[BatchExport] " + line + " has no mesh renderer");
                        missingProps++;
                        continue;
                    }
                    var path = orchestrator.ScheduleExport(prefab);
                    if (path == null)
                    {
                        Debug.LogError("[BatchExport] cannot export " + line);
                        missingProps++;
                        continue;
                    }
                    Debug.Log($"[BatchExport] prop {line} -> {path}");
                }
            }

            // Effects are prefabs with particle systems: each system is written as a particle graph effect and the prefab plays it with emitters.
            int missingEffects = 0;
            if (!string.IsNullOrEmpty(effectList))
            {
                foreach (var raw in System.IO.File.ReadAllLines(effectList))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(line);
                    var path = prefab == null ? null : orchestrator.ScheduleExport(prefab);
                    if (path == null)
                    {
                        Debug.LogError("[BatchExport] cannot export the effect " + line);
                        missingEffects++;
                        continue;
                    }
                    Debug.Log($"[BatchExport] effect {line} -> {path}");
                }
            }

            if (scenePath != null)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var path = orchestrator.ScheduleExport(scene);
                if (path == null) throw new InvalidOperationException("Cannot export the scene " + scenePath);
                Debug.Log($"[BatchExport] scene {scenePath} -> {path}");
            }

            int failures = orchestrator.RunToCompletion();
            bool toe = settings.Target == ExportTarget.Toe;
            var humanBones = toe && character != null ? HumanoidMapExporter.Build(character) : null;
            if (humanBones != null)
                foreach (var entry in models) HumanoidMapExporter.Write(settings, entry.Model, humanBones);
            List<string> normalized = null;
            if (humanBones != null && settings.NormalizeHumanoidClips && animations.Count > 0 && models.Count > 0)
                normalized = NormalizeClips(settings, models[0].Model, animations, ref failures);
            if (character != null) WriteManifest(settings, character.name, models, animations, rootBone, humanBones, normalized);
            if (toe && !string.IsNullOrEmpty(effectList)) WriteEffectReport(settings, context, System.IO.Path.GetFileNameWithoutExtension(effectList));
            bool missing = (character != null && models.Count == 0) || missingProps > 0 || missingEffects > 0;
            return failures == 0 && !missing ? 0 : 1;
        }

        /// <summary>True if the prefab draws something: an enabled mesh renderer or skinned mesh renderer, or a LOD group.</summary>
        private static bool HasRenderer(GameObject prefab)
            => prefab.GetComponentsInChildren<MeshRenderer>(true).Any(r => r.enabled)
               || prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(r => r.enabled)
               || prefab.GetComponentsInChildren<LODGroup>(true).Length > 0;

        private static readonly string[] AlbedoNames = { "_Albedo_Map", "_BaseMap", "_MainTex", "_BaseColorMap", "_Albedo" };
        private static readonly string[] NormalNames = { "_Normal_Map", "_BumpMap", "_NormalMap" };

        private sealed class ModelEntry
        {
            public string Model;
            public List<string> Materials;
            public List<string> Albedo;
            public List<string> Normal;
        }

        private static string ExportTexture(ExportOrchestrator orchestrator, UnityEngine.Material material, string[] names)
        {
            if (material == null) return null;
            foreach (var name in names)
            {
                if (material.HasProperty(name) && material.GetTexture(name) is UnityEngine.Texture texture && texture != null)
                    return orchestrator.ScheduleExport(texture);
            }
            return null;
        }

        private static bool IsActive(Transform t, Transform root)
        {
            for (var p = t; p != null; p = p.parent)
            {
                if (!p.gameObject.activeSelf) return false;
                if (p == root) break;
            }
            return true;
        }

        /// <summary>Effects/&lt;name&gt;.json: for every particle system that was written, its file, the nodes it uses and what could not be converted.</summary>
        private static void WriteEffectReport(ExportSettings settings, ExportContext context, string name)
        {
            string Q(string s) => "\"" + (s ?? string.Empty).Replace("\\", "/").Replace("\"", "\\\"") + "\"";
            var json = new StringBuilder();
            json.AppendLine("{ \"effects\": [");
            for (int i = 0; i < context.EffectReports.Count; i++)
            {
                var r = context.EffectReports[i];
                json.Append($"  {{ \"path\": {Q(r.Path)}, \"name\": {Q(r.Name)}, \"nodes\": [{string.Join(", ", r.Nodes.Select(Q))}], \"warnings\": [{string.Join(", ", r.Warnings.Select(Q))}] }}");
                json.AppendLine(i + 1 < context.EffectReports.Count ? "," : string.Empty);
            }
            json.AppendLine("] }");
            using (var stream = settings.CreateFile($"Effects/{ExporterBase.SafeFileName(name)}.json"))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(json.ToString());
        }

        /// <summary>
        /// Runs <c>dotnet toe.dll --data &lt;out&gt; anim normalize_all Animations/&lt;rig&gt; --model &lt;model&gt; --out &lt;out&gt;/Animations/Humanoid/&lt;rig&gt;</c> on the clips that were
        /// baked on the character, and returns the normalized clips (resource paths), or null if nothing was written.
        /// </summary>
        private static List<string> NormalizeClips(ExportSettings settings, string modelResource, List<(string Name, string Path)> animations, ref int failures)
        {
            if (string.IsNullOrEmpty(settings.ToeCliPath) || !File.Exists(settings.ToeCliPath))
            {
                Debug.LogWarning("[BatchExport] -normalize needs -toe <path of toe.dll>; the clips are not normalized.");
                return null;
            }
            var rigFolder = Path.GetDirectoryName(animations[0].Path.Replace('\\', '/'))?.Replace('\\', '/');
            if (string.IsNullOrEmpty(rigFolder)) return null;
            var outResource = "Animations/Humanoid/" + Path.GetFileName(rigFolder);
            var outFolder = Path.Combine(settings.path, outResource);
            Directory.CreateDirectory(outFolder);
            // The full model path (with .mdl): a bare name is looked up in the character manifest, which is written after this.
            var model = modelResource.Replace('\\', '/');
            var start = new System.Diagnostics.ProcessStartInfo("dotnet")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            foreach (var arg in new[] { settings.ToeCliPath, "--data", settings.path, "anim", "normalize_all", rigFolder, "--model", model, "--out", outFolder })
                start.ArgumentList.Add(arg);
            using (var process = System.Diagnostics.Process.Start(start))
            {
                var errorTask = process.StandardError.ReadToEndAsync();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                string error = errorTask.Result;
                if (output.Length > 0) Debug.Log("[BatchExport] toe: " + output);
                if (process.ExitCode != 0)
                {
                    Debug.LogError($"[BatchExport] toe anim normalize_all failed ({process.ExitCode}): {error}");
                    failures++;
                    return null;
                }
            }
            return Directory.GetFiles(outFolder, "*.ani").Select(f => outResource + "/" + Path.GetFileName(f)).OrderBy(f => f, StringComparer.Ordinal).ToList();
        }

        private static void WriteManifest(ExportSettings settings, string name, List<ModelEntry> models,
            List<(string Name, string Path)> animations, string rootBone, Dictionary<string, string> humanBones, List<string> normalized)
        {
            string Q(string s) => "\"" + (s ?? string.Empty).Replace("\\", "/").Replace("\"", "\\\"") + "\"";
            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"name\": {Q(name)},");
            json.AppendLine($"  \"rootBone\": {Q(rootBone)},");
            if (humanBones != null) json.AppendLine("  \"humanBones\": " + HumanoidMapExporter.ToJson(humanBones, "  ") + ",");
            json.AppendLine("  \"models\": [");
            for (int i = 0; i < models.Count; i++)
            {
                var m = models[i];
                json.Append($"    {{ \"model\": {Q(m.Model)}, \"materials\": [{string.Join(", ", m.Materials.Select(Q))}], " +
                            $"\"albedo\": [{string.Join(", ", m.Albedo.Select(Q))}], \"normal\": [{string.Join(", ", m.Normal.Select(Q))}] }}");
                json.AppendLine(i + 1 < models.Count ? "," : string.Empty);
            }
            json.AppendLine("  ],");
            json.AppendLine("  \"animations\": {");
            for (int i = 0; i < animations.Count; i++)
            {
                json.Append($"    {Q(animations[i].Name)}: {Q(animations[i].Path)}");
                json.AppendLine(i + 1 < animations.Count ? "," : string.Empty);
            }
            if (normalized != null)
            {
                json.AppendLine("  },");
                json.AppendLine($"  \"normalizedAnimations\": [{string.Join(", ", normalized.Select(Q))}]");
            }
            else
            {
                json.AppendLine("  }");
            }
            json.AppendLine("}");

            using (var stream = settings.CreateFile($"Characters/{ExporterBase.SafeFileName(name)}.json"))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(json.ToString());
        }
    }
}
