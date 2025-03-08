using System.Collections;
using System.IO;

namespace UnityToRebelFork.Editor
{
    public class MeshReferenceExporter : ExporterBase<MeshReference>
    {
        public MeshReferenceExporter(NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
        }

        /// <inheritdoc />
        public override string EvaluateResourcePath(MeshReference asset)
        {
            string extension = ".mdl";
            if (asset.Levels.Count != 1 || asset.Levels[0].Renderers.Count != 1)
            {
                extension = "." + asset.GetHashCode() + extension;
            }
            return EvaluateResourcePath(asset.FirstMesh, extension);
        }

        public override IEnumerable Export(MeshReference asset)
        {
            yield return "LOD group";

            var path = EvaluateResourcePath(asset);
            if (string.IsNullOrWhiteSpace(path))
                yield break;

            using (var stream = _settings.CreateFile(path))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    new ModelBuilder(Settings).WriteMesh(writer, new LODGroupSource(asset));
                }
            }
        }
    }
}