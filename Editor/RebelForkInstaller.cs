using Rbfx.LightInject;
using UnityToRebelFork.Editor.Unlit;

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

            // The orchestrator asks the exporters in the order of their registration names and takes the first that can export the asset. These take only what they
            // know (Toe target, sprites and audio), but AnimationExporter takes every AnimationClip and TextureExporter every Texture: the "0" puts these first.
            container.RegisterSingleton<IExporter, SpriteSheetExporter>("0" + nameof(SpriteSheetExporter));
            container.RegisterSingleton<IExporter, SpriteAnimationExporter>("0" + nameof(SpriteAnimationExporter));
            container.RegisterSingleton<IExporter, AudioClipExporter>("0" + nameof(AudioClipExporter));

            container.RegisterSingleton<IExporter, AnimationExporter>(nameof(AnimationExporter));
            container.RegisterSingleton<IExporter, MeshExporter>(nameof(MeshExporter));
            container.RegisterSingleton<IExporter, MeshReferenceExporter>(nameof(MeshReferenceExporter));
            container.RegisterSingleton<IExporter, MaterialExporter>(nameof(MaterialExporter));
            container.RegisterSingleton<IExporter, PrefabExporter>(nameof(PrefabExporter));
            container.RegisterSingleton<IExporter, SceneExporter>(nameof(SceneExporter));
            container.RegisterSingleton<IExporter, ParticleSystemExporter>(nameof(ParticleSystemExporter));
            container.RegisterSingleton<IExporter, TerrainExporter>(nameof(TerrainExporter));
            container.RegisterSingleton<IExporter, TextureExporter>(nameof(TextureExporter));
            container.RegisterSingleton<IExporter, TextureRecipeExporter>(nameof(TextureRecipeExporter));
            container.RegisterSingleton<IExporter, CubemapExporter>(nameof(CubemapExporter));

            container.RegisterSingleton<IShaderMapping, ParticleShaderMapping>(nameof(ParticleShaderMapping));
            container.RegisterSingleton<IShaderMapping, TransparentShaderMapping>(nameof(TransparentShaderMapping));
            container.RegisterSingleton<IShaderMapping, ToonShaderMapping>(nameof(ToonShaderMapping));
            container.RegisterSingleton<IShaderMapping, MobileVertexLitShaderMapping>(nameof(MobileVertexLitShaderMapping));
            container.RegisterSingleton<IShaderMapping, StandardShaderMapping>(nameof(StandardShaderMapping));
            container.RegisterSingleton<IShaderMapping, StandardSpecularShaderMapping>(nameof(StandardSpecularShaderMapping));
            container.RegisterSingleton<IShaderMapping, LegacyDiffuseShaderMapping>(nameof(LegacyDiffuseShaderMapping));
            container.RegisterSingleton<IShaderMapping, LitOpaqueShaderMapping>(nameof(LitOpaqueShaderMapping));
            container.RegisterSingleton<DefaultShaderMapping, DefaultShaderMapping>();

            container.RegisterSingleton<NameCollisionResolver>();
            container.Register<PrefabVisitor>();
        }
    }
}