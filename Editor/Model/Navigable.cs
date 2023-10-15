// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class Navigable: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(true), _=>((Navigable)_).IsEnabled, (_,v)=>((Navigable)_).IsEnabled = v);
        protected static readonly RbfxAttribute<bool> RecursiveAttr = new ("Recursive", VariantType.VarBool, new Variant(true), _=>((Navigable)_).Recursive, (_,v)=>((Navigable)_).Recursive = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return IsEnabledAttr;
            yield return RecursiveAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _isEnabled = true;

        protected bool _recursive = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public bool Recursive
        {
            get => _recursive;
            set => _recursive = value;
        }
    }
}
