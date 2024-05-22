#if UNITY_EDITOR        
using UnityEditor;
#endif    

using UnityEngine;
using UnityEngine.AddressableAssets;

using Hash128 = Unity.Entities.Hash128;

namespace Game.Views
{
    public class ItemVfxComponent : MonoBehaviour, IItemVfx
#if UNITY_EDITOR        
        , ISerializationCallbackReceiver
#endif    
    {
        [SerializeField, HideInInspector]
        private Hash128 _id; 
        public Hash128 ID => _id;
        
#if UNITY_EDITOR        
        public void OnBeforeSerialize()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            _id = new Hash128(guid);
        }

        public void OnAfterDeserialize()
        {
        }
#endif
    }
}
