using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public interface IMorphTarget
    {
        string Name { get; }
        IList<Vector3> Vertices { get; }
        IList<Vector3> Normals { get; }
        IList<Vector3> Tangents { get; }
    }
}