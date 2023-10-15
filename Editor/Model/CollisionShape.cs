// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class CollisionShape: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((CollisionShape)_).IsEnabled, (_,v)=>((CollisionShape)_).IsEnabled = v);
        protected static readonly RbfxAttribute<string> ShapeTypeAttr = new ("Shape Type", VariantType.VarString, new Variant(UnityToRebelFork.Editor.ShapeType.Box), _=>((CollisionShape)_).ShapeType, (_,v)=>((CollisionShape)_).ShapeType = v);
        protected static readonly RbfxAttribute<Vector3> SizeAttr = new ("Size", VariantType.VarVector3, new Variant(new Vector3(1f, 1f, 1f)), _=>((CollisionShape)_).Size, (_,v)=>((CollisionShape)_).Size = v);
        protected static readonly RbfxAttribute<Vector3> OffsetPositionAttr = new ("Offset Position", VariantType.VarVector3, new Variant(new Vector3(0f, 0f, 0f)), _=>((CollisionShape)_).OffsetPosition, (_,v)=>((CollisionShape)_).OffsetPosition = v);
        protected static readonly RbfxAttribute<Quaternion> OffsetRotationAttr = new ("Offset Rotation", VariantType.VarQuaternion, new Variant(new Quaternion(0f, 0f, 0f, 1f)), _=>((CollisionShape)_).OffsetRotation, (_,v)=>((CollisionShape)_).OffsetRotation = v);
        protected static readonly RbfxAttribute<ResourceRef> ModelAttr = new ("Model", VariantType.VarResourceRef, new Variant(new ResourceRef("Model", "")), _=>((CollisionShape)_).Model, (_,v)=>((CollisionShape)_).Model = v);
        protected static readonly RbfxAttribute<int> LODLevelAttr = new ("LOD Level", VariantType.VarInt, new Variant(0), _=>((CollisionShape)_).LODLevel, (_,v)=>((CollisionShape)_).LODLevel = v);
        protected static readonly RbfxAttribute<float> CollisionMarginAttr = new ("Collision Margin", VariantType.VarFloat, new Variant(0.04f), _=>((CollisionShape)_).CollisionMargin, (_,v)=>((CollisionShape)_).CollisionMargin = v);
        protected static readonly RbfxAttribute<int> CustomGeometryComponentIDAttr = new ("CustomGeometry ComponentID", VariantType.VarInt, new Variant(0), _=>((CollisionShape)_).CustomGeometryComponentID, (_,v)=>((CollisionShape)_).CustomGeometryComponentID = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return ShapeTypeAttr;
            yield return SizeAttr;
            yield return OffsetPositionAttr;
            yield return OffsetRotationAttr;
            yield return ModelAttr;
            yield return LODLevelAttr;
            yield return CollisionMarginAttr;
            yield return CustomGeometryComponentIDAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected string _shapeType = UnityToRebelFork.Editor.ShapeType.Box;

        protected Vector3 _size = new Vector3(1f, 1f, 1f);

        protected Vector3 _offsetPosition = new Vector3(0f, 0f, 0f);

        protected Quaternion _offsetRotation = new Quaternion(0f, 0f, 0f, 1f);

        protected ResourceRef _model = new ResourceRef("Model", "");

        protected int _lODLevel = 0;

        protected float _collisionMargin = 0.04f;

        protected int _customGeometryComponentID = 0;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public string ShapeType
        {
            get => _shapeType;
            set => _shapeType = value;
        }

        public Vector3 Size
        {
            get => _size;
            set => _size = value;
        }

        public Vector3 OffsetPosition
        {
            get => _offsetPosition;
            set => _offsetPosition = value;
        }

        public Quaternion OffsetRotation
        {
            get => _offsetRotation;
            set => _offsetRotation = value;
        }

        public ResourceRef Model
        {
            get => _model;
            set => _model = value;
        }

        public int LODLevel
        {
            get => _lODLevel;
            set => _lODLevel = value;
        }

        public float CollisionMargin
        {
            get => _collisionMargin;
            set => _collisionMargin = value;
        }

        public int CustomGeometryComponentID
        {
            get => _customGeometryComponentID;
            set => _customGeometryComponentID = value;
        }
    }
}
