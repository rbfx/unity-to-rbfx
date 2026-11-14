using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// Toe target: a prefab with an Animator and a SpriteRenderer is a sprite character; its sprite animations become <c>&lt;prefab path&gt;.anim.json</c> (the Toe
    /// <c>SpriteAnimationSet</c>) and the sheet of the sprites they use is written beside it (see <see cref="SpriteSheetWriter"/>).
    /// <list type="bullet">
    /// <item>Every state of the Animator controller (all layers) that plays an animation clip with sprite keys gives a clip of the set, and its lowercase name is a
    /// <c>state</c> that names that clip. A clip keys the sprite in one of two ways: an object reference curve on SpriteRenderer.sprite (the usual way), or the float
    /// <c>m_SpriteKey</c> of the 2D Animation package's SpriteResolver, whose bits are the hash of "Category_Label" in the character's Sprite Library (see <see cref="SpriteHash"/>).</item>
    /// <item>Clips that are not sprite animations (a colour flash), that use a sprite the library does not have, or whose sprites are not all in the one texture are left out with a message.</item>
    /// <item>The frames, the rate and the loop come from <see cref="ClipFrames"/>; animation events become events at frame numbers.</item>
    /// </list>
    /// The Sprite Library is read by reflection, so the exporter compiles and runs with or without the 2D Animation package.
    /// Takes prefabs only while <see cref="ExportContext.ClaimSpriteAssets"/> is on (the name starts with a 0 so that it is asked before <see cref="PrefabExporter"/>).
    /// </summary>
    public class SpriteAnimationExporter : ExporterBase, IExporter
    {
        private const string SpriteKeyProperty = "m_SpriteKey";

        public SpriteAnimationExporter(NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
        }

        bool IExporter.CanExport(object asset)
        {
            if (!context.ClaimSpriteAssets || Settings.Target != ExportTarget.Toe) return false;
            var prefab = asset as UnityEngine.GameObject;
            if (prefab == null || !AssetDatabase.Contains(prefab)) return false;
            var animator = prefab.GetComponentInChildren<UnityEngine.Animator>(true);
            return animator != null && animator.runtimeAnimatorController != null && prefab.GetComponentInChildren<UnityEngine.SpriteRenderer>(true) != null;
        }

        string IExporter.EvaluateResourcePath(object asset)
            => asset is UnityEngine.GameObject prefab ? ReplaceExtension(SpriteSheetWriter.RelativePath(AssetDatabase.GetAssetPath(prefab)), ".anim.json") : null;

        IEnumerable IExporter.Export(object asset)
        {
            var prefab = (UnityEngine.GameObject)asset;
            yield return prefab.name;

            var animator = prefab.GetComponentInChildren<UnityEngine.Animator>(true);
            var states = CollectStates(animator.runtimeAnimatorController, prefab);
            var library = ReadSpriteLibrary(prefab);

            // The keys of every clip as sprites; what cannot be read is left out, and said.
            var clipSprites = new List<KeyValuePair<UnityEngine.AnimationClip, List<KeyValuePair<float, UnityEngine.Sprite>>>>();
            foreach (var clip in states.Select(s => s.Value).Distinct())
            {
                string problem;
                var keys = ReadKeys(clip, library, out problem);
                if (keys == null) UnityEngine.Debug.Log("[SpriteAnimationExporter] " + prefab.name + ": the clip '" + clip.name + "' is left out: " + problem);
                else clipSprites.Add(new KeyValuePair<UnityEngine.AnimationClip, List<KeyValuePair<float, UnityEngine.Sprite>>>(clip, keys));
            }

            // One animation set has one sheet: the texture most of the sprites are in.
            var texturePaths = clipSprites.SelectMany(c => c.Value).Select(k => AssetDatabase.GetAssetPath(k.Value)).Where(p => !string.IsNullOrEmpty(p)).ToList();
            if (texturePaths.Count == 0)
            {
                UnityEngine.Debug.LogWarning("[SpriteAnimationExporter] " + prefab.name + " has no animation that switches sprites; nothing is written.", prefab);
                yield break;
            }
            var texturePath = texturePaths.GroupBy(p => p).OrderByDescending(g => g.Count()).First().Key;
            var texture = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(texturePath);
            if (texture == null || !SpriteSheetWriter.IsSpriteTexture(texture))
            {
                UnityEngine.Debug.LogError("[SpriteAnimationExporter] " + prefab.name + ": '" + texturePath + "' is not a texture imported as sprites.", prefab);
                throw new InvalidOperationException("The sprites of " + prefab.name + " are not in a sprite texture.");
            }
            var sheet = SpriteSheetWriter.Write(Settings, context, nameCollisionResolver, texture);

            var frameLists = new List<FrameList>();
            var clipNames = new Dictionary<UnityEngine.AnimationClip, string>();
            foreach (var pair in clipSprites)
            {
                var clip = pair.Key;
                if (pair.Value.Any(k => !sheet.Names.ContainsKey(k.Value)))
                {
                    UnityEngine.Debug.Log("[SpriteAnimationExporter] " + prefab.name + ": the clip '" + clip.name + "' is left out: it uses sprites of another texture than " + texturePath);
                    continue;
                }
                var keys = pair.Value.Select(k => new ClipKey(k.Key, sheet.Names[k.Value])).ToList();
                var events = AnimationUtility.GetAnimationEvents(clip).Select(e => new ClipEvent(e.time, e.functionName)).ToList();
                var name = UniqueName(clip.name, frameLists);
                var frames = ClipFrames.Build(name, keys, clip.length, AnimationUtility.GetAnimationClipSettings(clip).loopTime, events);
                if (frames == null)
                {
                    UnityEngine.Debug.Log("[SpriteAnimationExporter] " + prefab.name + ": the clip '" + clip.name + "' is left out: it shows no sprite for any length of time.");
                    continue;
                }
                foreach (var warning in frames.Warnings) UnityEngine.Debug.LogWarning("[SpriteAnimationExporter] " + prefab.name + ", clip '" + clip.name + "': " + warning, prefab);
                frameLists.Add(frames);
                clipNames[clip] = name;
            }

            // The states, by the lowercase name of the Animator state; the first state of that name wins.
            var stateMap = new List<KeyValuePair<string, string>>();
            foreach (var state in states)
            {
                string clipName;
                var key = state.Key.ToLowerInvariant();
                if (clipNames.TryGetValue(state.Value, out clipName) && stateMap.All(s => s.Key != key))
                    stateMap.Add(new KeyValuePair<string, string>(key, clipName));
            }

            var path = ReplaceExtension(SpriteSheetWriter.RelativePath(AssetDatabase.GetAssetPath(prefab)), ".anim.json");
            var json = SpriteAnimationJson.Build(RelativeTo(path, sheet.JsonPath), frameLists, stateMap);
            using (var stream = Settings.CreateFile(path))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(json);
            UnityEngine.Debug.Log("[SpriteAnimationExporter] " + prefab.name + " -> " + path + ": " + frameLists.Count + " clips (" + string.Join(", ", frameLists.Select(f => f.Name).ToArray()) + "), " +
                                  stateMap.Count + " states");
        }

        /// <summary>The way the animation set names its sheet: next to it or below it by a relative name, anywhere else from the root (a name that starts with a slash).</summary>
        public static string RelativeTo(string setPath, string sheetPath)
        {
            var slash = setPath.LastIndexOf('/');
            var folder = slash < 0 ? string.Empty : setPath.Substring(0, slash + 1);
            return sheetPath.StartsWith(folder, StringComparison.Ordinal) ? sheetPath.Substring(folder.Length) : "/" + sheetPath;
        }

        private static string UniqueName(string name, List<FrameList> taken)
        {
            if (taken.All(t => t.Name != name)) return name;
            for (int i = 2; ; i++)
            {
                var candidate = name + "_" + i;
                if (taken.All(t => t.Name != candidate)) return candidate;
            }
        }

        /// <summary>The animation states of a controller with the clip each plays (clips of override controllers replaced); blend trees are left out.</summary>
        private static List<KeyValuePair<string, UnityEngine.AnimationClip>> CollectStates(UnityEngine.RuntimeAnimatorController controller, UnityEngine.GameObject prefab)
        {
            var result = new List<KeyValuePair<string, UnityEngine.AnimationClip>>();
            var overrideController = controller as UnityEngine.AnimatorOverrideController;
            var editorController = (overrideController != null ? overrideController.runtimeAnimatorController : controller) as AnimatorController;
            if (editorController == null)
            {
                foreach (var clip in controller.animationClips) result.Add(new KeyValuePair<string, UnityEngine.AnimationClip>(clip.name, clip));
                return result;
            }

            var animatorStates = new List<AnimatorState>();
            foreach (var layer in editorController.layers) Collect(layer.stateMachine, animatorStates);
            foreach (var state in animatorStates)
            {
                var clip = state.motion as UnityEngine.AnimationClip;
                if (clip == null)
                {
                    if (state.motion != null) UnityEngine.Debug.Log("[SpriteAnimationExporter] " + prefab.name + ": the state '" + state.name + "' plays a blend tree, which is not exported.");
                    continue;
                }
                if (overrideController != null) clip = overrideController[clip] ?? clip;
                result.Add(new KeyValuePair<string, UnityEngine.AnimationClip>(state.name, clip));
            }
            return result;
        }

        private static void Collect(AnimatorStateMachine machine, List<AnimatorState> states)
        {
            foreach (var child in machine.states) states.Add(child.state);
            foreach (var sub in machine.stateMachines) Collect(sub.stateMachine, states);
        }

        /// <summary>
        /// The sprites a clip shows and when: from an object reference curve on a SpriteRenderer's sprite, or from the float curve of a SpriteResolver's key looked up in
        /// the Sprite Library. Null (with the reason) if the clip is no sprite animation or a key has no sprite.
        /// </summary>
        public static List<KeyValuePair<float, UnityEngine.Sprite>> ReadKeys(UnityEngine.AnimationClip clip, Dictionary<int, UnityEngine.Sprite> library, out string problem)
        {
            problem = null;
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                if (binding.propertyName != "m_Sprite") continue;
                var keys = new List<KeyValuePair<float, UnityEngine.Sprite>>();
                foreach (var key in AnimationUtility.GetObjectReferenceCurve(clip, binding))
                {
                    var sprite = key.value as UnityEngine.Sprite;
                    if (sprite == null) { problem = "a key at " + key.time + " s has no sprite."; return null; }
                    keys.Add(new KeyValuePair<float, UnityEngine.Sprite>(key.time, sprite));
                }
                return keys;
            }

            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (binding.propertyName != SpriteKeyProperty) continue;
                var keys = new List<KeyValuePair<float, UnityEngine.Sprite>>();
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                foreach (var key in curve.keys)
                {
                    int hash = SpriteHash.KeyFromCurveValue(key.value);
                    UnityEngine.Sprite sprite;
                    if (library == null || !library.TryGetValue(hash, out sprite) || sprite == null)
                    {
                        problem = library == null
                            ? "it switches sprites through a Sprite Library, but the prefab has none (or the 2D Animation package is missing)."
                            : "the key " + hash + " at " + key.time + " s is not a sprite of the Sprite Library.";
                        return null;
                    }
                    keys.Add(new KeyValuePair<float, UnityEngine.Sprite>(key.time, sprite));
                }
                return keys;
            }

            problem = "it does not switch sprites.";
            return null;
        }

        /// <summary>
        /// The sprites of the prefab's Sprite Library by their key (the hash of "Category_Label"), or null if it has none. Read by reflection, so that nothing here depends on
        /// the 2D Animation package being installed: <c>SpriteLibrary.spriteLibraryAsset</c>, then <c>GetCategoryNames</c>, <c>GetCategoryLabelNames</c> and <c>GetSprite</c> of the asset.
        /// </summary>
        public static Dictionary<int, UnityEngine.Sprite> ReadSpriteLibrary(UnityEngine.GameObject prefab)
        {
            var libraryType = Type.GetType("UnityEngine.U2D.Animation.SpriteLibrary, Unity.2D.Animation.Runtime");
            if (libraryType == null) return null;
            var assetProperty = libraryType.GetProperty("spriteLibraryAsset");
            if (assetProperty == null) return null;

            Dictionary<int, UnityEngine.Sprite> result = null;
            foreach (var component in prefab.GetComponentsInChildren(libraryType, true))
            {
                var asset = assetProperty.GetValue(component, null);
                if (asset == null) continue;
                var assetType = asset.GetType();
                var categoryNames = assetType.GetMethod("GetCategoryNames", Type.EmptyTypes);
                var labelNames = assetType.GetMethod("GetCategoryLabelNames", new[] { typeof(string) });
                var getSprite = assetType.GetMethod("GetSprite", new[] { typeof(string), typeof(string) });
                if (categoryNames == null || labelNames == null || getSprite == null) continue;

                if (result == null) result = new Dictionary<int, UnityEngine.Sprite>();
                foreach (var category in (IEnumerable<string>)categoryNames.Invoke(asset, null))
                {
                    foreach (var label in (IEnumerable<string>)labelNames.Invoke(asset, new object[] { category }))
                        result[SpriteHash.Key(category, label)] = getSprite.Invoke(asset, new object[] { category, label }) as UnityEngine.Sprite;
                }
            }
            return result;
        }
    }
}
