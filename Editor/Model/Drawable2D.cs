// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class Drawable2D: UnityToRebelFork.Editor.Drawable
    {
        protected static readonly RbfxAttribute<int> LayerAttr = new ("Layer", VariantType.VarInt, new Variant(0), _=>((Drawable2D)_).Layer, (_,v)=>((Drawable2D)_).Layer = v);
        protected static readonly RbfxAttribute<int> OrderinLayerAttr = new ("Order in Layer", VariantType.VarInt, new Variant(0), _=>((Drawable2D)_).OrderinLayer, (_,v)=>((Drawable2D)_).OrderinLayer = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return LayerAttr;
            yield return OrderinLayerAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected int _layer = 0;

        protected int _orderinLayer = 0;

        public int Layer
        {
            get => _layer;
            set => _layer = value;
        }

        public int OrderinLayer
        {
            get => _orderinLayer;
            set => _orderinLayer = value;
        }
    }
}
