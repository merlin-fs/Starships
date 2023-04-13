using System;
using Unity.Collections;
using Unity.Properties;

namespace Common.Core
{
    [Serializable]
    public struct ObjectID : IEquatable<ObjectID>
    {
        private FixedString64Bytes m_ID;
        [CreateProperty]
        private FixedString64Bytes ID => m_ID;

        private ObjectID(FixedString64Bytes id)
        {
            m_ID = id;
        }

        public static ObjectID Create(string value) 
        {
            FixedString64Bytes fs = default;
            FixedStringMethods.CopyFromTruncated(ref fs, value);
            return new ObjectID(fs);
        }

        public override string ToString()
        {
            return m_ID.ToString();
        }
        public bool Equals(ObjectID other)
        {
            return m_ID == other.m_ID;
        }

        public override bool Equals(object obj)
        {
            int result;
            if (obj is ObjectID)
            {
                ObjectID other = (ObjectID)obj;
                result = (Equals(other) ? 1 : 0);
            }
            else
            {
                result = 0;
            }

            return (byte)result != 0;
        }

        public static bool operator == (ObjectID left, ObjectID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObjectID left, ObjectID right)
        {
            return !left.Equals(right);
        }

        public static implicit operator ObjectID(string value) => new ObjectID(value);

        public override int GetHashCode()
        {
            return m_ID.GetHashCode();
        }
    }
}