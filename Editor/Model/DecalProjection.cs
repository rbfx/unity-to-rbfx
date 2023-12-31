// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class DecalProjection: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((DecalProjection)_).IsEnabled, (_,v)=>((DecalProjection)_).IsEnabled = v);
        protected static readonly RbfxAttribute<ResourceRef> MaterialAttr = new ("Material", VariantType.VarResourceRef, new Variant(new ResourceRef("Material", "")), _=>((DecalProjection)_).Material, (_,v)=>((DecalProjection)_).Material = v);
        protected static readonly RbfxAttribute<int> MaxVerticesAttr = new ("Max Vertices", VariantType.VarInt, new Variant(512), _=>((DecalProjection)_).MaxVertices, (_,v)=>((DecalProjection)_).MaxVertices = v);
        protected static readonly RbfxAttribute<int> MaxIndicesAttr = new ("Max Indices", VariantType.VarInt, new Variant(1024), _=>((DecalProjection)_).MaxIndices, (_,v)=>((DecalProjection)_).MaxIndices = v);
        protected static readonly RbfxAttribute<float> NearClipAttr = new ("Near Clip", VariantType.VarFloat, new Variant(-1f), _=>((DecalProjection)_).NearClip, (_,v)=>((DecalProjection)_).NearClip = v);
        protected static readonly RbfxAttribute<float> FarClipAttr = new ("Far Clip", VariantType.VarFloat, new Variant(1f), _=>((DecalProjection)_).FarClip, (_,v)=>((DecalProjection)_).FarClip = v);
        protected static readonly RbfxAttribute<float> FOVAttr = new ("FOV", VariantType.VarFloat, new Variant(45f), _=>((DecalProjection)_).FOV, (_,v)=>((DecalProjection)_).FOV = v);
        protected static readonly RbfxAttribute<float> AspectRatioAttr = new ("Aspect Ratio", VariantType.VarFloat, new Variant(1f), _=>((DecalProjection)_).AspectRatio, (_,v)=>((DecalProjection)_).AspectRatio = v);
        protected static readonly RbfxAttribute<bool> OrthographicAttr = new ("Orthographic", VariantType.VarBool, new Variant(true), _=>((DecalProjection)_).Orthographic, (_,v)=>((DecalProjection)_).Orthographic = v);
        protected static readonly RbfxAttribute<float> OrthographicSizeAttr = new ("Orthographic Size", VariantType.VarFloat, new Variant(1f), _=>((DecalProjection)_).OrthographicSize, (_,v)=>((DecalProjection)_).OrthographicSize = v);
        protected static readonly RbfxAttribute<float> NormalCutoffAttr = new ("Normal Cutoff", VariantType.VarFloat, new Variant(0.1f), _=>((DecalProjection)_).NormalCutoff, (_,v)=>((DecalProjection)_).NormalCutoff = v);
        protected static readonly RbfxAttribute<float> TimeToLiveAttr = new ("Time To Live", VariantType.VarFloat, new Variant(0f), _=>((DecalProjection)_).TimeToLive, (_,v)=>((DecalProjection)_).TimeToLive = v);
        protected static readonly RbfxAttribute<string> AutoremoveModeAttr = new ("Autoremove Mode", VariantType.VarString, new Variant(UnityToRebelFork.Editor.AutoremoveMode.Disabled), _=>((DecalProjection)_).AutoremoveMode, (_,v)=>((DecalProjection)_).AutoremoveMode = v);
        protected static readonly RbfxAttribute<float> ElapsedTimeAttr = new ("Elapsed Time", VariantType.VarFloat, new Variant(0f), _=>((DecalProjection)_).ElapsedTime, (_,v)=>((DecalProjection)_).ElapsedTime = v);
        protected static readonly RbfxAttribute<int> ViewMaskAttr = new ("View Mask", VariantType.VarInt, new Variant(-1), _=>((DecalProjection)_).ViewMask, (_,v)=>((DecalProjection)_).ViewMask = v);
        protected static readonly RbfxAttribute<bool> UpdateAttr = new ("Update", VariantType.VarBool, new Variant(false), _=>((DecalProjection)_).Update, (_,v)=>((DecalProjection)_).Update = v);
        protected static readonly RbfxAttribute<bool> InlineAttr = new ("Inline", VariantType.VarBool, new Variant(false), _=>((DecalProjection)_).Inline, (_,v)=>((DecalProjection)_).Inline = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return MaterialAttr;
            yield return MaxVerticesAttr;
            yield return MaxIndicesAttr;
            yield return NearClipAttr;
            yield return FarClipAttr;
            yield return FOVAttr;
            yield return AspectRatioAttr;
            yield return OrthographicAttr;
            yield return OrthographicSizeAttr;
            yield return NormalCutoffAttr;
            yield return TimeToLiveAttr;
            yield return AutoremoveModeAttr;
            yield return ElapsedTimeAttr;
            yield return ViewMaskAttr;
            yield return UpdateAttr;
            yield return InlineAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected ResourceRef _material = new ResourceRef("Material", "");

        protected int _maxVertices = 512;

        protected int _maxIndices = 1024;

        protected float _nearClip = -1f;

        protected float _farClip = 1f;

        protected float _fOV = 45f;

        protected float _aspectRatio = 1f;

        protected bool _orthographic = true;

        protected float _orthographicSize = 1f;

        protected float _normalCutoff = 0.1f;

        protected float _timeToLive = 0f;

        protected string _autoremoveMode = UnityToRebelFork.Editor.AutoremoveMode.Disabled;

        protected float _elapsedTime = 0f;

        protected int _viewMask = -1;

        protected bool _update = false;

        protected bool _inline = false;

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

        public int MaxVertices
        {
            get => _maxVertices;
            set => _maxVertices = value;
        }

        public int MaxIndices
        {
            get => _maxIndices;
            set => _maxIndices = value;
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

        public float NormalCutoff
        {
            get => _normalCutoff;
            set => _normalCutoff = value;
        }

        public float TimeToLive
        {
            get => _timeToLive;
            set => _timeToLive = value;
        }

        public string AutoremoveMode
        {
            get => _autoremoveMode;
            set => _autoremoveMode = value;
        }

        public float ElapsedTime
        {
            get => _elapsedTime;
            set => _elapsedTime = value;
        }

        public int ViewMask
        {
            get => _viewMask;
            set => _viewMask = value;
        }

        public bool Update
        {
            get => _update;
            set => _update = value;
        }

        public bool Inline
        {
            get => _inline;
            set => _inline = value;
        }
    }
}
