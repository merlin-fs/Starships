using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Ext;
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
            return m_Handles[value];
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
        
        private static Dictionary<Enum, LogicHandle> m_Handles = new Dictionary<Enum, LogicHandle>();

        static LogicHandle InternalFromEnum(Enum value)
        {
            return new LogicHandle(
                new Unity.Mathematics.int2(value.GetType().FullName.GetHashCode(), value.GetHashCode()).GetHashCode(),
                $"{value} ({value.GetType().DeclaringType?.Name})");
        }

        static LogicHandle()
        {
            var names = new HashSet<string> {"State", "Action", "Stat"};
            
            var types = typeof(Logic.IStateData).GetDerivedTypes(true)
                .SelectMany(t => t.GetNestedTypes())
                .Where(t => t.IsEnum && names.Contains(t.Name));

            foreach (var iter in types)
            {
                foreach (var e in iter.GetEnumValues())
                {
                    var key = (Enum)e;
                    var value = InternalFromEnum(key);
                    m_Handles.Add(key, value);
                }
            }
        } 
    }
}