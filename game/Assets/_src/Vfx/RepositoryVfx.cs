using Game.Core.Repositories;
using Game.Views;

using UnityEngine;
using UnityEngine.AddressableAssets;
using Hash128 = Unity.Entities.Hash128;

namespace Game.Core
{
    public class RepositoryVfx : Repository<IItemVfx, Hash128>
    {
        public bool TryGet(Hash128 id, out IItemVfx value)
        {
            value = FindByID(id);
            return value != null;
        }

        public async void CachedVfx(Hash128 id)
        {
            var item = FindByID(id);
            if (item != null) return;
            
            var prefab = await new AssetReferenceT<GameObject>(id.ToString()).LoadAssetAsync().Task;
            Insert(prefab.GetComponent<IItemVfx>());
        }
    }
}
