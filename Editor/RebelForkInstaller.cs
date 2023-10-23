using Zenject;

namespace UnityToRebelFork.Editor
{
    public class RebelForkInstaller : Installer<ExportSettings, ExportContext, RebelForkInstaller>
    {
        [Inject] private ExportSettings settings;
        [Inject] private ExportContext context;

        public override void InstallBindings()
        {
            Container.BindInstance(settings).AsSingle();
            Container.BindInstance(context).AsSingle();
            //Container.Bind<IExporter>().To(_=>_.AllTypes().DerivingFrom<IExporter>().FromThisAssembly()).AsCached();
            Container.BindInterfacesAndSelfTo<MeshExporter>().AsSingle();
            Container.BindInterfacesAndSelfTo<MeshReferenceExporter>().AsSingle();
            Container.BindInterfacesAndSelfTo<MaterialExporter>().AsSingle();
            Container.BindInterfacesAndSelfTo<PrefabExporter>().AsSingle();
            Container.BindInterfacesAndSelfTo<SceneExporter>().AsSingle();
            Container.BindInterfacesAndSelfTo<ExportOrchestrator>().AsSingle();
            Container.BindInterfacesAndSelfTo<TextureExporter>().AsSingle();
            Container.BindInterfacesAndSelfTo<TextureRecipeExporter>().AsSingle();

            Container.BindInterfacesAndSelfTo<MobileVertexLitShaderMapping>().AsSingle();
            Container.BindInterfacesAndSelfTo<StandardShaderMapping>().AsSingle();
            Container.BindInterfacesAndSelfTo<StandardSpecularShaderMapping>().AsSingle();
            Container.BindInterfacesAndSelfTo<DefaultShaderMapping>().AsSingle();

            Container.Bind<NameCollisionResolver>().AsCached();
            Container.Bind<PrefabVisitor>().AsTransient();
        }

        //public ExportSettings LoadSettings()
        //{
        //    var asset = AssetDatabase.FindAssets($"t:{nameof(ExportSettings)}").FirstOrDefault();
        //    if (asset == null)
        //    {
        //        var settings = new ExportSettings();
        //        AssetDatabase.CreateAsset(settings, "Assets/UnityToRebelFork.asset");
        //        return settings;
        //    }

        //    return AssetDatabase.LoadAssetAtPath<ExportSettings>(
        //        AssetDatabase.GUIDToAssetPath(asset)) as ExportSettings;
        //}
    }
}