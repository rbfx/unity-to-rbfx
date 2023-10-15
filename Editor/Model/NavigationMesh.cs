// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class NavigationMesh: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<int> TileSizeAttr = new ("Tile Size", VariantType.VarInt, new Variant(128), _=>((NavigationMesh)_).TileSize, (_,v)=>((NavigationMesh)_).TileSize = v);
        protected static readonly RbfxAttribute<float> CellSizeAttr = new ("Cell Size", VariantType.VarFloat, new Variant(0.3f), _=>((NavigationMesh)_).CellSize, (_,v)=>((NavigationMesh)_).CellSize = v);
        protected static readonly RbfxAttribute<float> CellHeightAttr = new ("Cell Height", VariantType.VarFloat, new Variant(0.2f), _=>((NavigationMesh)_).CellHeight, (_,v)=>((NavigationMesh)_).CellHeight = v);
        protected static readonly RbfxAttribute<float> AgentHeightAttr = new ("Agent Height", VariantType.VarFloat, new Variant(2f), _=>((NavigationMesh)_).AgentHeight, (_,v)=>((NavigationMesh)_).AgentHeight = v);
        protected static readonly RbfxAttribute<float> AgentRadiusAttr = new ("Agent Radius", VariantType.VarFloat, new Variant(0.6f), _=>((NavigationMesh)_).AgentRadius, (_,v)=>((NavigationMesh)_).AgentRadius = v);
        protected static readonly RbfxAttribute<float> AgentMaxClimbAttr = new ("Agent Max Climb", VariantType.VarFloat, new Variant(0.9f), _=>((NavigationMesh)_).AgentMaxClimb, (_,v)=>((NavigationMesh)_).AgentMaxClimb = v);
        protected static readonly RbfxAttribute<float> AgentMaxSlopeAttr = new ("Agent Max Slope", VariantType.VarFloat, new Variant(45f), _=>((NavigationMesh)_).AgentMaxSlope, (_,v)=>((NavigationMesh)_).AgentMaxSlope = v);
        protected static readonly RbfxAttribute<float> RegionMinSizeAttr = new ("Region Min Size", VariantType.VarFloat, new Variant(8f), _=>((NavigationMesh)_).RegionMinSize, (_,v)=>((NavigationMesh)_).RegionMinSize = v);
        protected static readonly RbfxAttribute<float> RegionMergeSizeAttr = new ("Region Merge Size", VariantType.VarFloat, new Variant(20f), _=>((NavigationMesh)_).RegionMergeSize, (_,v)=>((NavigationMesh)_).RegionMergeSize = v);
        protected static readonly RbfxAttribute<float> EdgeMaxLengthAttr = new ("Edge Max Length", VariantType.VarFloat, new Variant(12f), _=>((NavigationMesh)_).EdgeMaxLength, (_,v)=>((NavigationMesh)_).EdgeMaxLength = v);
        protected static readonly RbfxAttribute<float> EdgeMaxErrorAttr = new ("Edge Max Error", VariantType.VarFloat, new Variant(1.3f), _=>((NavigationMesh)_).EdgeMaxError, (_,v)=>((NavigationMesh)_).EdgeMaxError = v);
        protected static readonly RbfxAttribute<float> DetailSampleDistanceAttr = new ("Detail Sample Distance", VariantType.VarFloat, new Variant(6f), _=>((NavigationMesh)_).DetailSampleDistance, (_,v)=>((NavigationMesh)_).DetailSampleDistance = v);
        protected static readonly RbfxAttribute<float> DetailSampleMaxErrorAttr = new ("Detail Sample Max Error", VariantType.VarFloat, new Variant(1f), _=>((NavigationMesh)_).DetailSampleMaxError, (_,v)=>((NavigationMesh)_).DetailSampleMaxError = v);
        protected static readonly RbfxAttribute<Vector3> BoundingBoxPaddingAttr = new ("Bounding Box Padding", VariantType.VarVector3, new Variant(new Vector3(1f, 1f, 1f)), _=>((NavigationMesh)_).BoundingBoxPadding, (_,v)=>((NavigationMesh)_).BoundingBoxPadding = v);
        protected static readonly RbfxAttribute<Buffer> NavigationDataAttr = new ("Navigation Data", VariantType.VarBuffer, new Variant(new Buffer()), _=>((NavigationMesh)_).NavigationData, (_,v)=>((NavigationMesh)_).NavigationData = v);
        protected static readonly RbfxAttribute<string> PartitionTypeAttr = new ("Partition Type", VariantType.VarString, new Variant(UnityToRebelFork.Editor.PartitionType.watershed), _=>((NavigationMesh)_).PartitionType, (_,v)=>((NavigationMesh)_).PartitionType = v);
        protected static readonly RbfxAttribute<bool> DrawOffMeshConnectionsAttr = new ("Draw OffMeshConnections", VariantType.VarBool, new Variant(false), _=>((NavigationMesh)_).DrawOffMeshConnections, (_,v)=>((NavigationMesh)_).DrawOffMeshConnections = v);
        protected static readonly RbfxAttribute<bool> DrawNavAreasAttr = new ("Draw NavAreas", VariantType.VarBool, new Variant(false), _=>((NavigationMesh)_).DrawNavAreas, (_,v)=>((NavigationMesh)_).DrawNavAreas = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return TileSizeAttr;
            yield return CellSizeAttr;
            yield return CellHeightAttr;
            yield return AgentHeightAttr;
            yield return AgentRadiusAttr;
            yield return AgentMaxClimbAttr;
            yield return AgentMaxSlopeAttr;
            yield return RegionMinSizeAttr;
            yield return RegionMergeSizeAttr;
            yield return EdgeMaxLengthAttr;
            yield return EdgeMaxErrorAttr;
            yield return DetailSampleDistanceAttr;
            yield return DetailSampleMaxErrorAttr;
            yield return BoundingBoxPaddingAttr;
            yield return NavigationDataAttr;
            yield return PartitionTypeAttr;
            yield return DrawOffMeshConnectionsAttr;
            yield return DrawNavAreasAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected int _tileSize = 128;

        protected float _cellSize = 0.3f;

        protected float _cellHeight = 0.2f;

        protected float _agentHeight = 2f;

        protected float _agentRadius = 0.6f;

        protected float _agentMaxClimb = 0.9f;

        protected float _agentMaxSlope = 45f;

        protected float _regionMinSize = 8f;

        protected float _regionMergeSize = 20f;

        protected float _edgeMaxLength = 12f;

        protected float _edgeMaxError = 1.3f;

        protected float _detailSampleDistance = 6f;

        protected float _detailSampleMaxError = 1f;

        protected Vector3 _boundingBoxPadding = new Vector3(1f, 1f, 1f);

        protected Buffer _navigationData = new Buffer();

        protected string _partitionType = UnityToRebelFork.Editor.PartitionType.watershed;

        protected bool _drawOffMeshConnections = false;

        protected bool _drawNavAreas = false;

        public int TileSize
        {
            get => _tileSize;
            set => _tileSize = value;
        }

        public float CellSize
        {
            get => _cellSize;
            set => _cellSize = value;
        }

        public float CellHeight
        {
            get => _cellHeight;
            set => _cellHeight = value;
        }

        public float AgentHeight
        {
            get => _agentHeight;
            set => _agentHeight = value;
        }

        public float AgentRadius
        {
            get => _agentRadius;
            set => _agentRadius = value;
        }

        public float AgentMaxClimb
        {
            get => _agentMaxClimb;
            set => _agentMaxClimb = value;
        }

        public float AgentMaxSlope
        {
            get => _agentMaxSlope;
            set => _agentMaxSlope = value;
        }

        public float RegionMinSize
        {
            get => _regionMinSize;
            set => _regionMinSize = value;
        }

        public float RegionMergeSize
        {
            get => _regionMergeSize;
            set => _regionMergeSize = value;
        }

        public float EdgeMaxLength
        {
            get => _edgeMaxLength;
            set => _edgeMaxLength = value;
        }

        public float EdgeMaxError
        {
            get => _edgeMaxError;
            set => _edgeMaxError = value;
        }

        public float DetailSampleDistance
        {
            get => _detailSampleDistance;
            set => _detailSampleDistance = value;
        }

        public float DetailSampleMaxError
        {
            get => _detailSampleMaxError;
            set => _detailSampleMaxError = value;
        }

        public Vector3 BoundingBoxPadding
        {
            get => _boundingBoxPadding;
            set => _boundingBoxPadding = value;
        }

        public Buffer NavigationData
        {
            get => _navigationData;
            set => _navigationData = value;
        }

        public string PartitionType
        {
            get => _partitionType;
            set => _partitionType = value;
        }

        public bool DrawOffMeshConnections
        {
            get => _drawOffMeshConnections;
            set => _drawOffMeshConnections = value;
        }

        public bool DrawNavAreas
        {
            get => _drawNavAreas;
            set => _drawNavAreas = value;
        }
    }
}
