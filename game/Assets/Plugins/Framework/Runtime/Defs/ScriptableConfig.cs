using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Common.Defs
{
    [Serializable]
    public class ChildConfig
    {
        public ScriptableConfig Child;
#if UNITY_EDITOR
        [SelectChildPrefab]
        public GameObject PrefabObject;
        public bool Enabled = true;
#endif
    }

    public interface IConfigContainer
    {
        IEnumerable<ChildConfig> Childs { get; }
    }

    public abstract class ScriptableConfig : ScriptableIdentifiable, IConfig
    {
#if UNITY_EDITOR
        public GameObject PrefabObject;
        public GameObject GetPrefab() => PrefabObject;
#endif
        private Entity m_Prefab;

        public Entity Prefab => m_Prefab;

        void IConfig.Configurate(Entity root, IDefineableContext context)
        {
            m_Prefab = root;
            Configurate(root, context);
        }

        protected abstract void Configurate(Entity entity, IDefineableContext context);
    }
}
