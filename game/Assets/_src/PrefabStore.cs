using System;
using System.Collections.Generic;
using Unity.Entities;
using Common.Singletons;

namespace Game.Systems
{
    public class PrefabStore : Singleton<PrefabStore>, ISingleton
    {
        public static PrefabStore Instance => Inst;

        private Dictionary<string, Entity> m_Prefabs = new Dictionary<string, Entity>();
        public void Add(string name, Entity prefab)
        {
            if (!m_Prefabs.ContainsKey(name))
                m_Prefabs.Add(name, prefab);
        }

        public bool TryGet(string name, out Entity prefab)
        {
            return m_Prefabs.TryGetValue(name, out prefab);
        }
    }
}

