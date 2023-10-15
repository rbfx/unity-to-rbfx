// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class GlobalIllumination: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<float> EmissionBrightnessAttr = new ("Emission Brightness", VariantType.VarFloat, new Variant(1f), _=>((GlobalIllumination)_).EmissionBrightness, (_,v)=>((GlobalIllumination)_).EmissionBrightness = v);
        protected static readonly RbfxAttribute<ResourceRef> DataFileAttr = new ("Data File", VariantType.VarResourceRef, new Variant(new ResourceRef("Context", "")), _=>((GlobalIllumination)_).DataFile, (_,v)=>((GlobalIllumination)_).DataFile = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return EmissionBrightnessAttr;
            yield return DataFileAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected float _emissionBrightness = 1f;

        protected ResourceRef _dataFile = new ResourceRef("Context", "");

        public float EmissionBrightness
        {
            get => _emissionBrightness;
            set => _emissionBrightness = value;
        }

        public ResourceRef DataFile
        {
            get => _dataFile;
            set => _dataFile = value;
        }
    }
}
