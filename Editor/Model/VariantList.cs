using System;
using System.Collections.Generic;

namespace UnityToRebelFork.Editor
{
    public class VariantList : List<Variant>, IEquatable<VariantList>
    {
        public bool Equals(VariantList other)
        {
            if (other == null)
            {
                return this.Count == 0;
            }

            if (other.Count != this.Count)
            {
                return false;
            }

            for (var index = 0; index < this.Count; index++)
            {
                if (this[index] != other[index])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VariantList)obj);
        }

        public override int GetHashCode()
        {
            var code = new HashCode();
            foreach (var val in this)
            {
                code.Add(val);
            }
            return code.ToHashCode();
        }

        public static bool operator ==(VariantList left, VariantList right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VariantList left, VariantList right)
        {
            return !Equals(left, right);
        }
    }
}