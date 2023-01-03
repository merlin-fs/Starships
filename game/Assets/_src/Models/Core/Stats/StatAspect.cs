using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Properties;

namespace Game.Model.Stats
{
    public readonly partial struct StatAspect : IAspect
    {
        private readonly Entity m_Self;
        public Entity Self => m_Self;

        private readonly DynamicBuffer<Stat> m_Items;

        private readonly ModifiersAspect m_Modifiers;

        public DynamicBuffer<Stat> Values => m_Items;

        public void Estimation(float delta)
        {
            UnityEngine.Debug.Log($"[{Self}] Estimation");
            for (int i = 0; i < m_Items.Length; i++)
            {
                m_Modifiers.Estimation(ref m_Items.ElementAt(i), delta);
            }
        }

#if DEBUG
        [CreateProperty]
        public readonly List<Stat> StatsNames => m_Items.AsNativeArray().ToArray().ToList();//Select(i => i.StatName)
#endif
    }
}