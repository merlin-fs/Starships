using System;
using Game.Core;

namespace Game.Model.Logics
{
    public readonly struct GoalHandle : IEquatable<GoalHandle>, IComparable<GoalHandle>
    {
        private readonly EnumHandle m_Handle;
        private readonly int m_ID;
        private readonly bool m_Value;
        public bool Value => m_Value;

        public static GoalHandle Null { get; } = new GoalHandle(EnumHandle.Null, 0, false);

        public static GoalHandle FromHandle(EnumHandle handle, bool value)
        {
            return new GoalHandle(handle, HashCode.Combine(handle, value), value);
        }

        public static GoalHandle FromEnum<T>(T state, bool value)
            where T : struct, IConvertible
        {
            var handle = EnumHandle.FromEnum(state);
            return FromHandle(handle, value);
        }

        private GoalHandle(EnumHandle handle, int id, bool value)
        {
            m_Handle = handle;
            m_ID = id;
            m_Value = value;
        }

        public override string ToString()
        {
            return $"{m_Handle} => {m_Value}";
        }

        public bool Equals(GoalHandle other)
        {
            return m_ID == other.m_ID;
        }
        
        public override bool Equals(object obj)
        {
            return obj is GoalHandle lh && Equals(lh);
        }

        public override int GetHashCode()
        {
            return m_ID.GetHashCode();
        }

        public static bool operator ==(GoalHandle left, GoalHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GoalHandle left, GoalHandle right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(GoalHandle other)
        {
            return m_ID.CompareTo(other.m_ID);
        }
    }
}