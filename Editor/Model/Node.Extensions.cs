using System.Collections.Generic;

namespace UnityToRebelFork.Editor
{
    public partial class Node
    {
        public int Id { get; set; }
        public IList<Node> Children { get; } = new List<Node>();
        public IList<Component> Components { get; } = new List<Component>();
    }
}