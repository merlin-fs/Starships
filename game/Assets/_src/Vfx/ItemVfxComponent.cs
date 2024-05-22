#if UNITY_EDITOR        
using UnityEditor;
#endif

using UnityEngine;

using Hash128 = Unity.Entities.Hash128;

namespace Game.Views
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ItemVfxComponent : MonoBehaviour, IItemVfx
#if UNITY_EDITOR        
        , ISerializationCallbackReceiver
#endif    
    {
        [SerializeField] private Hash128 _id; 
        [field: SerializeField] public ParticleSystem ParticleSystem { get; private set; }

        public Hash128 ID => _id;

#if UNITY_EDITOR        
        public void OnBeforeSerialize()
        {
            if (!this) return;

            ParticleSystem = GetComponent<ParticleSystem>();
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            if (string.IsNullOrEmpty(assetPath)) return;
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var newId = new Hash128(guid);
            if (_id == newId) return;
            _id = newId;
            EditorUtility.SetDirty(gameObject);
        }

        public void OnAfterDeserialize()
        {
        }
#endif
    }
}
