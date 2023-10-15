// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class ConstraintRevolute2D: UnityToRebelFork.Editor.Constraint2D
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((ConstraintRevolute2D)_).IsEnabled, (_,v)=>((ConstraintRevolute2D)_).IsEnabled = v);
        protected static readonly RbfxAttribute<Vector2> AnchorAttr = new ("Anchor", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((ConstraintRevolute2D)_).Anchor, (_,v)=>((ConstraintRevolute2D)_).Anchor = v);
        protected static readonly RbfxAttribute<bool> EnableLimitAttr = new ("Enable Limit", VariantType.VarBool, new Variant(false), _=>((ConstraintRevolute2D)_).EnableLimit, (_,v)=>((ConstraintRevolute2D)_).EnableLimit = v);
        protected static readonly RbfxAttribute<float> LowerAngleAttr = new ("Lower Angle", VariantType.VarFloat, new Variant(0f), _=>((ConstraintRevolute2D)_).LowerAngle, (_,v)=>((ConstraintRevolute2D)_).LowerAngle = v);
        protected static readonly RbfxAttribute<float> UpperAngleAttr = new ("Upper Angle", VariantType.VarFloat, new Variant(0f), _=>((ConstraintRevolute2D)_).UpperAngle, (_,v)=>((ConstraintRevolute2D)_).UpperAngle = v);
        protected static readonly RbfxAttribute<bool> EnableMotorAttr = new ("Enable Motor", VariantType.VarBool, new Variant(false), _=>((ConstraintRevolute2D)_).EnableMotor, (_,v)=>((ConstraintRevolute2D)_).EnableMotor = v);
        protected static readonly RbfxAttribute<float> MotorSpeedAttr = new ("Motor Speed", VariantType.VarFloat, new Variant(0f), _=>((ConstraintRevolute2D)_).MotorSpeed, (_,v)=>((ConstraintRevolute2D)_).MotorSpeed = v);
        protected static readonly RbfxAttribute<float> MaxMotorTorqueAttr = new ("Max Motor Torque", VariantType.VarFloat, new Variant(0f), _=>((ConstraintRevolute2D)_).MaxMotorTorque, (_,v)=>((ConstraintRevolute2D)_).MaxMotorTorque = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return AnchorAttr;
            yield return EnableLimitAttr;
            yield return LowerAngleAttr;
            yield return UpperAngleAttr;
            yield return EnableMotorAttr;
            yield return MotorSpeedAttr;
            yield return MaxMotorTorqueAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected Vector2 _anchor = new Vector2(0f, 0f);

        protected bool _enableLimit = false;

        protected float _lowerAngle = 0f;

        protected float _upperAngle = 0f;

        protected bool _enableMotor = false;

        protected float _motorSpeed = 0f;

        protected float _maxMotorTorque = 0f;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public Vector2 Anchor
        {
            get => _anchor;
            set => _anchor = value;
        }

        public bool EnableLimit
        {
            get => _enableLimit;
            set => _enableLimit = value;
        }

        public float LowerAngle
        {
            get => _lowerAngle;
            set => _lowerAngle = value;
        }

        public float UpperAngle
        {
            get => _upperAngle;
            set => _upperAngle = value;
        }

        public bool EnableMotor
        {
            get => _enableMotor;
            set => _enableMotor = value;
        }

        public float MotorSpeed
        {
            get => _motorSpeed;
            set => _motorSpeed = value;
        }

        public float MaxMotorTorque
        {
            get => _maxMotorTorque;
            set => _maxMotorTorque = value;
        }
    }
}
