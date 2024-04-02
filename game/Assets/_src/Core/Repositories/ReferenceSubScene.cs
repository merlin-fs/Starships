using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.Serialization;
using UnityEngine;
using Common.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Core.Repositories
{
    public class ReferenceSubScene : ScriptableObject
    {
        [SerializeField]
        private List<SubScene> m_Items = new List<SubScene>();

        public IDictionary<ObjectID, EntitySceneReference> Values => m_Items.ToDictionary(iter => iter.ID, iter => iter.Reference); 

#if UNITY_EDITOR
        public void Add(ObjectID id, EntitySceneReference sceneReference)
        {
            if (m_Items.ToDictionary(iter => iter.ID).ContainsKey(id)) return;
            
            m_Items.Add(new SubScene 
            {
                ID = id,
                Reference = sceneReference,
            });
            EditorUtility.SetDirty(this);
        }
#endif
        
        [Serializable]
        private struct SubScene
        {
            public ObjectID ID;
            public EntitySceneReference Reference;
        }
    }
}