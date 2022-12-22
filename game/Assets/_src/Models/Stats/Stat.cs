using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;

namespace Game.Model.Stats
{
    [Serializable]
    public struct StatValue
    {
        [Serializable]
        public struct ValueStruct
        {
            public float Min;
            public float Max;
            public float Value;
            public float Normalize;
        }

        [SerializeField]
        private ValueStruct m_Original;
        [SerializeField]
        private ValueStruct m_Value;

        [CreateProperty]
        public float Min { get => m_Value.Min; set => m_Value.Min = value; }
        
        [CreateProperty]
        public float Max { get => m_Value.Max; set => m_Value.Max = value; }
        
        [CreateProperty]
        public float Value { get => m_Value.Value; set => m_Value.Value = value; }
        
        [CreateProperty]
        public float Normalize { get => m_Value.Normalize; set => m_Value.Normalize = value; }

        public ValueStruct Original => m_Original;

        public void Reset()
        {
            m_Value = m_Original;
        }
        #region extension
        public static implicit operator StatValue(float value)
        {
            return new StatValue()
            {
                m_Original = new ValueStruct()
                {
                    Min = 0,
                    Max = value,
                    Value = value,
                    Normalize = 1f,
                },

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