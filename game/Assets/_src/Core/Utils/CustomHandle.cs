using System;
using Unity.Entities;

namespace Game.Core
{
    public readonly partial struct CustomHandle: IEquatable<CustomHandle>, IComparable<CustomHandle>, IBufferElementData
    {
        private readonly int m_ID;

        public static CustomHandle Null { get; } = new CustomHandle(0);

        public static CustomHandle From<T>()
        {
            return Manager.GetHandle<T>();
        }

        public static void Registry(Type type) => Manager.Registry(type); 
        public static void Registry<T>() => Registry(typeof(T));
        private CustomHandle(int id) => m_ID = id;   

        public static bool Equals(CustomHandle l1, CustomHandle l2) => l1.m_ID == l2.m_ID;

        public override string ToString() => Manager.GetName(this);
        
        public bool Equals(CustomHandle other) => m_ID == other.m_ID;

        public override bool Equals(object obj) => obj is CustomHandle lh && Equals(lh);

        public override int GetHashCode() => m_ID;

        public static bool operator ==(CustomHandle left, CustomHandle right) => left.Equals(right);

        public static bool operator !=(CustomHandle left, CustomHandle right) => !left.Equals(right);

        public int CompareTo(CustomHandle other) => m_ID - other.m_ID;
    }
}