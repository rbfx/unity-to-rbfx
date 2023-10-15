using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class PrefabSceneSerializer : ISceneSerializer
    {
        public class XNames
        {
            public static readonly XName resource = XName.Get("resource");
            public static readonly XName attributes = XName.Get("attributes");
            public static readonly XName attribute = XName.Get("attribute");
            public static readonly XName name = XName.Get("name");
            public static readonly XName type = XName.Get("type");
            public static readonly XName value = XName.Get("value");
            public static readonly XName components = XName.Get("components");
            public static readonly XName component = XName.Get("component");
            public static readonly XName _typeName = XName.Get("_typeName");
            public static readonly XName _id = XName.Get("_id");
            public static readonly XName nodes = XName.Get("nodes");
            public static readonly XName node = XName.Get("node");

        }

        public XElement Visit(Scene scene)
        {
            var root = new XElement(XNames.resource, new XAttribute(XNames._id, scene.Id));
            VisitAttributes(root, scene);
            VisitComponents(root, scene.Components);
            VisitNodes(root, scene.Children);
            return root;
        }

        private void VisitAttributes(XElement element, RefCounted obj)
        {
            XElement attributeRoot = null;
            foreach (var attribute in obj.GetTypeAttributes())
            {
                var value = attribute.Get(obj);
                if (value != attribute.DefaultValue)
                {
                    if (attributeRoot == null)
                    {
                        attributeRoot = new XElement(XNames.attributes);
                        element.Add(attributeRoot);
                    }
                    attributeRoot.Add(attribute.ToPrefabXElement(obj));
                }
            }
        }

        private void VisitComponents(XElement element, IList<Component> components)
        {
            if (components == null || components.Count == 0)
                return;
            var componentsRoot = new XElement(XNames.components);
            element.Add(componentsRoot);
            foreach (var component in components)
            {
                componentsRoot.Add(Visit(component));
            }
        }

        private void VisitNodes(XElement element, IList<Node> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return;
            var componentsRoot = new XElement(XNames.nodes);
            element.Add(componentsRoot);
            foreach (var node in nodes)
            {
                componentsRoot.Add(Visit(node));
            }
        }

        private XElement Visit(Component component)
        {
            var c = new XElement(XNames.component, new XAttribute(XNames._id, component.Id), new XAttribute(XNames._typeName, component.GetType().Name));
            VisitAttributes(c, component);
            return c;
        }

        private XElement Visit(Node node)
        {
            var n = new XElement(XNames.node, new XAttribute(XNames._id, node.Id));
            VisitAttributes(n, node);
            VisitComponents(n, node.Components);
            VisitNodes(n, node.Children);
            return n;
        }
    }
}