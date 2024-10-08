using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace UnityToRebelFork.Editor
{
    public class SceneExporter: ExporterBase<UnityEngine.SceneManagement.Scene>
    {
        private readonly PrefabVisitor prefabVisitor;

        public SceneExporter(PrefabVisitor prefabVisitor, NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings):base(nameCollisionResolver, context, settings)
        {
            this.prefabVisitor = prefabVisitor;
        }

        /// <inheritdoc />
        public override string EvaluateResourcePath(UnityEngine.SceneManagement.Scene asset)
        {
            return ReplaceExtension(GetRelPathFromAsset(asset), ".scene");
        }

        public override IEnumerable Export(UnityEngine.SceneManagement.Scene asset)
        {
            yield return asset.name;

            var scene = prefabVisitor.Visit(asset);
            if (!Settings.EmptyNodes)
                new EmptyNodeCleanupVisitor().Visit(scene);
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