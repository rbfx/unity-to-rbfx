// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class PhysicsWorld: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<Vector3> GravityAttr = new ("Gravity", VariantType.VarVector3, new Variant(new Vector3(0f, -9.81f, 0f)), _=>((PhysicsWorld)_).Gravity, (_,v)=>((PhysicsWorld)_).Gravity = v);
        protected static readonly RbfxAttribute<int> PhysicsFPSAttr = new ("Physics FPS", VariantType.VarInt, new Variant(60), _=>((PhysicsWorld)_).PhysicsFPS, (_,v)=>((PhysicsWorld)_).PhysicsFPS = v);
        protected static readonly RbfxAttribute<int> MaxSubstepsAttr = new ("Max Substeps", VariantType.VarInt, new Variant(0), _=>((PhysicsWorld)_).MaxSubsteps, (_,v)=>((PhysicsWorld)_).MaxSubsteps = v);
        protected static readonly RbfxAttribute<int> SolverIterationsAttr = new ("Solver Iterations", VariantType.VarInt, new Variant(10), _=>((PhysicsWorld)_).SolverIterations, (_,v)=>((PhysicsWorld)_).SolverIterations = v);
        protected static readonly RbfxAttribute<float> NetMaxAngularVelAttr = new ("Net Max Angular Vel.", VariantType.VarFloat, new Variant(100f), _=>((PhysicsWorld)_).NetMaxAngularVel, (_,v)=>((PhysicsWorld)_).NetMaxAngularVel = v);
        protected static readonly RbfxAttribute<bool> InterpolationAttr = new ("Interpolation", VariantType.VarBool, new Variant(true), _=>((PhysicsWorld)_).Interpolation, (_,v)=>((PhysicsWorld)_).Interpolation = v);
        protected static readonly RbfxAttribute<bool> InternalEdgeUtilityAttr = new ("Internal Edge Utility", VariantType.VarBool, new Variant(true), _=>((PhysicsWorld)_).InternalEdgeUtility, (_,v)=>((PhysicsWorld)_).InternalEdgeUtility = v);
        protected static readonly RbfxAttribute<bool> SplitImpulseAttr = new ("Split Impulse", VariantType.VarBool, new Variant(false), _=>((PhysicsWorld)_).SplitImpulse, (_,v)=>((PhysicsWorld)_).SplitImpulse = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return GravityAttr;
            yield return PhysicsFPSAttr;
            yield return MaxSubstepsAttr;
            yield return SolverIterationsAttr;
            yield return NetMaxAngularVelAttr;
            yield return InterpolationAttr;
            yield return InternalEdgeUtilityAttr;
            yield return SplitImpulseAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected Vector3 _gravity = new Vector3(0f, -9.81f, 0f);

        protected int _physicsFPS = 60;

        protected int _maxSubsteps = 0;

        protected int _solverIterations = 10;

        protected float _netMaxAngularVel = 100f;

        protected bool _interpolation = true;

        protected bool _internalEdgeUtility = true;

        protected bool _splitImpulse = false;

        public Vector3 Gravity
        {
            get => _gravity;
            set => _gravity = value;
        }

        public int PhysicsFPS
        {
            get => _physicsFPS;
            set => _physicsFPS = value;
        }

        public int MaxSubsteps
        {
            get => _maxSubsteps;
            set => _maxSubsteps = value;
        }

        public int SolverIterations
        {
            get => _solverIterations;
            set => _solverIterations = value;
        }

        public float NetMaxAngularVel
        {
            get => _netMaxAngularVel;
            set => _netMaxAngularVel = value;
        }

        public bool Interpolation
        {
            get => _interpolation;
            set => _interpolation = value;
        }

        public bool InternalEdgeUtility
        {
            get => _internalEdgeUtility;
            set => _internalEdgeUtility = value;
        }

        public bool SplitImpulse
        {
            get => _splitImpulse;
            set => _splitImpulse = value;
        }
    }
}
