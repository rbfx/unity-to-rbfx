// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class CollisionEdge2D: UnityToRebelFork.Editor.CollisionShape2D
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((CollisionEdge2D)_).IsEnabled, (_,v)=>((CollisionEdge2D)_).IsEnabled = v);
        protected static readonly RbfxAttribute<Vector2> Vertex1Attr = new ("Vertex 1", VariantType.VarVector2, new Variant(new Vector2(-0.01f, 0f)), _=>((CollisionEdge2D)_).Vertex1, (_,v)=>((CollisionEdge2D)_).Vertex1 = v);
        protected static readonly RbfxAttribute<Vector2> Vertex2Attr = new ("Vertex 2", VariantType.VarVector2, new Variant(new Vector2(0.01f, 0f)), _=>((CollisionEdge2D)_).Vertex2, (_,v)=>((CollisionEdge2D)_).Vertex2 = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return Vertex1Attr;
            yield return Vertex2Attr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected Vector2 _vertex1 = new Vector2(-0.01f, 0f);

        protected Vector2 _vertex2 = new Vector2(0.01f, 0f);

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public Vector2 Vertex1
        {
            get => _vertex1;
            set => _vertex1 = value;
        }

        public Vector2 Vertex2
        {
            get => _vertex2;
            set => _vertex2 = value;
        }
    }
}