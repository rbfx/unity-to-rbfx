// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class BillboardSet: UnityToRebelFork.Editor.Drawable
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((BillboardSet)_).IsEnabled, (_,v)=>((BillboardSet)_).IsEnabled = v);
        protected static readonly RbfxAttribute<ResourceRef> MaterialAttr = new ("Material", VariantType.VarResourceRef, new Variant(new ResourceRef("Material", "")), _=>((BillboardSet)_).Material, (_,v)=>((BillboardSet)_).Material = v);
        protected static readonly RbfxAttribute<bool> RelativePositionAttr = new ("Relative Position", VariantType.VarBool, new Variant(true), _=>((BillboardSet)_).RelativePosition, (_,v)=>((BillboardSet)_).RelativePosition = v);
        protected static readonly RbfxAttribute<bool> RelativeScaleAttr = new ("Relative Scale", VariantType.VarBool, new Variant(true), _=>((BillboardSet)_).RelativeScale, (_,v)=>((BillboardSet)_).RelativeScale = v);
        protected static readonly RbfxAttribute<bool> SortByDistanceAttr = new ("Sort By Distance", VariantType.VarBool, new Variant(false), _=>((BillboardSet)_).SortByDistance, (_,v)=>((BillboardSet)_).SortByDistance = v);
        protected static readonly RbfxAttribute<bool> FixedScreenSizeAttr = new ("Fixed Screen Size", VariantType.VarBool, new Variant(false), _=>((BillboardSet)_).FixedScreenSize, (_,v)=>((BillboardSet)_).FixedScreenSize = v);
        protected static readonly RbfxAttribute<bool> CanBeOccludedAttr = new ("Can Be Occluded", VariantType.VarBool, new Variant(true), _=>((BillboardSet)_).CanBeOccluded, (_,v)=>((BillboardSet)_).CanBeOccluded = v);
        protected static readonly RbfxAttribute<bool> CastShadowsAttr = new ("Cast Shadows", VariantType.VarBool, new Variant(false), _=>((BillboardSet)_).CastShadows, (_,v)=>((BillboardSet)_).CastShadows = v);
        protected static readonly RbfxAttribute<string> FaceCameraModeAttr = new ("Face Camera Mode", VariantType.VarString, new Variant(UnityToRebelFork.Editor.FaceCameraMode.RotateXYZ), _=>((BillboardSet)_).FaceCameraMode, (_,v)=>((BillboardSet)_).FaceCameraMode = v);
        protected static readonly RbfxAttribute<float> MinAngleAttr = new ("Min Angle", VariantType.VarFloat, new Variant(0f), _=>((BillboardSet)_).MinAngle, (_,v)=>((BillboardSet)_).MinAngle = v);
        protected static readonly RbfxAttribute<float> DrawDistanceAttr = new ("Draw Distance", VariantType.VarFloat, new Variant(0f), _=>((BillboardSet)_).DrawDistance, (_,v)=>((BillboardSet)_).DrawDistance = v);
        protected static readonly RbfxAttribute<float> ShadowDistanceAttr = new ("Shadow Distance", VariantType.VarFloat, new Variant(0f), _=>((BillboardSet)_).ShadowDistance, (_,v)=>((BillboardSet)_).ShadowDistance = v);
        protected static readonly RbfxAttribute<float> AnimationLODBiasAttr = new ("Animation LOD Bias", VariantType.VarFloat, new Variant(1f), _=>((BillboardSet)_).AnimationLODBias, (_,v)=>((BillboardSet)_).AnimationLODBias = v);
        protected static readonly RbfxAttribute<VariantList> BillboardsAttr = new ("Billboards", VariantType.VarVariantList, new Variant(new VariantList() {  }), _=>((BillboardSet)_).Billboards, (_,v)=>((BillboardSet)_).Billboards = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return MaterialAttr;
            yield return RelativePositionAttr;
            yield return RelativeScaleAttr;
            yield return SortByDistanceAttr;
            yield return FixedScreenSizeAttr;
            yield return CanBeOccludedAttr;
            yield return CastShadowsAttr;
            yield return FaceCameraModeAttr;
            yield return MinAngleAttr;
            yield return DrawDistanceAttr;
            yield return ShadowDistanceAttr;
            yield return AnimationLODBiasAttr;
            yield return BillboardsAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected ResourceRef _material = new ResourceRef("Material", "");

        protected bool _relativePosition = true;

        protected bool _relativeScale = true;

        protected bool _sortByDistance = false;

        protected bool _fixedScreenSize = false;

        protected bool _canBeOccluded = true;

        protected bool _castShadows = false;

        protected string _faceCameraMode = UnityToRebelFork.Editor.FaceCameraMode.RotateXYZ;

        protected float _minAngle = 0f;

        protected float _drawDistance = 0f;

        protected float _shadowDistance = 0f;

        protected float _animationLODBias = 1f;

        protected VariantList _billboards = new VariantList() {  };

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public ResourceRef Material
        {
            get => _material;
            set => _material = value;
        }

        public bool RelativePosition
        {
            get => _relativePosition;
            set => _relativePosition = value;
        }

        public bool RelativeScale
        {
            get => _relativeScale;
            set => _relativeScale = value;
        }

        public bool SortByDistance
        {
            get => _sortByDistance;
            set => _sortByDistance = value;
        }

        public bool FixedScreenSize
        {
            get => _fixedScreenSize;
            set => _fixedScreenSize = value;
        }

        public bool CanBeOccluded
        {
            get => _canBeOccluded;
            set => _canBeOccluded = value;
        }

        public bool CastShadows
        {
            get => _castShadows;
            set => _castShadows = value;
        }

        public string FaceCameraMode
        {
            get => _faceCameraMode;
            set => _faceCameraMode = value;
        }

        public float MinAngle
        {
            get => _minAngle;
            set => _minAngle = value;
        }

        public float DrawDistance
        {
            get => _drawDistance;
            set => _drawDistance = value;
        }

        public float ShadowDistance
        {
            get => _shadowDistance;
            set => _shadowDistance = value;
        }

        public float AnimationLODBias
        {
            get => _animationLODBias;
            set => _animationLODBias = value;
        }

        public VariantList Billboards
        {
            get => _billboards;
            set => _billboards = value;
        }
    }
}
