// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class DynamicNavigationMesh: UnityToRebelFork.Editor.NavigationMesh
    {
        protected static readonly RbfxAttribute<int> MaxObstaclesAttr = new ("Max Obstacles", VariantType.VarInt, new Variant(1024), _=>((DynamicNavigationMesh)_).MaxObstacles, (_,v)=>((DynamicNavigationMesh)_).MaxObstacles = v);
        protected static readonly RbfxAttribute<int> MaxLayersAttr = new ("Max Layers", VariantType.VarInt, new Variant(16), _=>((DynamicNavigationMesh)_).MaxLayers, (_,v)=>((DynamicNavigationMesh)_).MaxLayers = v);
        protected static readonly RbfxAttribute<bool> DrawObstaclesAttr = new ("Draw Obstacles", VariantType.VarBool, new Variant(false), _=>((DynamicNavigationMesh)_).DrawObstacles, (_,v)=>((DynamicNavigationMesh)_).DrawObstacles = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return MaxObstaclesAttr;
            yield return MaxLayersAttr;
            yield return DrawObstaclesAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected int _maxObstacles = 1024;

        protected int _maxLayers = 16;

        protected bool _drawObstacles = false;

        public int MaxObstacles
        {
            get => _maxObstacles;
            set => _maxObstacles = value;
        }

        public int MaxLayers
        {
            get => _maxLayers;
            set => _maxLayers = value;
        }

        public bool DrawObstacles
        {
            get => _drawObstacles;
            set => _drawObstacles = value;
        }
    }
}
