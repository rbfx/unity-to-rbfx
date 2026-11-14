using System.Collections.Generic;

namespace UnityToRebelFork.Editor
{
    /// <summary>rbfx's scene-level manager of reflection probes: a scene with ReflectionProbe components needs one.</summary>
    public partial class ReflectionProbeManager : UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<float> QueryPaddingAttr = new("Query Padding", VariantType.VarFloat, new Variant(2f), _ => ((ReflectionProbeManager)_).QueryPadding, (_, v) => ((ReflectionProbeManager)_).QueryPadding = v);
        protected static readonly RbfxAttribute<int> RenderBudgetAttr = new("Render Budget", VariantType.VarInt, new Variant(6), _ => ((ReflectionProbeManager)_).RenderBudget, (_, v) => ((ReflectionProbeManager)_).RenderBudget = v);
        protected static readonly RbfxAttribute<bool> FilterCubemapsAttr = new("Filter Cubemaps", VariantType.VarBool, new Variant(true), _ => ((ReflectionProbeManager)_).FilterCubemaps, (_, v) => ((ReflectionProbeManager)_).FilterCubemaps = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return QueryPaddingAttr;
            yield return RenderBudgetAttr;
            yield return FilterCubemapsAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        public float QueryPadding { get; set; } = 2f;
        public int RenderBudget { get; set; } = 6;

        /// <summary>Let rbfx filter the cube maps into roughness mips. Off for exports whose files already hold them.</summary>
        public bool FilterCubemaps { get; set; } = true;
    }
}
