using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public interface IMeshGeometry
    {
        int NumLods { get; }
        Bounds? Bounds { get; }
        MeshTopology Topology { get; }
        IList<int> GetIndices(int lod);
        float GetLodDistance(int lod);
    }
}