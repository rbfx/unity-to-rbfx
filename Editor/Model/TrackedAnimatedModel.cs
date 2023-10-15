// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class TrackedAnimatedModel: UnityToRebelFork.Editor.NetworkBehavior
    {
        protected static readonly RbfxAttribute<bool> TrackOnClientAttr = new ("Track On Client", VariantType.VarBool, new Variant(false), _=>((TrackedAnimatedModel)_).TrackOnClient, (_,v)=>((TrackedAnimatedModel)_).TrackOnClient = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return TrackOnClientAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected bool _trackOnClient = false;

        public bool TrackOnClient
        {
            get => _trackOnClient;
            set => _trackOnClient = value;
        }
    }
}