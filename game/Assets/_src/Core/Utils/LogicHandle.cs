using System;

using Game.Model.Logics;

using Unity.Entities;

namespace Game.Core
{
    public readonly struct LogicHandle: IEquatable<LogicHandle>, IComparable<LogicHandle>, 
        IBufferElementData, ICustomHandle
    {
        public int ID => m_ID;
        private readonly int m_ID;

        public static LogicHandle Null { get; } = new LogicHandle(0);

        public static LogicHandle From<T>()
            where T : Logic.IStateData
        {
            return Manager<LogicHandle>.GetHandle<T>();
        }

        public static LogicHandle FromType(Type type)
        {
            return Manager<LogicHandle>.GetHandle(type);
        }

        public static void Registry(Type type) => Manager<LogicHandle>.Registry(type); 
        public static void Registry<T>()
            where T : Logic.IStateData => Registry(typeof(T));
        private LogicHandle(int id) => m_ID = id;   

        public static bool Equals(LogicHandle l1, LogicHandle l2) => l1.m_ID == l2.m_ID;

        public override string ToString() => Manager<LogicHandle>.GetName(this);
        
        public bool Equals(LogicHandle other) => m_ID == other.m_ID;

        public override bool Equals(object obj) => obj is LogicHandle lh && Equals(lh);

        public override int GetHashCode() => m_ID;

        public static bool operator ==(LogicHandle left, LogicHandle right) => left.Equals(right);

        public static bool operator !=(LogicHandle left, LogicHandle right) => !left.Equals(right);

        public int CompareTo(LogicHandle other) => m_ID - other.m_ID;
    }
}