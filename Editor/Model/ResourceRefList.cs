using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityToRebelFork.Editor
{
    public class ResourceRefList: List<string>, IEquatable<ResourceRefList>
    {
        public bool Equals(ResourceRefList other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_type != other._type)
                return false;
            if (Count != other.Count)
                return false;
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
            return Equals((ResourceRefList)obj);
        }

        public static bool operator ==(ResourceRefList left, ResourceRefList right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ResourceRefList left, ResourceRefList right)
        {
            return !Equals(left, right);
        }

        private readonly string _type;
        
        public ResourceRefList(string type, params string[] values)
        {
            _type = type;
            AddRange(values);
        }

        public static ResourceRefList Create<T>(params string[] values)
        {
            return new ResourceRefList(typeof(T).Name, values);
        }

        public static ResourceRefList Create<T>(IEnumerable<string> values)
        {
            return new ResourceRefList(typeof(T).Name, values.ToArray());
        }

        public override string ToString()
        {
            return $"{_type};{string.Join(";", this)}";
        }
    }
}