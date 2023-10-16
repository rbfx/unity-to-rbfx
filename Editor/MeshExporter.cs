using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
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

            // Get asset path in the asset database
            var assetPath = AssetDatabase.GetAssetPath(asset);

            SkinnedMeshRenderer skinRenderer = null;
            if (AssetDatabase.LoadMainAssetAtPath(assetPath) is GameObject gameObject)
            {
                SkinnedMeshRenderer renderer;
                if (gameObject.TryGetComponent<SkinnedMeshRenderer>(out renderer) && renderer.sharedMesh == asset)
                {
                    skinRenderer = renderer;
                }

                else
                {
                    foreach (var renderer2 in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        if (renderer2.sharedMesh == asset)
                        {
                            skinRenderer = renderer2;
                        }
                    }
                }
            }

            using (var stream = _settings.CreateFile(EvaluateResourcePath(asset)))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    new ModelBuilder(Settings).WriteMesh(writer, new MeshSource(asset, skinRenderer));
                }
            }
        }
    }
}