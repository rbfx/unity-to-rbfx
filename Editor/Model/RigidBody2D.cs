// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class RigidBody2D: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((RigidBody2D)_).IsEnabled, (_,v)=>((RigidBody2D)_).IsEnabled = v);
        protected static readonly RbfxAttribute<string> BodyTypeAttr = new ("Body Type", VariantType.VarString, new Variant(UnityToRebelFork.Editor.BodyType.Static), _=>((RigidBody2D)_).BodyType, (_,v)=>((RigidBody2D)_).BodyType = v);
        protected static readonly RbfxAttribute<float> MassAttr = new ("Mass", VariantType.VarFloat, new Variant(0f), _=>((RigidBody2D)_).Mass, (_,v)=>((RigidBody2D)_).Mass = v);
        protected static readonly RbfxAttribute<float> InertiaAttr = new ("Inertia", VariantType.VarFloat, new Variant(0f), _=>((RigidBody2D)_).Inertia, (_,v)=>((RigidBody2D)_).Inertia = v);
        protected static readonly RbfxAttribute<Vector2> MassCenterAttr = new ("Mass Center", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((RigidBody2D)_).MassCenter, (_,v)=>((RigidBody2D)_).MassCenter = v);
        protected static readonly RbfxAttribute<bool> UseFixtureMassAttr = new ("Use Fixture Mass", VariantType.VarBool, new Variant(true), _=>((RigidBody2D)_).UseFixtureMass, (_,v)=>((RigidBody2D)_).UseFixtureMass = v);
        protected static readonly RbfxAttribute<float> LinearDampingAttr = new ("Linear Damping", VariantType.VarFloat, new Variant(0f), _=>((RigidBody2D)_).LinearDamping, (_,v)=>((RigidBody2D)_).LinearDamping = v);
        protected static readonly RbfxAttribute<float> AngularDampingAttr = new ("Angular Damping", VariantType.VarFloat, new Variant(0f), _=>((RigidBody2D)_).AngularDamping, (_,v)=>((RigidBody2D)_).AngularDamping = v);
        protected static readonly RbfxAttribute<bool> AllowSleepAttr = new ("Allow Sleep", VariantType.VarBool, new Variant(true), _=>((RigidBody2D)_).AllowSleep, (_,v)=>((RigidBody2D)_).AllowSleep = v);
        protected static readonly RbfxAttribute<bool> FixedRotationAttr = new ("Fixed Rotation", VariantType.VarBool, new Variant(false), _=>((RigidBody2D)_).FixedRotation, (_,v)=>((RigidBody2D)_).FixedRotation = v);
        protected static readonly RbfxAttribute<bool> BulletAttr = new ("Bullet", VariantType.VarBool, new Variant(false), _=>((RigidBody2D)_).Bullet, (_,v)=>((RigidBody2D)_).Bullet = v);
        protected static readonly RbfxAttribute<float> GravityScaleAttr = new ("Gravity Scale", VariantType.VarFloat, new Variant(1f), _=>((RigidBody2D)_).GravityScale, (_,v)=>((RigidBody2D)_).GravityScale = v);
        protected static readonly RbfxAttribute<bool> AwakeAttr = new ("Awake", VariantType.VarBool, new Variant(true), _=>((RigidBody2D)_).Awake, (_,v)=>((RigidBody2D)_).Awake = v);
        protected static readonly RbfxAttribute<Vector2> LinearVelocityAttr = new ("Linear Velocity", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((RigidBody2D)_).LinearVelocity, (_,v)=>((RigidBody2D)_).LinearVelocity = v);
        protected static readonly RbfxAttribute<float> AngularVelocityAttr = new ("Angular Velocity", VariantType.VarFloat, new Variant(0f), _=>((RigidBody2D)_).AngularVelocity, (_,v)=>((RigidBody2D)_).AngularVelocity = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return BodyTypeAttr;
            yield return MassAttr;
            yield return InertiaAttr;
            yield return MassCenterAttr;
            yield return UseFixtureMassAttr;
            yield return LinearDampingAttr;
            yield return AngularDampingAttr;
            yield return AllowSleepAttr;
            yield return FixedRotationAttr;
            yield return BulletAttr;
            yield return GravityScaleAttr;
            yield return AwakeAttr;
            yield return LinearVelocityAttr;
            yield return AngularVelocityAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected string _bodyType = UnityToRebelFork.Editor.BodyType.Static;

        protected float _mass = 0f;

        protected float _inertia = 0f;

        protected Vector2 _massCenter = new Vector2(0f, 0f);

        protected bool _useFixtureMass = true;

        protected float _linearDamping = 0f;

        protected float _angularDamping = 0f;

        protected bool _allowSleep = true;

        protected bool _fixedRotation = false;

        protected bool _bullet = false;

        protected float _gravityScale = 1f;

        protected bool _awake = true;

        protected Vector2 _linearVelocity = new Vector2(0f, 0f);

        protected float _angularVelocity = 0f;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public string BodyType
        {
            get => _bodyType;
            set => _bodyType = value;
        }

        public float Mass
        {
            get => _mass;
            set => _mass = value;
        }

        public float Inertia
        {
            get => _inertia;
            set => _inertia = value;
        }

        public Vector2 MassCenter
        {
            get => _massCenter;
            set => _massCenter = value;
        }

        public bool UseFixtureMass
        {
            get => _useFixtureMass;
            set => _useFixtureMass = value;
        }

        public float LinearDamping
        {
            get => _linearDamping;
            set => _linearDamping = value;
        }

        public float AngularDamping
        {
            get => _angularDamping;
            set => _angularDamping = value;
        }

        public bool AllowSleep
        {
            get => _allowSleep;
            set => _allowSleep = value;
        }

        public bool FixedRotation
        {
            get => _fixedRotation;
            set => _fixedRotation = value;
        }

        public bool Bullet
        {
            get => _bullet;
            set => _bullet = value;
        }

        public float GravityScale
        {
            get => _gravityScale;
            set => _gravityScale = value;
        }

        public bool Awake
        {
            get => _awake;
            set => _awake = value;
        }

        public Vector2 LinearVelocity
        {
            get => _linearVelocity;
            set => _linearVelocity = value;
        }

        public float AngularVelocity
        {
            get => _angularVelocity;
            set => _angularVelocity = value;
        }
    }
}