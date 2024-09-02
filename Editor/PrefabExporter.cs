using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class PrefabExporter: ExporterBase<GameObject>
    {
        private readonly PrefabVisitor prefabVisitor;

        public PrefabExporter(PrefabVisitor prefabVisitor, NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
            this.prefabVisitor = prefabVisitor;
        }

        /// <inheritdoc />
        public override string EvaluateResourcePath(GameObject asset)
        {
            return ReplaceExtension(GetRelPathFromAsset(asset), ".prefab");
        }

        /// <summary>
        /// Export prefab to a file.
        /// </summary>
        /// <param name="asset">Prefab's root game object.</param>
        /// <returns>Unity coroutine enumerable.</returns>
        public override IEnumerable Export(GameObject asset)
        {
            yield return asset.name;

            var root = prefabVisitor.Visit(asset);
            root.Position = Vector3.zero;
            root.Rotation = Quaternion.identity;
            root.Scale = Vector3.one;
            var scene = Helpers.CreateDefaultScene(asset.name);
            scene.Children.Add(root);
            if (!Settings.EmptyNodes)
                new EmptyNodeCleanupVisitor().Visit(root);
            scene.Children.Add(new Node()
            {
                Name = "Prefab Light",
                Rotation = Quaternion.AngleAxis(90, Vector3.right),
                Components = {
                    new Light()
                    {
                        LightType = LightType.Directional
                    }
                }
            });
            new UniqueIdVisitor().Vist(scene);

            using (var stream = _settings.CreateFile(EvaluateResourcePath(asset)))
            {
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    new XDocument(new PrefabSceneSerializer().Visit(scene)).Save(writer);
                }
            }
        }
    }
}