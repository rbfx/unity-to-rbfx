// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class ConstraintDistance2D: UnityToRebelFork.Editor.Constraint2D
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((ConstraintDistance2D)_).IsEnabled, (_,v)=>((ConstraintDistance2D)_).IsEnabled = v);
        protected static readonly RbfxAttribute<Vector2> OwnerBodyAnchorAttr = new ("Owner Body Anchor", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((ConstraintDistance2D)_).OwnerBodyAnchor, (_,v)=>((ConstraintDistance2D)_).OwnerBodyAnchor = v);
        protected static readonly RbfxAttribute<Vector2> OtherBodyAnchorAttr = new ("Other Body Anchor", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((ConstraintDistance2D)_).OtherBodyAnchor, (_,v)=>((ConstraintDistance2D)_).OtherBodyAnchor = v);
        protected static readonly RbfxAttribute<float> FrequencyHzAttr = new ("Frequency Hz", VariantType.VarFloat, new Variant(0f), _=>((ConstraintDistance2D)_).FrequencyHz, (_,v)=>((ConstraintDistance2D)_).FrequencyHz = v);
        protected static readonly RbfxAttribute<float> DampingRatioAttr = new ("Damping Ratio", VariantType.VarFloat, new Variant(0f), _=>((ConstraintDistance2D)_).DampingRatio, (_,v)=>((ConstraintDistance2D)_).DampingRatio = v);
        protected static readonly RbfxAttribute<float> LengthAttr = new ("Length", VariantType.VarFloat, new Variant(1f), _=>((ConstraintDistance2D)_).Length, (_,v)=>((ConstraintDistance2D)_).Length = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return OwnerBodyAnchorAttr;
            yield return OtherBodyAnchorAttr;
            yield return FrequencyHzAttr;
            yield return DampingRatioAttr;
            yield return LengthAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected Vector2 _ownerBodyAnchor = new Vector2(0f, 0f);

        protected Vector2 _otherBodyAnchor = new Vector2(0f, 0f);

        protected float _frequencyHz = 0f;

        protected float _dampingRatio = 0f;

        protected float _length = 1f;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public Vector2 OwnerBodyAnchor
        {
            get => _ownerBodyAnchor;
            set => _ownerBodyAnchor = value;
        }

        public Vector2 OtherBodyAnchor
        {
            get => _otherBodyAnchor;
            set => _otherBodyAnchor = value;
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

        public float Length
        {
            get => _length;
            set => _length = value;
        }
    }
}
