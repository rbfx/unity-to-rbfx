using System.Collections;
using System.IO;

namespace UnityToRebelFork.Editor
{
    public class MeshReferenceExporter : ExporterBase<MeshReference>
    {
        public MeshReferenceExporter()
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

            using (var stream = _settings.CreateFile(EvaluateResourcePath(asset)))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    new ModelBuilder(Settings).WriteMesh(writer, new LODGroupSource(asset));
                }
            }
        }
    }
}