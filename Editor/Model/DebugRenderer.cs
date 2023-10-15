// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class DebugRenderer: UnityToRebelFork.Editor.Component
    {
        protected static readonly RbfxAttribute<bool> LineAntialiasAttr = new ("Line Antialias", VariantType.VarBool, new Variant(false), _=>((DebugRenderer)_).LineAntialias, (_,v)=>((DebugRenderer)_).LineAntialias = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return LineAntialiasAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _lineAntialias = false;

        public bool LineAntialias
        {
            get => _lineAntialias;
            set => _lineAntialias = value;
        }
    }
}