// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class CollisionCircle2D: UnityToRebelFork.Editor.CollisionShape2D
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((CollisionCircle2D)_).IsEnabled, (_,v)=>((CollisionCircle2D)_).IsEnabled = v);
        protected static readonly RbfxAttribute<float> RadiusAttr = new ("Radius", VariantType.VarFloat, new Variant(0.01f), _=>((CollisionCircle2D)_).Radius, (_,v)=>((CollisionCircle2D)_).Radius = v);
        protected static readonly RbfxAttribute<Vector2> CenterAttr = new ("Center", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((CollisionCircle2D)_).Center, (_,v)=>((CollisionCircle2D)_).Center = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return RadiusAttr;
            yield return CenterAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected float _radius = 0.01f;

        protected Vector2 _center = new Vector2(0f, 0f);

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public float Radius
        {
            get => _radius;
            set => _radius = value;
        }

        public Vector2 Center
        {
            get => _center;
            set => _center = value;
        }
    }
}
