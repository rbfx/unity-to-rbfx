// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class ConstraintMouse2D: UnityToRebelFork.Editor.Constraint2D
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((ConstraintMouse2D)_).IsEnabled, (_,v)=>((ConstraintMouse2D)_).IsEnabled = v);
        protected static readonly RbfxAttribute<Vector2> TargetAttr = new ("Target", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((ConstraintMouse2D)_).Target, (_,v)=>((ConstraintMouse2D)_).Target = v);
        protected static readonly RbfxAttribute<float> MaxForceAttr = new ("Max Force", VariantType.VarFloat, new Variant(0f), _=>((ConstraintMouse2D)_).MaxForce, (_,v)=>((ConstraintMouse2D)_).MaxForce = v);
        protected static readonly RbfxAttribute<float> FrequencyHzAttr = new ("Frequency Hz", VariantType.VarFloat, new Variant(5f), _=>((ConstraintMouse2D)_).FrequencyHz, (_,v)=>((ConstraintMouse2D)_).FrequencyHz = v);
        protected static readonly RbfxAttribute<float> DampingRatioAttr = new ("Damping Ratio", VariantType.VarFloat, new Variant(0.7f), _=>((ConstraintMouse2D)_).DampingRatio, (_,v)=>((ConstraintMouse2D)_).DampingRatio = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return TargetAttr;
            yield return MaxForceAttr;
            yield return FrequencyHzAttr;
            yield return DampingRatioAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected Vector2 _target = new Vector2(0f, 0f);

        protected float _maxForce = 0f;

        protected float _frequencyHz = 5f;

        protected float _dampingRatio = 0.7f;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public Vector2 Target
        {
            get => _target;
            set => _target = value;
        }

        public float MaxForce
        {
            get => _maxForce;
            set => _maxForce = value;
        }

        public float FrequencyHz
        {
            get => _frequencyHz;
            set => _frequencyHz = value;
        }

        public float DampingRatio
        {
            get => _dampingRatio;
            set => _dampingRatio = value;
        }
    }
}
