using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Hash128 = Unity.Entities.Hash128;

namespace Common.Defs
{
    using Core;

    [Serializable]
    public class ChildConfig
    {
        public ScriptableConfig Child;
#if UNITY_EDITOR
        [SelectChildPrefab]
        public GameObject PrefabObject;
#endif
    }

    public interface IConfigContainer
    {
        IEnumerable<ChildConfig> Childs { get; }
    }

    public abstract class ScriptableConfig : ScriptableObject, IConfig, IIdentifiable<ObjectID>, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        public GameObject PrefabObject;
        public GameObject GetPrefab() => PrefabObject;
#endif

        //[SerializeField, HideInInspector]
        //private Hash128 m_PrefabID;

        private ObjectID m_ID;
        private Entity m_Prefab;

        //public Hash128 PrefabID => m_PrefabID;
        public ObjectID ID => m_ID;
        public Entity Prefab => m_Prefab;

        void IConfig.Configurate(Entity root, IDefineableContext context)
        {
            m_Prefab = root;
            Configurate(root, context);
        }

        protected abstract void Configurate(Entity entity, IDefineableContext context);

        public virtual void OnBeforeSerialize()
        {
            m_ID = ObjectID.Create(name);
            CreateID();
        }

        public virtual void OnAfterDeserialize()
        {
        }

        private void OnEnable()
        {
            m_ID = ObjectID.Create(name);
        }

        private void OnValidate()
        {
            m_ID = ObjectID.Create(name);
            CreateID();
        }

        private void CreateID()
        {
            /*
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;
            if (PrefabObject)
            {
                UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(PrefabObject, out string guid, out long localId);
                m_PrefabID = new Hash128(guid);
            }
#endif
            */
        }
    }
}
