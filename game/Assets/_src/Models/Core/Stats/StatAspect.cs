using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Properties;
using Unity.Collections;

namespace Game.Model.Stats
{
    public readonly partial struct StatAspect : IAspect
    {
        private readonly Entity m_Self;
        private readonly RefRO<Root> m_Root;

        private readonly DynamicBuffer<Stat> m_Items;
        [ReadOnly]
        private readonly DynamicBuffer<Modifier> m_Modifiers;

        public Entity Self => m_Self;
        public Entity Root => m_Root.ValueRO.Value;
        public DynamicBuffer<Stat> Values => m_Items;

        public void Estimation(float delta)
        {
            for (int i = 0; i < m_Items.Length; i++)
            {
                Modifier.Estimation(Self, ref m_Items.ElementAt(i), m_Modifiers, delta);
            }
        }

        public ref Stat GetRW(Enum stat)
        {
            return ref m_Items.GetRW(Stat.GetID(stat));
        }

        public ref Stat GetRW(int statId)
        {
            return ref m_Items.GetRW(statId);
        }

        public Stat GetRO(Enum stat)
        {
            return m_Items.GetRO(Stat.GetID(stat));
        }

        public Stat GetRO(int statId)
        {
            return m_Items.GetRO(statId);
        }

        #region DesignTime

#if UNITY_EDITOR
        [CreateProperty]
        public readonly List<Stat> StatsNames => m_Items.AsNativeArray().ToArray().ToList();//Select(i => i.StatName)

#endif
        #endregion
    }
}