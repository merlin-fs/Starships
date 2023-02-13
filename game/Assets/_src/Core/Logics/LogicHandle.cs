using System;
using Unity.Collections;
using Unity.Entities;

namespace Game.Model.Logics
{
    public readonly struct LogicHandle : IEquatable<LogicHandle>, IComparable<LogicHandle>, IBufferElementData
    {
        private readonly int m_ID;

        private readonly FixedString64Bytes m_Name;

        public static LogicHandle Null { get; } = new LogicHandle(0, "null");

        public static LogicHandle FromEnum(Enum value)
        {
            return new LogicHandle(
                new Unity.Mathematics.int2(value.GetType().FullName.GetHashCode(), value.GetHashCode()).GetHashCode(),
                $"{value} ({value.GetType().DeclaringType.Name})");
        }

        public static LogicHandle FromEnumTest(Enum value)
        {
            return new LogicHandle(
                new Unity.Mathematics.int2(value.GetType().FullName.GetHashCode(), value.GetHashCode()).GetHashCode(), null);
        }

        public static LogicHandle FromType(Type value)
        {
            return new LogicHandle(
                new Unity.Mathematics.int2(value.FullName.GetHashCode(), value.GetHashCode()).GetHashCode(),
                $"{value} ({value.GetType().DeclaringType.Name})");
        }

        private LogicHandle(int id, string name)
        {
            m_ID = id;
            m_Name = name ?? default(FixedString64Bytes);
        }

        public override string ToString()
        {
            return m_Name.ToString();
        }

        public bool Equals(LogicHandle other)
        {
            return string.Equals(m_ID, other.m_ID);
        }
        
        public override bool Equals(object obj)
        {
            return obj is not null && obj is LogicHandle lh && Equals(lh);
        }

        public override int GetHashCode()
        {
            return m_ID;
        }

        public static bool operator ==(LogicHandle left, LogicHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LogicHandle left, LogicHandle right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(LogicHandle other)
        {
            return m_ID - other.m_ID;
        }
    }
}