using System.Collections;

namespace UnityToRebelFork.Editor
{
    public class UniqueIdVisitor
    {
        private int _id = 0;

        public void Vist(Node node)
        {
            node.Id = ++_id;
            foreach (var component in node.Components)
            {
                Vist(component);
            }
            foreach (var child in node.Children)
            {
                Vist(child);
            }
        }
        public void Vist(Component component)
        {
            component.Id = ++_id;
        }
    }
}