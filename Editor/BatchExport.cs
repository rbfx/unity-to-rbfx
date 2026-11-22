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
    ///       [-exportScene &lt;Assets/...unity&gt;] [-exportSprites &lt;list file&gt;] [-exportAudio &lt;list file&gt;] [-toe &lt;toe.dll&gt;]
    /// </code>
    /// Toe target only: -exportSprites takes a list of textures imported as sprites (written as a PNG and a sprite sheet JSON), prefabs with an Animator and sprite renderers (written as
    /// an animation set JSON next to the sheet of their sprites) and folders of those (see <see cref="SpriteSheetExporter"/>, <see cref="SpriteAnimationExporter"/>); -exportAudio takes audio
    /// clips and folders of them (see <see cref="AudioClipExporter"/>) and, with -toe and ffmpeg, encodes them to Ogg Vorbis.
    /// A scene is written as &lt;its asset path&gt;.scene with everything it uses; its prefab instances are written inline (see
    /// <see cref="ExportSettings.InlinePrefabs"/>) and [ExportComponent] behaviours become components.
    /// A list file names one asset path per line (# starts a comment). The character's enabled skinned meshes are exported with
    /// their materials and textures, the clips (FBX files or animation clips) are baked onto its skeleton, and
    /// Characters/&lt;name&gt;.json lists what was written. Each prop (a prefab, or the root object of a model file) is exported
    /// as a prefab at the path of its asset (see <see cref="PrefabExporter"/>), with the meshes, materials and textures it uses.
    /// Toe target: the Jiggle Chain and Jiggle Collider components on the character's prefab are written as the "jiggle" section of Characters/&lt;name&gt;.json (see <see cref="JiggleExport"/>),
    /// and the Magica Cloth 2 components as its "cloth" section (see <see cref="ClothExport"/>; read by reflection, Magica Cloth 2 is no dependency).
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
            var spriteList = Arg(args, "-exportSprites");
            var audioList = Arg(args, "-exportAudio");
            if (characterPath == null && propList == null && scenePath == null && effectList == null && spriteList == null && audioList == null)
                throw new ArgumentException("Give -exportCharacter, -exportProps, -exportEffects, -exportSprites, -exportAudio or -exportScene.");
            if ((spriteList != null || audioList != null) && target != ExportTarget.Toe)
                throw new ArgumentException("-exportSprites and -exportAudio are for the Toe target: give -exportTarget toe (or use RunToe).");
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
            // Sprite and audio exporters take what they know only when the run was asked for sprites or audio: a character's texture that happens to be imported as a sprite stays a texture.
            context.ClaimSpriteAssets = spriteList != null || audioList != null;
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

            // Sprites: textures imported as sprites become sheets, prefabs with an Animator and sprite renderers become animation sets. Audio: clips are copied and encoded afterwards.
            int missingSprites = ScheduleAssets(orchestrator, context, spriteList, "sprite", new[] { "t:Texture2D", "t:Prefab" });
            int missingAudio = ScheduleAssets(orchestrator, context, audioList, "audio", new[] { "t:AudioClip" });

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
            if (toe && settings.PrefilterCubemaps && context.CubemapsToPrefilter.Count > 0)
                PrefilterCubemaps(settings, context.CubemapsToPrefilter, ref failures);
            if (toe && context.AudioToEncode.Count > 0)
                EncodeAudio(settings, context.AudioToEncode, ref failures);
            string jiggle = character != null ? JiggleSection(character, toe) : null;
            string cloth = character != null ? ClothSection(character, toe) : null;
            if (character != null) WriteManifest(settings, character.name, models, animations, rootBone, humanBones, normalized, jiggle, cloth);
            if (toe && !string.IsNullOrEmpty(effectList)) WriteEffectReport(settings, context, System.IO.Path.GetFileNameWithoutExtension(effectList));
            bool missing = (character != null && models.Count == 0) || missingProps > 0 || missingEffects > 0 || missingSprites > 0 || missingAudio > 0;
            return failures == 0 && !missing ? 0 : 1;
        }

        /// <summary>
        /// Schedules the assets of a list file: a line is an asset path, or a folder whose assets of the given types (the Asset Database filters, such as t:Texture2D) are all offered to
        /// the exporters. An asset that no exporter takes is an error when it is named, and is skipped quietly when it was only found in a folder (a folder of sprites also holds
        /// textures and prefabs that are not sprites). A line can end with options after a bar: <c>Assets/Pack | ppu=32</c> forces 32 pixels per unit on the sprites under that path
        /// (see <see cref="ExportContext.SpritePixelsPerUnitRules"/>). Returns the number of errors.
        /// </summary>
        private static int ScheduleAssets(ExportOrchestrator orchestrator, ExportContext context, string listFile, string kind, string[] filters)
        {
            if (string.IsNullOrEmpty(listFile)) return 0;
            int errors = 0, scheduled = 0, skipped = 0;
            foreach (var raw in System.IO.File.ReadAllLines(listFile))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var bar = line.IndexOf('|');
                if (bar >= 0)
                {
                    foreach (var option in line.Substring(bar + 1).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (option.StartsWith("ppu=", StringComparison.OrdinalIgnoreCase) && float.TryParse(option.Substring(4), NumberStyles.Float, CultureInfo.InvariantCulture, out var ppu) && ppu > 0f)
                            context.SpritePixelsPerUnitRules.Add(new KeyValuePair<string, float>(line.Substring(0, bar).Trim(), ppu));
                        else
                            Debug.LogError($"[BatchExport] unknown option '{option}' in the list {listFile} (known: ppu=<pixels per unit>)");
                    }
                    line = line.Substring(0, bar).Trim();
                }
                if (AssetDatabase.IsValidFolder(line))
                {
                    var paths = new SortedSet<string>(StringComparer.Ordinal);
                    foreach (var filter in filters)
                        foreach (var guid in AssetDatabase.FindAssets(filter, new[] { line })) paths.Add(AssetDatabase.GUIDToAssetPath(guid));
                    foreach (var path in paths)
                    {
                        var asset = AssetDatabase.LoadMainAssetAtPath(path);
                        var resource = asset == null ? null : orchestrator.ScheduleExport(asset);
                        if (resource == null) { skipped++; continue; }
                        scheduled++;
                        Debug.Log($"[BatchExport] {kind} {path} -> {resource}");
                    }
                    continue;
                }

                var named = AssetDatabase.LoadMainAssetAtPath(line);
                var result = named == null ? null : orchestrator.ScheduleExport(named);
                if (result == null)
                {
                    Debug.LogError($"[BatchExport] cannot export the {kind} asset {line} (is it there, and is it a {kind} the exporters know?)");
                    errors++;
                    continue;
                }
                scheduled++;
                Debug.Log($"[BatchExport] {kind} {line} -> {result}");
            }
            Debug.Log($"[BatchExport] {kind}: {scheduled} scheduled, {skipped} skipped (found in folders, not {kind} assets), {errors} errors");
            return errors;
        }

        /// <summary>
        /// Runs <c>dotnet toe.dll sound encode &lt;file&gt; &lt;file&gt;.ogg</c> for every exported audio file that is not an Ogg yet, and removes the original when it worked. Without the
        /// Toe CLI, or when ffmpeg is missing, the files stay as they are (a game can play them, they are only bigger) and that is a warning; any other failure is an error of the export.
        /// </summary>
        private static void EncodeAudio(ExportSettings settings, List<string> files, ref int failures)
        {
            if (string.IsNullOrEmpty(settings.ToeCliPath) || !System.IO.File.Exists(settings.ToeCliPath))
            {
                Debug.LogWarning($"[BatchExport] {files.Count} audio file(s) were exported as they are; give -toe <path of toe.dll> (and have ffmpeg on the PATH) to encode them to Ogg Vorbis.");
                return;
            }
            foreach (var resource in files)
            {
                var input = Path.GetFullPath(Path.Combine(settings.path, resource));
                var output = Path.ChangeExtension(input, ".ogg");
                var start = new System.Diagnostics.ProcessStartInfo("dotnet")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                foreach (var arg in new[] { settings.ToeCliPath, "sound", "encode", input, output }) start.ArgumentList.Add(arg);
                using (var process = System.Diagnostics.Process.Start(start))
                {
                    var errorTask = process.StandardError.ReadToEndAsync();
                    string text = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    string error = errorTask.Result;
                    if (process.ExitCode == 0)
                    {
                        Debug.Log("[BatchExport] toe: " + text.Trim());
                        System.IO.File.Delete(input);
                        continue;
                    }
                    var message = (text + "\n" + error).Trim();
                    if (message.Contains("ffmpeg not found"))
                    {
                        Debug.LogWarning($"[BatchExport] ffmpeg was not found, so {resource} is left as it is (put ffmpeg on the PATH to get an Ogg file).");
                        continue;
                    }
                    Debug.LogError($"[BatchExport] toe sound encode failed for {resource} ({process.ExitCode}): {message}");
                    failures++;
                }
            }
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
            if (string.IsNullOrEmpty(settings.ToeCliPath) || !System.IO.File.Exists(settings.ToeCliPath))
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

        /// <summary>
        /// Runs <c>dotnet toe.dll --data &lt;out&gt; cubemap prefilter &lt;cube map xml&gt;... --in-place</c> once on the cube maps that were exported (probes and the scene's reflection):
        /// Toe bakes their roughness chain into the files, so a game does not filter them when it first uses them. Without the Toe CLI the cube maps are left as they are
        /// (Toe filters them on load); a failure is an error of the export.
        /// </summary>
        private static void PrefilterCubemaps(ExportSettings settings, List<string> cubemaps, ref int failures)
        {
            if (string.IsNullOrEmpty(settings.ToeCliPath) || !System.IO.File.Exists(settings.ToeCliPath))
            {
                Debug.LogWarning($"[BatchExport] {cubemaps.Count} cube map(s) were exported; give -toe <path of toe.dll> to bake their roughness chain (cubemap prefilter), otherwise Toe filters them when a game loads them.");
                return;
            }
            var start = new System.Diagnostics.ProcessStartInfo("dotnet")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            foreach (var arg in new[] { settings.ToeCliPath, "--data", settings.path, "cubemap", "prefilter" }) start.ArgumentList.Add(arg);
            foreach (var cubemap in cubemaps) start.ArgumentList.Add(cubemap.Replace('\\', '/'));
            start.ArgumentList.Add("--in-place");
            using (var process = System.Diagnostics.Process.Start(start))
            {
                var errorTask = process.StandardError.ReadToEndAsync();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                string error = errorTask.Result;
                if (output.Length > 0) Debug.Log("[BatchExport] toe: " + output);
                if (process.ExitCode != 0)
                {
                    Debug.LogError($"[BatchExport] toe cubemap prefilter failed ({process.ExitCode}): {error}");
                    failures++;
                }
            }
        }

        /// <summary>
        /// The <c>jiggle</c> section of the character manifest: the Jiggle Chain and Jiggle Collider components on the prefab (<see cref="JiggleExport"/>), for the Toe target. The Rbfx
        /// target leaves them out with one warning. Null when there are none.
        /// </summary>
        private static string JiggleSection(GameObject character, bool toe)
        {
            var chains = new List<JiggleChainData>();
            var colliders = new List<JiggleColliderData>();
            JiggleExport.Collect(character.transform, chains, colliders, message => Debug.LogWarning("[Jiggle] " + message, character));
            if (chains.Count == 0 && colliders.Count == 0) return null;
            if (!toe)
            {
                Debug.LogWarning($"[Jiggle] '{character.name}' has {chains.Count} Jiggle Chain and {colliders.Count} Jiggle Collider components: they are exported for the Toe target only; the Rbfx export leaves them out.", character);
                return null;
            }
            Debug.Log($"[BatchExport] jiggle: {chains.Count} chains, {colliders.Count} colliders");
            return JiggleExport.ToJson(chains, colliders, "  ");
        }

        /// <summary>
        /// The <c>cloth</c> section of the character manifest: the Magica Cloth 2 components on the prefab, read by reflection (see <see cref="ClothExport"/>), for the Toe target. The Rbfx
        /// target leaves them out with one warning. Null when there are none.
        /// </summary>
        private static string ClothSection(GameObject character, bool toe)
        {
            var section = ClothExport.Collect(character.transform, message => Debug.LogWarning("[Cloth] " + message, character));
            if (section == null) return null;
            if (!toe)
            {
                Debug.LogWarning($"[Cloth] '{character.name}' has Magica Cloth 2 components: they are exported for the Toe target only; the Rbfx export leaves them out.", character);
                return null;
            }
            Debug.Log($"[BatchExport] cloth: {section["cloths"].Count()} cloths, {section["colliders"].Count()} colliders");
            return ClothExport.ToJson(section, "  ");
        }

        private static void WriteManifest(ExportSettings settings, string name, List<ModelEntry> models,
            List<(string Name, string Path)> animations, string rootBone, Dictionary<string, string> humanBones, List<string> normalized, string jiggle = null, string cloth = null)
        {
            string Q(string s) => "\"" + (s ?? string.Empty).Replace("\\", "/").Replace("\"", "\\\"") + "\"";
            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"name\": {Q(name)},");
            json.AppendLine($"  \"rootBone\": {Q(rootBone)},");
            if (humanBones != null) json.AppendLine("  \"humanBones\": " + HumanoidMapExporter.ToJson(humanBones, "  ") + ",");
            if (jiggle != null) json.AppendLine("  \"jiggle\": " + jiggle + ",");
            if (cloth != null) json.AppendLine("  \"cloth\": " + cloth + ",");
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
