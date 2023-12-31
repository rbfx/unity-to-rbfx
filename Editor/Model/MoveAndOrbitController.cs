// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class MoveAndOrbitController: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<ResourceRef> InputMapAttr = new ("Input Map", VariantType.VarResourceRef, new Variant(new ResourceRef("InputMap", "")), _=>((MoveAndOrbitController)_).InputMap, (_,v)=>((MoveAndOrbitController)_).InputMap = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return InputMapAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected ResourceRef _inputMap = new ResourceRef("InputMap", "");

        public ResourceRef InputMap
        {
            get => _inputMap;
            set => _inputMap = value;
        }
    }
}
