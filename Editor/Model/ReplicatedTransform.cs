// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class ReplicatedTransform: UnityToRebelFork.Editor.NetworkBehavior
    {
        protected static readonly RbfxAttribute<int> NumUploadAttemptsAttr = new ("Num Upload Attempts", VariantType.VarInt, new Variant(8), _=>((ReplicatedTransform)_).NumUploadAttempts, (_,v)=>((ReplicatedTransform)_).NumUploadAttempts = v);
        protected static readonly RbfxAttribute<bool> ReplicateOwnerAttr = new ("Replicate Owner", VariantType.VarBool, new Variant(false), _=>((ReplicatedTransform)_).ReplicateOwner, (_,v)=>((ReplicatedTransform)_).ReplicateOwner = v);
        protected static readonly RbfxAttribute<bool> PositionTrackOnlyAttr = new ("Position Track Only", VariantType.VarBool, new Variant(false), _=>((ReplicatedTransform)_).PositionTrackOnly, (_,v)=>((ReplicatedTransform)_).PositionTrackOnly = v);
        protected static readonly RbfxAttribute<bool> RotationTrackOnlyAttr = new ("Rotation Track Only", VariantType.VarBool, new Variant(false), _=>((ReplicatedTransform)_).RotationTrackOnly, (_,v)=>((ReplicatedTransform)_).RotationTrackOnly = v);
        protected static readonly RbfxAttribute<float> SmoothingConstantAttr = new ("Smoothing Constant", VariantType.VarFloat, new Variant(15f), _=>((ReplicatedTransform)_).SmoothingConstant, (_,v)=>((ReplicatedTransform)_).SmoothingConstant = v);
        protected static readonly RbfxAttribute<float> MovementThresholdAttr = new ("Movement Threshold", VariantType.VarFloat, new Variant(0.001f), _=>((ReplicatedTransform)_).MovementThreshold, (_,v)=>((ReplicatedTransform)_).MovementThreshold = v);
        protected static readonly RbfxAttribute<float> SnapThresholdAttr = new ("Snap Threshold", VariantType.VarFloat, new Variant(5f), _=>((ReplicatedTransform)_).SnapThreshold, (_,v)=>((ReplicatedTransform)_).SnapThreshold = v);
        protected static readonly RbfxAttribute<bool> SynchronizePositionAttr = new ("Synchronize Position", VariantType.VarBool, new Variant(true), _=>((ReplicatedTransform)_).SynchronizePosition, (_,v)=>((ReplicatedTransform)_).SynchronizePosition = v);
        protected static readonly RbfxAttribute<string> SynchronizeRotationAttr = new ("Synchronize Rotation", VariantType.VarString, new Variant(UnityToRebelFork.Editor.SynchronizeRotation.XYZ), _=>((ReplicatedTransform)_).SynchronizeRotation, (_,v)=>((ReplicatedTransform)_).SynchronizeRotation = v);
        protected static readonly RbfxAttribute<bool> ExtrapolatePositionAttr = new ("Extrapolate Position", VariantType.VarBool, new Variant(true), _=>((ReplicatedTransform)_).ExtrapolatePosition, (_,v)=>((ReplicatedTransform)_).ExtrapolatePosition = v);
        protected static readonly RbfxAttribute<bool> ExtrapolateRotationAttr = new ("Extrapolate Rotation", VariantType.VarBool, new Variant(false), _=>((ReplicatedTransform)_).ExtrapolateRotation, (_,v)=>((ReplicatedTransform)_).ExtrapolateRotation = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return NumUploadAttemptsAttr;
            yield return ReplicateOwnerAttr;
            yield return PositionTrackOnlyAttr;
            yield return RotationTrackOnlyAttr;
            yield return SmoothingConstantAttr;
            yield return MovementThresholdAttr;
            yield return SnapThresholdAttr;
            yield return SynchronizePositionAttr;
            yield return SynchronizeRotationAttr;
            yield return ExtrapolatePositionAttr;
            yield return ExtrapolateRotationAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected int _numUploadAttempts = 8;

        protected bool _replicateOwner = false;

        protected bool _positionTrackOnly = false;

        protected bool _rotationTrackOnly = false;

        protected float _smoothingConstant = 15f;

        protected float _movementThreshold = 0.001f;

        protected float _snapThreshold = 5f;

        protected bool _synchronizePosition = true;

        protected string _synchronizeRotation = UnityToRebelFork.Editor.SynchronizeRotation.XYZ;

        protected bool _extrapolatePosition = true;

        protected bool _extrapolateRotation = false;

        public int NumUploadAttempts
        {
            get => _numUploadAttempts;
            set => _numUploadAttempts = value;
        }

        public bool ReplicateOwner
        {
            get => _replicateOwner;
            set => _replicateOwner = value;
        }

        public bool PositionTrackOnly
        {
            get => _positionTrackOnly;
            set => _positionTrackOnly = value;
        }

        public bool RotationTrackOnly
        {
            get => _rotationTrackOnly;
            set => _rotationTrackOnly = value;
        }

        public float SmoothingConstant
        {
            get => _smoothingConstant;
            set => _smoothingConstant = value;
        }

        public float MovementThreshold
        {
            get => _movementThreshold;
            set => _movementThreshold = value;
        }

        public float SnapThreshold
        {
            get => _snapThreshold;
            set => _snapThreshold = value;
        }

        public bool SynchronizePosition
        {
            get => _synchronizePosition;
            set => _synchronizePosition = value;
        }

        public string SynchronizeRotation
        {
            get => _synchronizeRotation;
            set => _synchronizeRotation = value;
        }

        public bool ExtrapolatePosition
        {
            get => _extrapolatePosition;
            set => _extrapolatePosition = value;
        }

        public bool ExtrapolateRotation
        {
            get => _extrapolateRotation;
            set => _extrapolateRotation = value;
        }
    }
}
