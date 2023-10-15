namespace UnityToRebelFork.Editor
{
    public class EmptyNodeCleanupVisitor
    {
        public bool Visit(Node node)
        {
            for (var index = node.Children.Count - 1; index >= 0; index--)
            {
                var nodeChild = node.Children[index];
                if (Visit(nodeChild))
                    node.Children.RemoveAt(index);
            }

            return node.Children.Count == 0 && node.Components.Count == 0;
        }
    }
}