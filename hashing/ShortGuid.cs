using System;

namespace hashing
{
    /// <summary>
    /// http://www.singular.co.nz/2007/12/shortguid-a-shorter-and-url-friendly-guid-in-c-sharp/
    /// </summary>
    public struct ShortGuid
    {
        /// <summary>
        /// A read-only instance of the ShortGuid class whose value is guaranteed to be all zeroes.
        /// </summary>
        public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

        readonly Guid _guid;

        public ShortGuid(string value)
            : this()
        {
            Value = value;
            _guid = Decode(value);
        }

        public ShortGuid(Guid guid)
            : this()
        {
            Value = Encode(guid);
            _guid = guid;
        }

        public Guid Guid
        {
            get { return _guid; }
        }

        public string Value { get; private set; }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is ShortGuid)
            {
                return _guid.Equals(((ShortGuid)obj)._guid);
            }

            if (obj is Guid)
            {
                return _guid.Equals((Guid)obj);
            }

            var value = obj as string;
            return value != null && _guid.Equals(new ShortGuid(value)._guid);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public static ShortGuid NewGuid()
        {
            return new ShortGuid(Guid.NewGuid());
        }

        public static string Encode(string value)
        {
            var guid = new Guid(value);
            return Encode(guid);
        }

        public static string Encode(Guid guid)
        {
            var encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded
                .Replace("/", "_")
                .Replace("+", "-");
            return encoded.Substring(0, 22);
        }

        public static Guid Decode(string value)
        {
            value = value
                .Replace("_", "/")
                .Replace("-", "+");
            var buffer = Convert.FromBase64String(value + "==");
            return new Guid(buffer);
        }

        public static bool operator ==(ShortGuid x, ShortGuid y)
        {
            return x._guid == y._guid;
        }

        public static bool operator !=(ShortGuid x, ShortGuid y)
        {
            return !(x == y);
        }

        public static implicit operator string (ShortGuid shortGuid)
        {
            return shortGuid.Value;
        }

        public static implicit operator Guid(ShortGuid shortGuid)
        {
            return shortGuid._guid;
        }

        public static implicit operator ShortGuid(string shortGuid)
        {
            return new ShortGuid(shortGuid);
        }

        public static implicit operator ShortGuid(Guid guid)
        {
            return new ShortGuid(guid);
        }
    }
}