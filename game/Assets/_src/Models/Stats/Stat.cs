using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Stats
{
    [Serializable]
    public struct StatValue
    {
        public float Min;
        public float Max;
        public float Value;
        public float Normalize;
        #region extension
        public static implicit operator StatValue(float value)
        {
            return new StatValue()
            {
                Min = 0,
                Max = value,
                Value = value,
                Normalize = 1f,
            };
        }

        public static StatValue Default => m_Default;
        private static readonly StatValue m_Default = new StatValue()
        {
            Value = 1,
            Normalize = 1,
            Max = 1,
            Min = 1,
        };
        #endregion
    }

    [Serializable]
    public struct Stat : IBufferElementData
    {
        public int StatID;
        public StatValue Value;

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
        }

        public static implicit operator Stat(Enum @enum)
        {
            return new Stat(@enum);
        }
    }
}