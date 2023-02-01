using System;

using Unity.Collections;

namespace Game.Model.Logics
{
    public readonly struct GoalHandle : IEquatable<GoalHandle>, IComparable<GoalHandle>
    {
        private readonly int m_ID;

        private readonly FixedString64Bytes m_Name;

        public static GoalHandle Null { get; } = new GoalHandle(0, "null");

        public static GoalHandle FromHandle(LogicHandle handle, bool value)
        {
            return new GoalHandle(
                new Unity.Mathematics.int2(handle.GetHashCode(), value.GetHashCode()).GetHashCode(),
                $"{handle} = {value})");
        }

        private GoalHandle(int id, string name)
        {
            m_ID = id;
            m_Name = name;
        }

        public override string ToString()
        {
            return m_Name.ToString();
        }

        public bool Equals(GoalHandle other)
        {
            return string.Equals(m_ID, other.m_ID);
        }
        
        public override bool Equals(object obj)
        {
            return obj is not null && obj is GoalHandle lh && Equals(lh);
        }

        public override int GetHashCode()
        {
            return m_ID;
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
            return m_ID - other.m_ID;
        }
    }
}