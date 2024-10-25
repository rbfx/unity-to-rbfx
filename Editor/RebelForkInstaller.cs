using Rbfx.LightInject;

namespace UnityToRebelFork.Editor
{
    public class RebelForkInstaller
    {
        public static void Install(Rbfx.LightInject.ServiceContainer container, ExportSettings settings, ExportContext context)
        {
            container.RegisterInstance(settings);
            container.RegisterInstance(context);

            container.RegisterSingleton<ExportOrchestrator>();
            
            //Container.Bind<IExporter>().To(_=>_.AllTypes().DerivingFrom<IExporter>().FromThisAssembly()).AsCached();
            container.RegisterSingleton<IExporter, AnimationExporter>(nameof(AnimationExporter));
            container.RegisterSingleton<IExporter, MeshExporter>(nameof(MeshExporter));
            container.RegisterSingleton<IExporter, MeshReferenceExporter>(nameof(MeshReferenceExporter));
            container.RegisterSingleton<IExporter, MaterialExporter>(nameof(MaterialExporter));
            container.RegisterSingleton<IExporter, PrefabExporter>(nameof(PrefabExporter));
            container.RegisterSingleton<IExporter, SceneExporter>(nameof(SceneExporter));
            container.RegisterSingleton<IExporter, TextureExporter>(nameof(TextureExporter));
            container.RegisterSingleton<IExporter, TextureRecipeExporter>(nameof(TextureRecipeExporter));

            container.RegisterSingleton<IShaderMapping, MobileVertexLitShaderMapping>(nameof(MobileVertexLitShaderMapping));
            container.RegisterSingleton<IShaderMapping, StandardShaderMapping>(nameof(StandardShaderMapping));
            container.RegisterSingleton<IShaderMapping, StandardSpecularShaderMapping>(nameof(StandardSpecularShaderMapping));
            container.RegisterSingleton<IShaderMapping, LegacyDiffuseShaderMapping>(nameof(LegacyDiffuseShaderMapping));
            container.RegisterSingleton<IShaderMapping, LitOpaqueShaderMapping>(nameof(LitOpaqueShaderMapping));
            container.RegisterSingleton<IShaderMapping, DefaultShaderMapping>(nameof(DefaultShaderMapping));

            container.RegisterSingleton<NameCollisionResolver>();
            container.Register<PrefabVisitor>();
        }
    }
}