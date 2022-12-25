using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;

namespace Game.Model.Stats
{
    [Serializable]
    public struct Stat : IBufferElementData
    {
#if DEBUG
        private static Dictionary<int, Enum> m_DebugNames = new Dictionary<int, Enum>();
        public static string GetName(int statID) => Enum.GetName(m_DebugNames[statID].GetType(), m_DebugNames[statID]);
#endif
        [HideInInspector]
        public int StatID;
        public StatValue Value;

#if DEBUG
        [CreateProperty]
        public string StatName => GetName(StatID);
#endif
        public static unsafe void AddStat(DynamicBuffer<Stat> buff, Enum value, StatValue* stat = null)
        {
            AddStat(buff, value, (stat != null) ? *stat : StatValue.Default);
        }

        public static unsafe void AddStat(DynamicBuffer<Stat> buff, Enum value, float initial)
        {
            AddStat(buff, value, (StatValue)initial);
        }

        public static unsafe void AddStat(DynamicBuffer<Stat> buff, Enum value, StatValue stat)
        {
            var element = new Stat(value)
            {
                Value = stat,
            };
            buff.Add(element);
        }

        private Stat(Enum value)
        {
            StatID = new int2(value.GetType().GetHashCode(), value.GetHashCode()).GetHashCode();
            Value = StatValue.Default;
#if DEBUG
            m_DebugNames.TryAdd(StatID, value);
#endif
        }

        public static implicit operator Stat(Enum @enum)
        {
            return new Stat(@enum);
        }
    }
}