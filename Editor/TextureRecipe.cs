using System;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class TextureRecipe : IEquatable<TextureRecipe>
    {
        public static readonly TextureRecipe Default = new TextureRecipe();

        public bool Equals(TextureRecipe other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(RChannel, other.RChannel) && RSourceRange.Equals(other.RSourceRange) && RDestinationRange.Equals(other.RDestinationRange) && RMask.Equals(other.RMask) && Equals(GChannel, other.GChannel) && GSourceRange.Equals(other.GSourceRange) && GDestinationRange.Equals(other.GDestinationRange) && GMask.Equals(other.GMask) && Equals(BChannel, other.BChannel) && BSourceRange.Equals(other.BSourceRange) && BDestinationRange.Equals(other.BDestinationRange) && BMask.Equals(other.BMask) && Equals(AChannel, other.AChannel) && ASourceRange.Equals(other.ASourceRange) && ADestinationRange.Equals(other.ADestinationRange) && AMask.Equals(other.AMask);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextureRecipe)obj);
        }

        public override int GetHashCode()
        {
            return Helpers.CombineHashCodes(
                RChannel,
                RSourceRange,
                RDestinationRange,
                RMask,
                GChannel,
                GSourceRange,
                GDestinationRange,
                GMask,
                BChannel,
                BSourceRange,
                BDestinationRange,
                BMask,
                AChannel,
                ASourceRange,
                ADestinationRange,
                AMask);
        }

        public static bool operator ==(TextureRecipe left, TextureRecipe right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TextureRecipe left, TextureRecipe right)
        {
            return !Equals(left, right);
        }

        public UnityEngine.Texture RChannel;
        public Vector2 RSourceRange = new Vector2(0, 1);
        public Vector2 RDestinationRange = new Vector2(0, 1);
        public Vector4 RMask = new Vector4(1, 0, 0, 0);

        public UnityEngine.Texture GChannel;
        public Vector2 GSourceRange = new Vector2(0, 1);
        public Vector2 GDestinationRange = new Vector2(0, 1);
        public Vector4 GMask = new Vector4(0, 1, 0, 0);

        public UnityEngine.Texture BChannel;
        public Vector2 BSourceRange = new Vector2(0, 1);
        public Vector2 BDestinationRange = new Vector2(0, 1);
        public Vector4 BMask = new Vector4(0, 0, 1, 0);

        public UnityEngine.Texture AChannel;
        public Vector2 ASourceRange = new Vector2(0, 1);
        public Vector2 ADestinationRange = new Vector2(0, 1);
        public Vector4 AMask = new Vector4(0, 0, 0, 1);

    }
}