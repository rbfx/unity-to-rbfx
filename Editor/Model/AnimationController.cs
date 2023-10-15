// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class AnimationController: UnityToRebelFork.Editor.AnimationStateSource
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((AnimationController)_).IsEnabled, (_,v)=>((AnimationController)_).IsEnabled = v);
        protected static readonly RbfxAttribute<bool> ResetSkeletonAttr = new ("Reset Skeleton", VariantType.VarBool, new Variant(false), _=>((AnimationController)_).ResetSkeleton, (_,v)=>((AnimationController)_).ResetSkeleton = v);
        protected static readonly RbfxAttribute<VariantList> AnimationsAttr = new ("Animations", VariantType.VarVariantList, new Variant(new VariantList() {  }), _=>((AnimationController)_).Animations, (_,v)=>((AnimationController)_).Animations = v);
        protected static readonly RbfxAttribute<bool> UpdatePoseAttr = new ("Update Pose", VariantType.VarBool, new Variant(false), _=>((AnimationController)_).UpdatePose, (_,v)=>((AnimationController)_).UpdatePose = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return ResetSkeletonAttr;
            yield return AnimationsAttr;
            yield return UpdatePoseAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected bool _resetSkeleton = false;

        protected VariantList _animations = new VariantList() {  };

        protected bool _updatePose = false;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public bool ResetSkeleton
        {
            get => _resetSkeleton;
            set => _resetSkeleton = value;
        }

        public VariantList Animations
        {
            get => _animations;
            set => _animations = value;
        }

        public bool UpdatePose
        {
            get => _updatePose;
            set => _updatePose = value;
        }
    }
}
