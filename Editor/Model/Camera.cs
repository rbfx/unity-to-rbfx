// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class Camera: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((Camera)_).IsEnabled, (_,v)=>((Camera)_).IsEnabled = v);
        protected static readonly RbfxAttribute<float> NearClipAttr = new ("Near Clip", VariantType.VarFloat, new Variant(0.1f), _=>((Camera)_).NearClip, (_,v)=>((Camera)_).NearClip = v);
        protected static readonly RbfxAttribute<float> FarClipAttr = new ("Far Clip", VariantType.VarFloat, new Variant(1000f), _=>((Camera)_).FarClip, (_,v)=>((Camera)_).FarClip = v);
        protected static readonly RbfxAttribute<float> FOVAttr = new ("FOV", VariantType.VarFloat, new Variant(45f), _=>((Camera)_).FOV, (_,v)=>((Camera)_).FOV = v);
        protected static readonly RbfxAttribute<float> AspectRatioAttr = new ("Aspect Ratio", VariantType.VarFloat, new Variant(1f), _=>((Camera)_).AspectRatio, (_,v)=>((Camera)_).AspectRatio = v);
        protected static readonly RbfxAttribute<string> FillModeAttr = new ("Fill Mode", VariantType.VarString, new Variant(UnityToRebelFork.Editor.FillMode.Solid), _=>((Camera)_).FillMode, (_,v)=>((Camera)_).FillMode = v);
        protected static readonly RbfxAttribute<bool> AutoAspectRatioAttr = new ("Auto Aspect Ratio", VariantType.VarBool, new Variant(true), _=>((Camera)_).AutoAspectRatio, (_,v)=>((Camera)_).AutoAspectRatio = v);
        protected static readonly RbfxAttribute<bool> OrthographicAttr = new ("Orthographic", VariantType.VarBool, new Variant(false), _=>((Camera)_).Orthographic, (_,v)=>((Camera)_).Orthographic = v);
        protected static readonly RbfxAttribute<float> OrthographicSizeAttr = new ("Orthographic Size", VariantType.VarFloat, new Variant(20f), _=>((Camera)_).OrthographicSize, (_,v)=>((Camera)_).OrthographicSize = v);
        protected static readonly RbfxAttribute<float> ZoomAttr = new ("Zoom", VariantType.VarFloat, new Variant(1f), _=>((Camera)_).Zoom, (_,v)=>((Camera)_).Zoom = v);
        protected static readonly RbfxAttribute<float> LODBiasAttr = new ("LOD Bias", VariantType.VarFloat, new Variant(1f), _=>((Camera)_).LODBias, (_,v)=>((Camera)_).LODBias = v);
        protected static readonly RbfxAttribute<int> ViewMaskAttr = new ("View Mask", VariantType.VarInt, new Variant(-1), _=>((Camera)_).ViewMask, (_,v)=>((Camera)_).ViewMask = v);
        protected static readonly RbfxAttribute<int> ZoneMaskAttr = new ("Zone Mask", VariantType.VarInt, new Variant(-1), _=>((Camera)_).ZoneMask, (_,v)=>((Camera)_).ZoneMask = v);
        protected static readonly RbfxAttribute<int> ViewOverrideFlagsAttr = new ("View Override Flags", VariantType.VarInt, new Variant(0), _=>((Camera)_).ViewOverrideFlags, (_,v)=>((Camera)_).ViewOverrideFlags = v);
        protected static readonly RbfxAttribute<Vector2> ProjectionOffsetAttr = new ("Projection Offset", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((Camera)_).ProjectionOffset, (_,v)=>((Camera)_).ProjectionOffset = v);
        protected static readonly RbfxAttribute<Vector4> ReflectionPlaneAttr = new ("Reflection Plane", VariantType.VarVector4, new Variant(new Vector4(0f, 1f, 0f, 0f)), _=>((Camera)_).ReflectionPlane, (_,v)=>((Camera)_).ReflectionPlane = v);
        protected static readonly RbfxAttribute<Vector4> ClipPlaneAttr = new ("Clip Plane", VariantType.VarVector4, new Variant(new Vector4(0f, 1f, 0f, 0f)), _=>((Camera)_).ClipPlane, (_,v)=>((Camera)_).ClipPlane = v);
        protected static readonly RbfxAttribute<bool> UseReflectionAttr = new ("Use Reflection", VariantType.VarBool, new Variant(false), _=>((Camera)_).UseReflection, (_,v)=>((Camera)_).UseReflection = v);
        protected static readonly RbfxAttribute<bool> UseClippingAttr = new ("Use Clipping", VariantType.VarBool, new Variant(false), _=>((Camera)_).UseClipping, (_,v)=>((Camera)_).UseClipping = v);
        protected static readonly RbfxAttribute<bool> DrawDebugGeometryAttr = new ("Draw Debug Geometry", VariantType.VarBool, new Variant(true), _=>((Camera)_).DrawDebugGeometry, (_,v)=>((Camera)_).DrawDebugGeometry = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return NearClipAttr;
            yield return FarClipAttr;
            yield return FOVAttr;
            yield return AspectRatioAttr;
            yield return FillModeAttr;
            yield return AutoAspectRatioAttr;
            yield return OrthographicAttr;
            yield return OrthographicSizeAttr;
            yield return ZoomAttr;
            yield return LODBiasAttr;
            yield return ViewMaskAttr;
            yield return ZoneMaskAttr;
            yield return ViewOverrideFlagsAttr;
            yield return ProjectionOffsetAttr;
            yield return ReflectionPlaneAttr;
            yield return ClipPlaneAttr;
            yield return UseReflectionAttr;
            yield return UseClippingAttr;
            yield return DrawDebugGeometryAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected float _nearClip = 0.1f;

        protected float _farClip = 1000f;

        protected float _fOV = 45f;

        protected float _aspectRatio = 1f;

        protected string _fillMode = UnityToRebelFork.Editor.FillMode.Solid;

        protected bool _autoAspectRatio = true;

        protected bool _orthographic = false;

        protected float _orthographicSize = 20f;

        protected float _zoom = 1f;

        protected float _lODBias = 1f;

        protected int _viewMask = -1;

        protected int _zoneMask = -1;

        protected int _viewOverrideFlags = 0;

        protected Vector2 _projectionOffset = new Vector2(0f, 0f);

        protected Vector4 _reflectionPlane = new Vector4(0f, 1f, 0f, 0f);

        protected Vector4 _clipPlane = new Vector4(0f, 1f, 0f, 0f);

        protected bool _useReflection = false;

        protected bool _useClipping = false;

        protected bool _drawDebugGeometry = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public float NearClip
        {
            get => _nearClip;
            set => _nearClip = value;
        }

        public float FarClip
        {
            get => _farClip;
            set => _farClip = value;
        }

        public float FOV
        {
            get => _fOV;
            set => _fOV = value;
        }

        public float AspectRatio
        {
            get => _aspectRatio;
            set => _aspectRatio = value;
        }

        public string FillMode
        {
            get => _fillMode;
            set => _fillMode = value;
        }

        public bool AutoAspectRatio
        {
            get => _autoAspectRatio;
            set => _autoAspectRatio = value;
        }

        public bool Orthographic
        {
            get => _orthographic;
            set => _orthographic = value;
        }

        public float OrthographicSize
        {
            get => _orthographicSize;
            set => _orthographicSize = value;
        }

        public float Zoom
        {
            get => _zoom;
            set => _zoom = value;
        }

        public float LODBias
        {
            get => _lODBias;
            set => _lODBias = value;
        }

        public int ViewMask
        {
            get => _viewMask;
            set => _viewMask = value;
        }

        public int ZoneMask
        {
            get => _zoneMask;
            set => _zoneMask = value;
        }

        public int ViewOverrideFlags
        {
            get => _viewOverrideFlags;
            set => _viewOverrideFlags = value;
        }

        public Vector2 ProjectionOffset
        {
            get => _projectionOffset;
            set => _projectionOffset = value;
        }

        public Vector4 ReflectionPlane
        {
            get => _reflectionPlane;
            set => _reflectionPlane = value;
        }

        public Vector4 ClipPlane
        {
            get => _clipPlane;
            set => _clipPlane = value;
        }

        public bool UseReflection
        {
            get => _useReflection;
            set => _useReflection = value;
        }

        public bool UseClipping
        {
            get => _useClipping;
            set => _useClipping = value;
        }

        public bool DrawDebugGeometry
        {
            get => _drawDebugGeometry;
            set => _drawDebugGeometry = value;
        }
    }
}
