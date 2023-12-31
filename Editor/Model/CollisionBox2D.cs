// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class CollisionBox2D: UnityToRebelFork.Editor.CollisionShape2D
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((CollisionBox2D)_).IsEnabled, (_,v)=>((CollisionBox2D)_).IsEnabled = v);
        protected static readonly RbfxAttribute<Vector2> SizeAttr = new ("Size", VariantType.VarVector2, new Variant(new Vector2(0.01f, 0.01f)), _=>((CollisionBox2D)_).Size, (_,v)=>((CollisionBox2D)_).Size = v);
        protected static readonly RbfxAttribute<Vector2> CenterAttr = new ("Center", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((CollisionBox2D)_).Center, (_,v)=>((CollisionBox2D)_).Center = v);
        protected static readonly RbfxAttribute<float> AngleAttr = new ("Angle", VariantType.VarFloat, new Variant(0f), _=>((CollisionBox2D)_).Angle, (_,v)=>((CollisionBox2D)_).Angle = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return SizeAttr;
            yield return CenterAttr;
            yield return AngleAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected Vector2 _size = new Vector2(0.01f, 0.01f);

        protected Vector2 _center = new Vector2(0f, 0f);

        protected float _angle = 0f;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public Vector2 Size
        {
            get => _size;
            set => _size = value;
        }

        public Vector2 Center
        {
            get => _center;
            set => _center = value;
        }

        public float Angle
        {
            get => _angle;
            set => _angle = value;
        }
    }
}
