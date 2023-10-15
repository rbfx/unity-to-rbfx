// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class RigidBody: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((RigidBody)_).IsEnabled, (_,v)=>((RigidBody)_).IsEnabled = v);
        protected static readonly RbfxAttribute<Quaternion> PhysicsRotationAttr = new ("Physics Rotation", VariantType.VarQuaternion, new Variant(new Quaternion(0f, 0f, 0f, 1f)), _=>((RigidBody)_).PhysicsRotation, (_,v)=>((RigidBody)_).PhysicsRotation = v);
        protected static readonly RbfxAttribute<Vector3> PhysicsPositionAttr = new ("Physics Position", VariantType.VarVector3, new Variant(new Vector3(0f, 0f, 0f)), _=>((RigidBody)_).PhysicsPosition, (_,v)=>((RigidBody)_).PhysicsPosition = v);
        protected static readonly RbfxAttribute<float> MassAttr = new ("Mass", VariantType.VarFloat, new Variant(0f), _=>((RigidBody)_).Mass, (_,v)=>((RigidBody)_).Mass = v);
        protected static readonly RbfxAttribute<float> FrictionAttr = new ("Friction", VariantType.VarFloat, new Variant(0.5f), _=>((RigidBody)_).Friction, (_,v)=>((RigidBody)_).Friction = v);
        protected static readonly RbfxAttribute<Vector3> AnisotropicFrictionAttr = new ("Anisotropic Friction", VariantType.VarVector3, new Variant(new Vector3(1f, 1f, 1f)), _=>((RigidBody)_).AnisotropicFriction, (_,v)=>((RigidBody)_).AnisotropicFriction = v);
        protected static readonly RbfxAttribute<float> RollingFrictionAttr = new ("Rolling Friction", VariantType.VarFloat, new Variant(0f), _=>((RigidBody)_).RollingFriction, (_,v)=>((RigidBody)_).RollingFriction = v);
        protected static readonly RbfxAttribute<float> RestitutionAttr = new ("Restitution", VariantType.VarFloat, new Variant(0f), _=>((RigidBody)_).Restitution, (_,v)=>((RigidBody)_).Restitution = v);
        protected static readonly RbfxAttribute<Vector3> LinearVelocityAttr = new ("Linear Velocity", VariantType.VarVector3, new Variant(new Vector3(0f, 0f, 0f)), _=>((RigidBody)_).LinearVelocity, (_,v)=>((RigidBody)_).LinearVelocity = v);
        protected static readonly RbfxAttribute<Vector3> AngularVelocityAttr = new ("Angular Velocity", VariantType.VarVector3, new Variant(new Vector3(0f, 0f, 0f)), _=>((RigidBody)_).AngularVelocity, (_,v)=>((RigidBody)_).AngularVelocity = v);
        protected static readonly RbfxAttribute<Vector3> LinearFactorAttr = new ("Linear Factor", VariantType.VarVector3, new Variant(new Vector3(1f, 1f, 1f)), _=>((RigidBody)_).LinearFactor, (_,v)=>((RigidBody)_).LinearFactor = v);
        protected static readonly RbfxAttribute<Vector3> AngularFactorAttr = new ("Angular Factor", VariantType.VarVector3, new Variant(new Vector3(1f, 1f, 1f)), _=>((RigidBody)_).AngularFactor, (_,v)=>((RigidBody)_).AngularFactor = v);
        protected static readonly RbfxAttribute<float> LinearDampingAttr = new ("Linear Damping", VariantType.VarFloat, new Variant(0f), _=>((RigidBody)_).LinearDamping, (_,v)=>((RigidBody)_).LinearDamping = v);
        protected static readonly RbfxAttribute<float> AngularDampingAttr = new ("Angular Damping", VariantType.VarFloat, new Variant(0f), _=>((RigidBody)_).AngularDamping, (_,v)=>((RigidBody)_).AngularDamping = v);
        protected static readonly RbfxAttribute<float> LinearRestThresholdAttr = new ("Linear Rest Threshold", VariantType.VarFloat, new Variant(0.8f), _=>((RigidBody)_).LinearRestThreshold, (_,v)=>((RigidBody)_).LinearRestThreshold = v);
        protected static readonly RbfxAttribute<float> AngularRestThresholdAttr = new ("Angular Rest Threshold", VariantType.VarFloat, new Variant(1f), _=>((RigidBody)_).AngularRestThreshold, (_,v)=>((RigidBody)_).AngularRestThreshold = v);
        protected static readonly RbfxAttribute<int> CollisionLayerAttr = new ("Collision Layer", VariantType.VarInt, new Variant(1), _=>((RigidBody)_).CollisionLayer, (_,v)=>((RigidBody)_).CollisionLayer = v);
        protected static readonly RbfxAttribute<int> CollisionMaskAttr = new ("Collision Mask", VariantType.VarInt, new Variant(-1), _=>((RigidBody)_).CollisionMask, (_,v)=>((RigidBody)_).CollisionMask = v);
        protected static readonly RbfxAttribute<float> ContactThresholdAttr = new ("Contact Threshold", VariantType.VarFloat, new Variant(1E+18f), _=>((RigidBody)_).ContactThreshold, (_,v)=>((RigidBody)_).ContactThreshold = v);
        protected static readonly RbfxAttribute<float> CCDRadiusAttr = new ("CCD Radius", VariantType.VarFloat, new Variant(0f), _=>((RigidBody)_).CCDRadius, (_,v)=>((RigidBody)_).CCDRadius = v);
        protected static readonly RbfxAttribute<float> CCDMotionThresholdAttr = new ("CCD Motion Threshold", VariantType.VarFloat, new Variant(0f), _=>((RigidBody)_).CCDMotionThreshold, (_,v)=>((RigidBody)_).CCDMotionThreshold = v);
        protected static readonly RbfxAttribute<string> CollisionEventModeAttr = new ("Collision Event Mode", VariantType.VarString, new Variant(UnityToRebelFork.Editor.CollisionEventMode.WhenActive), _=>((RigidBody)_).CollisionEventMode, (_,v)=>((RigidBody)_).CollisionEventMode = v);
        protected static readonly RbfxAttribute<bool> UseGravityAttr = new ("Use Gravity", VariantType.VarBool, new Variant(true), _=>((RigidBody)_).UseGravity, (_,v)=>((RigidBody)_).UseGravity = v);
        protected static readonly RbfxAttribute<bool> IsKinematicAttr = new ("Is Kinematic", VariantType.VarBool, new Variant(false), _=>((RigidBody)_).IsKinematic, (_,v)=>((RigidBody)_).IsKinematic = v);
        protected static readonly RbfxAttribute<bool> IsTriggerAttr = new ("Is Trigger", VariantType.VarBool, new Variant(false), _=>((RigidBody)_).IsTrigger, (_,v)=>((RigidBody)_).IsTrigger = v);
        protected static readonly RbfxAttribute<Vector3> GravityOverrideAttr = new ("Gravity Override", VariantType.VarVector3, new Variant(new Vector3(0f, 0f, 0f)), _=>((RigidBody)_).GravityOverride, (_,v)=>((RigidBody)_).GravityOverride = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return PhysicsRotationAttr;
            yield return PhysicsPositionAttr;
            yield return MassAttr;
            yield return FrictionAttr;
            yield return AnisotropicFrictionAttr;
            yield return RollingFrictionAttr;
            yield return RestitutionAttr;
            yield return LinearVelocityAttr;
            yield return AngularVelocityAttr;
            yield return LinearFactorAttr;
            yield return AngularFactorAttr;
            yield return LinearDampingAttr;
            yield return AngularDampingAttr;
            yield return LinearRestThresholdAttr;
            yield return AngularRestThresholdAttr;
            yield return CollisionLayerAttr;
            yield return CollisionMaskAttr;
            yield return ContactThresholdAttr;
            yield return CCDRadiusAttr;
            yield return CCDMotionThresholdAttr;
            yield return CollisionEventModeAttr;
            yield return UseGravityAttr;
            yield return IsKinematicAttr;
            yield return IsTriggerAttr;
            yield return GravityOverrideAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected Quaternion _physicsRotation = new Quaternion(0f, 0f, 0f, 1f);

        protected Vector3 _physicsPosition = new Vector3(0f, 0f, 0f);

        protected float _mass = 0f;

        protected float _friction = 0.5f;

        protected Vector3 _anisotropicFriction = new Vector3(1f, 1f, 1f);

        protected float _rollingFriction = 0f;

        protected float _restitution = 0f;

        protected Vector3 _linearVelocity = new Vector3(0f, 0f, 0f);

        protected Vector3 _angularVelocity = new Vector3(0f, 0f, 0f);

        protected Vector3 _linearFactor = new Vector3(1f, 1f, 1f);

        protected Vector3 _angularFactor = new Vector3(1f, 1f, 1f);

        protected float _linearDamping = 0f;

        protected float _angularDamping = 0f;

        protected float _linearRestThreshold = 0.8f;

        protected float _angularRestThreshold = 1f;

        protected int _collisionLayer = 1;

        protected int _collisionMask = -1;

        protected float _contactThreshold = 1E+18f;

        protected float _cCDRadius = 0f;

        protected float _cCDMotionThreshold = 0f;

        protected string _collisionEventMode = UnityToRebelFork.Editor.CollisionEventMode.WhenActive;

        protected bool _useGravity = true;

        protected bool _isKinematic = false;

        protected bool _isTrigger = false;

        protected Vector3 _gravityOverride = new Vector3(0f, 0f, 0f);

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public Quaternion PhysicsRotation
        {
            get => _physicsRotation;
            set => _physicsRotation = value;
        }

        public Vector3 PhysicsPosition
        {
            get => _physicsPosition;
            set => _physicsPosition = value;
        }

        public float Mass
        {
            get => _mass;
            set => _mass = value;
        }

        public float Friction
        {
            get => _friction;
            set => _friction = value;
        }

        public Vector3 AnisotropicFriction
        {
            get => _anisotropicFriction;
            set => _anisotropicFriction = value;
        }

        public float RollingFriction
        {
            get => _rollingFriction;
            set => _rollingFriction = value;
        }

        public float Restitution
        {
            get => _restitution;
            set => _restitution = value;
        }

        public Vector3 LinearVelocity
        {
            get => _linearVelocity;
            set => _linearVelocity = value;
        }

        public Vector3 AngularVelocity
        {
            get => _angularVelocity;
            set => _angularVelocity = value;
        }

        public Vector3 LinearFactor
        {
            get => _linearFactor;
            set => _linearFactor = value;
        }

        public Vector3 AngularFactor
        {
            get => _angularFactor;
            set => _angularFactor = value;
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

        public float LinearRestThreshold
        {
            get => _linearRestThreshold;
            set => _linearRestThreshold = value;
        }

        public float AngularRestThreshold
        {
            get => _angularRestThreshold;
            set => _angularRestThreshold = value;
        }

        public int CollisionLayer
        {
            get => _collisionLayer;
            set => _collisionLayer = value;
        }

        public int CollisionMask
        {
            get => _collisionMask;
            set => _collisionMask = value;
        }

        public float ContactThreshold
        {
            get => _contactThreshold;
            set => _contactThreshold = value;
        }

        public float CCDRadius
        {
            get => _cCDRadius;
            set => _cCDRadius = value;
        }

        public float CCDMotionThreshold
        {
            get => _cCDMotionThreshold;
            set => _cCDMotionThreshold = value;
        }

        public string CollisionEventMode
        {
            get => _collisionEventMode;
            set => _collisionEventMode = value;
        }

        public bool UseGravity
        {
            get => _useGravity;
            set => _useGravity = value;
        }

        public bool IsKinematic
        {
            get => _isKinematic;
            set => _isKinematic = value;
        }

        public bool IsTrigger
        {
            get => _isTrigger;
            set => _isTrigger = value;
        }

        public Vector3 GravityOverride
        {
            get => _gravityOverride;
            set => _gravityOverride = value;
        }
    }
}
