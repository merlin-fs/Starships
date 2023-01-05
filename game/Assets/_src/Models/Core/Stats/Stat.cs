using System;
using System.Collections.Concurrent;

using Common.Defs;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;

namespace Game.Model.Stats
{
    [Serializable]
    [WriteGroup(typeof(Stat))]
    public struct Stat : IBufferElementData
    {
        #region debug
        private static readonly ConcurrentDictionary<int, Enum> m_DebugNames = new ConcurrentDictionary<int, Enum>();
        public static string GetName(int statID) => Enum.GetName(m_DebugNames[statID].GetType(), m_DebugNames[statID]);
        [CreateProperty]
        public string StatName => GetName(StatID);
        #endregion
        private static readonly ConcurrentDictionary<Enum, int> m_AllStats = new ConcurrentDictionary<Enum, int>();
        private static Stat m_Null = new Stat() { StatID = 0, };

        [HideInInspector]
        public int StatID;
        private StatValue m_Value;

        public bool IsValid => StatID != 0;

        [CreateProperty]
        public float Value => m_Value.Current.Value;
        public float Normalize => m_Value.Current.Value / m_Value.Current.Max;

        public void ModMull(float value)
        {
            m_Value.Current.Value *= value;
        }

        public void Damage(float value)
        {
            m_Value.Current.Value -= value;
            m_Value.Original.Value = m_Value.Current.Value;
        }

        public void Reset()
        {
            m_Value.Current = m_Value.Original;
        }

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
            var id = FindStat(buff, value);
            var element = new Stat(value)
            {
                m_Value = stat,
            };

            if (id < 0)
                buff.Add(element);
            else
                buff[id] = element;
        }

        public static ref Stat GetRW(DynamicBuffer<Stat> buff, int statId)
        {
            var id = FindStat(buff, statId);
            if (id == -1)
                throw new NotImplementedException($"Stat: {GetName(statId)}");
            return ref buff.ElementAt(id);
        }

        public static Stat GetRO(DynamicBuffer<Stat> buff, int statId)
        {
            var id = FindStat(buff, statId);
            if (id == -1)
                throw new NotImplementedException($"Stat: {GetName(statId)}");
            return buff[id];
        }

        public static bool TryGetStat(DynamicBuffer<Stat> buff, Enum stat, out Stat data)
        {
            data = m_Null;
            var id = FindStat(buff, stat);
            if (id == -1)
                return false;
            data = buff[id];
            return true;
        }

        public static void SetStat(DynamicBuffer<Stat> buff, Enum stat, ref Stat data)
        {
            var id = FindStat(buff, stat);
            if (id == -1)
                return;

            buff.ElementAt(id) = data;
        }

        private static int FindStat(DynamicBuffer<Stat> buff, Enum stat)
        {
            return FindStat(buff, GetID(stat));
        }

        private static int FindStat(DynamicBuffer<Stat> buff, int statId)
        {
            for (int i = 0; i < buff.Length; i++)
            {
                if (buff[i].StatID == statId)
                    return i;
            }
            return -1;
        }

        public static int GetID(Enum value)
        {
            if (!m_AllStats.TryGetValue(value, out int id))
            {
                id = new int2(value.GetType().GetHashCode(), value.GetHashCode()).GetHashCode();
                m_AllStats.TryAdd(value, id);
            }
            return id;
        }

        private Stat(Enum value)
        {
            StatID = GetID(value);
            m_Value = StatValue.Default;
            m_DebugNames.TryAdd(StatID, value);
        }

        public static implicit operator Stat(Enum @enum)
        {
            return new Stat(@enum);
        }
    }

    public static class StatExt
    {
        public static unsafe void AddStat(this DynamicBuffer<Stat> buff, Enum value, StatValue* stat = null)
        {
            Stat.AddStat(buff, value, stat);
        }

        public static void AddStat(this DynamicBuffer<Stat> buff, Enum value, float initial)
        {
            Stat.AddStat(buff, value, initial);
        }

        public static void AddStat(this DynamicBuffer<Stat> buff, Enum value, StatValue stat)
        {
            Stat.AddStat(buff, value, stat);
        }

        public static void AddStat(this DynamicBuffer<Stat> buff, Enum value, IStatValue stat)
        {
            Stat.AddStat(buff, value, stat.Value);
        }

        public static bool TryGetStat(this DynamicBuffer<Stat> buff, Enum stat, out Stat data)
        {
            return Stat.TryGetStat(buff, stat, out data);
        }

        public static ref Stat GetRW(this DynamicBuffer<Stat> buff, Enum stat)
        {
            return ref Stat.GetRW(buff, Stat.GetID(stat));
        }

        public static Stat GetRO(this DynamicBuffer<Stat> buff, Enum stat)
        {
            return Stat.GetRO(buff, Stat.GetID(stat));
        }

        public static ref Stat GetRW(this DynamicBuffer<Stat> buff, int statId)
        {
            return ref Stat.GetRW(buff, statId);
        }

        public static Stat GetRO(this DynamicBuffer<Stat> buff, int statId)
        {
            return Stat.GetRO(buff, statId);
        }
    }
}