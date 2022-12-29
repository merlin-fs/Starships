using System;
using Common.Defs;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Game.Core.Prefabs
{
    public class PrefabTarget : MonoBehaviour
    {
        [SerializeField, SelectType(typeof(IDefineable))]
        string m_Type;

        class _baker : Baker<PrefabTarget>
        {
            public unsafe override void Bake(PrefabTarget authoring)
            {
#if UNITY_EDITOR
                var obj = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(authoring.gameObject);
                var root = !obj;
                if (root)
                    obj = authoring.gameObject;

                UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localId);

                var type = TypeManager.GetTypeIndex(Type.GetType(authoring.m_Type));
                AddComponent(new PrefabTargetData 
                {
                    IsChild = !root,
                    PrefabID = new Hash128(guid),
                    Type = type,
                    Target = GetEntity(),
                });
#endif
            }
        }
    }
}
