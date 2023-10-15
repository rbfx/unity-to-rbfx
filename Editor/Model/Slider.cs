// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class Slider: UnityToRebelFork.Editor.BorderImage
    {
        protected static readonly RbfxAttribute<string> OrientationAttr = new ("Orientation", VariantType.VarString, new Variant(UnityToRebelFork.Editor.Orientation.Horizontal), _=>((Slider)_).Orientation, (_,v)=>((Slider)_).Orientation = v);
        protected static readonly RbfxAttribute<float> RangeAttr = new ("Range", VariantType.VarFloat, new Variant(1f), _=>((Slider)_).Range, (_,v)=>((Slider)_).Range = v);
        protected static readonly RbfxAttribute<float> ValueAttr = new ("Value", VariantType.VarFloat, new Variant(0f), _=>((Slider)_).Value, (_,v)=>((Slider)_).Value = v);
        protected static readonly RbfxAttribute<float> RepeatRateAttr = new ("Repeat Rate", VariantType.VarFloat, new Variant(0f), _=>((Slider)_).RepeatRate, (_,v)=>((Slider)_).RepeatRate = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return OrientationAttr;
            yield return RangeAttr;
            yield return ValueAttr;
            yield return RepeatRateAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected string _orientation = UnityToRebelFork.Editor.Orientation.Horizontal;

        protected float _range = 1f;

        protected float _value = 0f;

        protected float _repeatRate = 0f;

        public string Orientation
        {
            get => _orientation;
            set => _orientation = value;
        }

        public float Range
        {
            get => _range;
            set => _range = value;
        }

        public float Value
        {
            get => _value;
            set => _value = value;
        }

        public float RepeatRate
        {
            get => _repeatRate;
            set => _repeatRate = value;
        }
    }
}
