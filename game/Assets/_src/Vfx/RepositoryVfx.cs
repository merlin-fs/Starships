using System.Collections.Concurrent;

using Game.Core.Repositories;
using Game.Views;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Core
{
    public class RepositoryVfx : Repository<IItemVfx, Unity.Entities.Hash128>
    {
        public IItemVfx GetVfx(Unity.Entities.Hash128 id) => FindByID(id);

        public async void CachedVfx(Unity.Entities.Hash128 id)
        {
            var item = FindByID(id);
            if (item != null) return;
            var reference = new AssetReferenceT<GameObject>(id.ToString());
            var prefab = await reference.LoadAssetAsync().Task;
                /*
            var prefab = !reference.IsValid()
                ? await reference.LoadAssetAsync().Task
                : (GameObject)reference.Asset;
                */
            Insert(prefab.GetComponent<IItemVfx>());
        }
    }
}
