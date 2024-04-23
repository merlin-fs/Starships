using System;

using Game.Model.Logics;

using Unity.Entities;

using UnityEngine;

namespace Game.Core
{
    public readonly struct LogicActionHandle: IEquatable<LogicActionHandle>, IComparable<LogicActionHandle>, 
        IBufferElementData, ICustomHandle
    {
        public int ID => m_ID;
        private readonly int m_ID;

        static LogicActionHandle()
        {
            Manager<LogicActionHandle>.Initialize(type =>
            {
                var name = $"{type}";
                var stringId = $"{type.FullName}";
                var handle = new LogicActionHandle(stringId.GetHashCode());
                Manager<LogicActionHandle>.Registry(type, handle, name);
            });
        }
        
        public static LogicActionHandle Null { get; } = new LogicActionHandle(0);

        public static LogicActionHandle From<T>()
            where T : Logic.IAction
        {
            return Manager<LogicActionHandle>.GetHandle<T>();
        }

        public static LogicActionHandle FromType(Type type)
        {
            return Manager<LogicActionHandle>.GetHandle(type);
        }

        public static void Registry(Type type) => Manager<LogicActionHandle>.Registry(type); 
        public static void Registry<T>() where T : Logic.IAction => Registry(typeof(T));
        private LogicActionHandle(int id) => m_ID = id;   

        public static bool Equals(LogicActionHandle l1, LogicActionHandle l2) => l1.m_ID == l2.m_ID;

        public override string ToString() => Manager<LogicActionHandle>.GetName(this);
        
        public bool Equals(LogicActionHandle other) => m_ID == other.m_ID;

        public override bool Equals(object obj) => obj is LogicActionHandle lh && Equals(lh);

        public override int GetHashCode() => m_ID;

        public static bool operator ==(LogicActionHandle left, LogicActionHandle right) => left.Equals(right);

        public static bool operator !=(LogicActionHandle left, LogicActionHandle right) => !left.Equals(right);

        public int CompareTo(LogicActionHandle other) => m_ID - other.m_ID;
    }
}