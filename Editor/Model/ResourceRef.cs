using System;

namespace UnityToRebelFork.Editor
{
    public class ResourceRef : IEquatable<ResourceRef>
    {
        public bool Equals(ResourceRef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _type == other._type && _path == other._path;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ResourceRef)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_type, _path);
        }

        public static bool operator ==(ResourceRef left, ResourceRef right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ResourceRef left, ResourceRef right)
        {
            return !Equals(left, right);
        }

        private readonly string _type;
        private readonly string _path;

        public ResourceRef(string type, string path)
        {
            _type = type;
            _path = path;
        }

        public static ResourceRef Create<T>(string path)
        {
            return new ResourceRef(typeof(T).Name, path);
        }

        public override string ToString()
        {
            return $"{_type};{_path}";
        }
    }
}