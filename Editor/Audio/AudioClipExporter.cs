using System.Collections;
using System.IO;
using UnityEditor;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// Toe target: an audio clip is exported from its SOURCE file (the WAV or MP3 the project imported), not from the clip Unity decoded, so nothing is lost on the way.
    /// The file is copied to the output next to where its Ogg Vorbis version is to be; when <see cref="ExportSettings.EncodeAudio"/> is on and the batch export has the Toe CLI,
    /// the file is queued in <see cref="ExportContext.AudioToEncode"/> and replaced by <c>&lt;path&gt;.ogg</c> afterwards (<c>toe sound encode</c>, which needs ffmpeg).
    /// Unity cannot make Ogg files from a script itself. A source that already is an Ogg file is copied as it is. Takes clips only while <see cref="ExportContext.ClaimSpriteAssets"/> is on.
    /// </summary>
    public class AudioClipExporter : ExporterBase, IExporter
    {
        public AudioClipExporter(NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
        }

        bool IExporter.CanExport(object asset) => context.ClaimSpriteAssets && Settings.Target == ExportTarget.Toe && asset is UnityEngine.AudioClip;

        string IExporter.EvaluateResourcePath(object asset)
        {
            if (!(asset is UnityEngine.AudioClip clip)) return null;
            var path = AssetDatabase.GetAssetPath(clip);
            return string.IsNullOrEmpty(path) ? null : ReplaceExtension(SpriteSheetWriter.RelativePath(path), ".ogg");
        }

        IEnumerable IExporter.Export(object asset)
        {
            var clip = (UnityEngine.AudioClip)asset;
            yield return clip.name;

            var source = AssetDatabase.GetAssetPath(clip);
            if (string.IsNullOrEmpty(source) || !System.IO.File.Exists(source))
            {
                UnityEngine.Debug.LogError("[AudioClipExporter] '" + clip.name + "' has no source file in the project, so it cannot be exported.", clip);
                throw new FileNotFoundException("No source file for the audio clip " + clip.name);
            }

            var extension = Path.GetExtension(source).ToLowerInvariant();
            var relative = ReplaceExtension(SpriteSheetWriter.RelativePath(source), extension);
            System.IO.File.Copy(Path.GetFullPath(source), Settings.ResolveResourceFileName(relative), true);

            if (extension == ".ogg")
            {
                UnityEngine.Debug.Log("[AudioClipExporter] " + source + " -> " + relative);
            }
            else if (Settings.EncodeAudio)
            {
                context.AudioToEncode.Add(relative);
                UnityEngine.Debug.Log("[AudioClipExporter] " + source + " -> " + relative + " (to be encoded to .ogg)");
            }
            else
            {
                UnityEngine.Debug.Log("[AudioClipExporter] " + source + " -> " + relative + " (not encoded: EncodeAudio is off)");
            }
        }
    }
}
