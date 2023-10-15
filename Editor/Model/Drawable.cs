// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class Drawable: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<int> MaxLightsAttr = new ("Max Lights", VariantType.VarInt, new Variant(0), _=>((Drawable)_).MaxLights, (_,v)=>((Drawable)_).MaxLights = v);
        protected static readonly RbfxAttribute<int> ViewMaskAttr = new ("View Mask", VariantType.VarInt, new Variant(-1), _=>((Drawable)_).ViewMask, (_,v)=>((Drawable)_).ViewMask = v);
        protected static readonly RbfxAttribute<int> LightMaskAttr = new ("Light Mask", VariantType.VarInt, new Variant(-1), _=>((Drawable)_).LightMask, (_,v)=>((Drawable)_).LightMask = v);
        protected static readonly RbfxAttribute<int> ShadowMaskAttr = new ("Shadow Mask", VariantType.VarInt, new Variant(-1), _=>((Drawable)_).ShadowMask, (_,v)=>((Drawable)_).ShadowMask = v);
        protected static readonly RbfxAttribute<int> ZoneMaskAttr = new ("Zone Mask", VariantType.VarInt, new Variant(-1), _=>((Drawable)_).ZoneMask, (_,v)=>((Drawable)_).ZoneMask = v);
        protected static readonly RbfxAttribute<string> GlobalIlluminationValueAttr = new ("Global Illumination", VariantType.VarString, new Variant(UnityToRebelFork.Editor.GlobalIlluminationValue.None), _=>((Drawable)_).GlobalIlluminationValue, (_,v)=>((Drawable)_).GlobalIlluminationValue = v);
        protected static readonly RbfxAttribute<string> ReflectionModeAttr = new ("Reflection Mode", VariantType.VarString, new Variant(UnityToRebelFork.Editor.ReflectionMode.BlendProbesandZone), _=>((Drawable)_).ReflectionMode, (_,v)=>((Drawable)_).ReflectionMode = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return MaxLightsAttr;
            yield return ViewMaskAttr;
            yield return LightMaskAttr;
            yield return ShadowMaskAttr;
            yield return ZoneMaskAttr;
            yield return GlobalIlluminationValueAttr;
            yield return ReflectionModeAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected int _maxLights = 0;

        protected int _viewMask = -1;

        protected int _lightMask = -1;

        protected int _shadowMask = -1;

        protected int _zoneMask = -1;

        protected string _globalIlluminationValue = UnityToRebelFork.Editor.GlobalIlluminationValue.None;

        protected string _reflectionMode = UnityToRebelFork.Editor.ReflectionMode.BlendProbesandZone;

        public int MaxLights
        {
            get => _maxLights;
            set => _maxLights = value;
        }

        public int ViewMask
        {
            get => _viewMask;
            set => _viewMask = value;
        }

        public int LightMask
        {
            get => _lightMask;
            set => _lightMask = value;
        }

        public int ShadowMask
        {
            get => _shadowMask;
            set => _shadowMask = value;
        }

        public int ZoneMask
        {
            get => _zoneMask;
            set => _zoneMask = value;
        }

        public string GlobalIlluminationValue
        {
            get => _globalIlluminationValue;
            set => _globalIlluminationValue = value;
        }

        public string ReflectionMode
        {
            get => _reflectionMode;
            set => _reflectionMode = value;
        }
    }
}
