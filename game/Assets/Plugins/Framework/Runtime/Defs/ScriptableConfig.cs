using System;
using UnityEngine;
using Unity.Entities;
using Hash128 = Unity.Entities.Hash128;

namespace Common.Defs
{
    using Core;

    public class ScriptableConfig : ScriptableObject, IConfig, IIdentifiable<ObjectID>
    {
        public GameObject Prefab;

        [SerializeField, HideInInspector] 
        private Hash128 m_PrefabID;
        [SerializeField] private ObjectID m_ID;

        public Hash128 PrefabID => m_PrefabID;
        public ObjectID ID => m_ID;

        private Entity m_Entity;

        public void SetPrefab(Entity entity)
        {
            m_Entity = entity;
        }

        //public abstract Entity Instantiate();

#if UNITY_EDITOR 
        private void OnValidate()
        {
            m_ID = ObjectID.Create(name);
            if (Prefab)
            {
                UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(Prefab, out string guid, out long localId);
                m_PrefabID = new Hash128(guid);
            }
        }
#endif
    }
}
