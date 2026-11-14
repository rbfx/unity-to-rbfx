using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// rbfx's ReflectionProbe (Bounding Box, Texture, projection box, priority). The attributes at the end belong to Toe's probe of the
    /// same name, which sizes its volume and blends probes differently; each engine skips the attributes it does not know.
    /// </summary>
    public partial class ReflectionProbe : UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new("Is Enabled", VariantType.VarBool, new Variant(true), _ => ((ReflectionProbe)_).IsEnabled, (_, v) => ((ReflectionProbe)_).IsEnabled = v);
        protected static readonly RbfxAttribute<bool> IsMovableAttr = new("Is Movable", VariantType.VarBool, new Variant(false), _ => ((ReflectionProbe)_).IsMovable, (_, v) => ((ReflectionProbe)_).IsMovable = v);
        protected static readonly RbfxAttribute<bool> IsRealtimeUpdateAttr = new("Is Realtime Update", VariantType.VarBool, new Variant(false), _ => ((ReflectionProbe)_).IsRealtimeUpdate, (_, v) => ((ReflectionProbe)_).IsRealtimeUpdate = v);
        protected static readonly RbfxAttribute<Vector3> BoundingBoxMinAttr = new("Bounding Box Min", VariantType.VarVector3, new Variant(new Vector3(-1f, -1f, -1f)), _ => ((ReflectionProbe)_).BoundingBoxMin, (_, v) => ((ReflectionProbe)_).BoundingBoxMin = v);
        protected static readonly RbfxAttribute<Vector3> BoundingBoxMaxAttr = new("Bounding Box Max", VariantType.VarVector3, new Variant(new Vector3(1f, 1f, 1f)), _ => ((ReflectionProbe)_).BoundingBoxMax, (_, v) => ((ReflectionProbe)_).BoundingBoxMax = v);
        protected static readonly RbfxAttribute<int> PriorityAttr = new("Priority", VariantType.VarInt, new Variant(0), _ => ((ReflectionProbe)_).Priority, (_, v) => ((ReflectionProbe)_).Priority = v);
        protected static readonly RbfxAttribute<ResourceRef> TextureAttr = new("Texture", VariantType.VarResourceRef, new Variant(new ResourceRef("TextureCube", "")), _ => ((ReflectionProbe)_).Texture, (_, v) => ((ReflectionProbe)_).Texture = v);
        protected static readonly RbfxAttribute<bool> UseBoxProjectionAttr = new("Use Box Projection", VariantType.VarBool, new Variant(false), _ => ((ReflectionProbe)_).UseBoxProjection, (_, v) => ((ReflectionProbe)_).UseBoxProjection = v);
        protected static readonly RbfxAttribute<Vector3> ProjectionBoxMinAttr = new("Projection Box Min", VariantType.VarVector3, new Variant(new Vector3(-1f, -1f, -1f)), _ => ((ReflectionProbe)_).ProjectionBoxMin, (_, v) => ((ReflectionProbe)_).ProjectionBoxMin = v);
        protected static readonly RbfxAttribute<Vector3> ProjectionBoxMaxAttr = new("Projection Box Max", VariantType.VarVector3, new Variant(new Vector3(1f, 1f, 1f)), _ => ((ReflectionProbe)_).ProjectionBoxMax, (_, v) => ((ReflectionProbe)_).ProjectionBoxMax = v);
        protected static readonly RbfxAttribute<int> TextureSizeAttr = new("Texture Size", VariantType.VarInt, new Variant(256), _ => ((ReflectionProbe)_).TextureSize, (_, v) => ((ReflectionProbe)_).TextureSize = v);
        protected static readonly RbfxAttribute<float> NearClipAttr = new("Near Clip", VariantType.VarFloat, new Variant(0.1f), _ => ((ReflectionProbe)_).NearClip, (_, v) => ((ReflectionProbe)_).NearClip = v);
        protected static readonly RbfxAttribute<float> FarClipAttr = new("Far Clip", VariantType.VarFloat, new Variant(1000f), _ => ((ReflectionProbe)_).FarClip, (_, v) => ((ReflectionProbe)_).FarClip = v);

        // Toe only.
        protected static readonly RbfxAttribute<string> ModeAttr = new("Mode", VariantType.VarString, new Variant("CustomTexture"), _ => ((ReflectionProbe)_).Mode, (_, v) => ((ReflectionProbe)_).Mode = v);
        protected static readonly RbfxAttribute<float> IntensityAttr = new("Intensity", VariantType.VarFloat, new Variant(1f), _ => ((ReflectionProbe)_).Intensity, (_, v) => ((ReflectionProbe)_).Intensity = v);
        protected static readonly RbfxAttribute<float> BlendDistanceAttr = new("Blend Distance", VariantType.VarFloat, new Variant(1f), _ => ((ReflectionProbe)_).BlendDistance, (_, v) => ((ReflectionProbe)_).BlendDistance = v);
        protected static readonly RbfxAttribute<bool> BoxProjectionAttr = new("Box Projection", VariantType.VarBool, new Variant(true), _ => ((ReflectionProbe)_).BoxProjection, (_, v) => ((ReflectionProbe)_).BoxProjection = v);
        protected static readonly RbfxAttribute<int> ResolutionAttr = new("Resolution", VariantType.VarInt, new Variant(128), _ => ((ReflectionProbe)_).Resolution, (_, v) => ((ReflectionProbe)_).Resolution = v);
        protected static readonly RbfxAttribute<float> CaptureFarClipAttr = new("Capture Far Clip", VariantType.VarFloat, new Variant(100f), _ => ((ReflectionProbe)_).CaptureFarClip, (_, v) => ((ReflectionProbe)_).CaptureFarClip = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return IsMovableAttr;
            yield return IsRealtimeUpdateAttr;
            yield return BoundingBoxMinAttr;
            yield return BoundingBoxMaxAttr;
            yield return PriorityAttr;
            yield return TextureAttr;
            yield return UseBoxProjectionAttr;
            yield return ProjectionBoxMinAttr;
            yield return ProjectionBoxMaxAttr;
            yield return TextureSizeAttr;
            yield return NearClipAttr;
            yield return FarClipAttr;
            yield return ModeAttr;
            yield return IntensityAttr;
            yield return BlendDistanceAttr;
            yield return BoxProjectionAttr;
            yield return ResolutionAttr;
            yield return CaptureFarClipAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        public bool IsEnabled { get; set; } = true;
        public bool IsMovable { get; set; }
        public bool IsRealtimeUpdate { get; set; }
        public Vector3 BoundingBoxMin { get; set; } = new Vector3(-1f, -1f, -1f);
        public Vector3 BoundingBoxMax { get; set; } = new Vector3(1f, 1f, 1f);
        public int Priority { get; set; }
        public ResourceRef Texture { get; set; } = new ResourceRef("TextureCube", "");
        public bool UseBoxProjection { get; set; }
        public Vector3 ProjectionBoxMin { get; set; } = new Vector3(-1f, -1f, -1f);
        public Vector3 ProjectionBoxMax { get; set; } = new Vector3(1f, 1f, 1f);
        public int TextureSize { get; set; } = 256;
        public float NearClip { get; set; } = 0.1f;
        public float FarClip { get; set; } = 1000f;

        /// <summary>Toe: CustomTexture, Baked (Toe draws the scene from the probe once) or Dynamic.</summary>
        public string Mode { get; set; } = "CustomTexture";
        public float Intensity { get; set; } = 1f;
        public float BlendDistance { get; set; } = 1f;
        public bool BoxProjection { get; set; } = true;
        public int Resolution { get; set; } = 128;
        public float CaptureFarClip { get; set; } = 100f;
    }
}
