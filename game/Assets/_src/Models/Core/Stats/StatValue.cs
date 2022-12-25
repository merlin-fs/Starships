using System;
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

        [HideInInspector, SerializeField]
        private ValueStruct m_Original;
        [HideInInspector, SerializeField]
        private ValueStruct m_Value;

        [CreateProperty] public float Min => m_Value.Min;
        [CreateProperty] public float Max => m_Value.Max;
        [CreateProperty] public float Value => m_Value.Value;
        [CreateProperty] public float Normalize => m_Value.Normalize;

        public ValueStruct Original => m_Original;

        public void SetMin(float value) => m_Value.Min = value;
        public void SetMax(float value) => m_Value.Max = value;
        public void SetValue(float value) => m_Value.Value = value;
        public void SetNornalize(float value) => m_Value.Normalize = value;
        
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

                m_Value = new ValueStruct()
                {
                    Min = 0,
                    Max = value,
                    Value = value,
                    Normalize = 1f,
                },
            };
        }

        public static StatValue Default => StatValueExt.Default;
        #endregion
    }

    public static class StatValueExt
    {
        public static readonly StatValue Default = 1f;

        public static void Mull(ref this StatValue stat, float mull)
        {
            stat.SetValue(stat.Value * mull);
        }
    }

}