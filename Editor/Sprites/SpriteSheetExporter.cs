using System.Collections;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// Toe target: a texture that is imported as sprites (single or multiple) becomes a sprite sheet: <c>&lt;path&gt;.png</c> and <c>&lt;path&gt;.json</c> (see
    /// <see cref="SpriteSheetWriter"/>). It takes textures only when <see cref="ExportContext.ClaimSpriteAssets"/> is on; the orchestrator asks the exporters by
    /// registration name, and this one's name starts with a 0 so that it is asked before <see cref="TextureExporter"/>.
    /// </summary>
    public class SpriteSheetExporter : ExporterBase, IExporter
    {
        public SpriteSheetExporter(NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
        }

        bool IExporter.CanExport(object asset)
            => context.ClaimSpriteAssets && Settings.Target == ExportTarget.Toe && asset is UnityEngine.Texture2D texture && SpriteSheetWriter.IsSpriteTexture(texture);

        string IExporter.EvaluateResourcePath(object asset)
            => asset is UnityEngine.Texture2D texture ? SpriteSheetWriter.JsonPathOf(texture) : null;

        IEnumerable IExporter.Export(object asset)
        {
            var texture = (UnityEngine.Texture2D)asset;
            yield return texture.name;
            SpriteSheetWriter.Write(Settings, context, nameCollisionResolver, texture);
        }
    }
}
