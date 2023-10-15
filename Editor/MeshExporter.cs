using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class MeshExporter: ExporterBase<Mesh>
    {
        public MeshExporter()
        {
        }

        /// <inheritdoc />
        public override string EvaluateResourcePath(Mesh asset)
        {
            return EvaluateResourcePath(asset, ".mdl");
        }

        public override IEnumerable Export(Mesh asset)
        {
            yield return asset.name;

            using (var stream = _settings.CreateFile(EvaluateResourcePath(asset)))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    new ModelBuilder(Settings).WriteMesh(writer, new MeshSource(asset));
                }
            }
        }
    }
}