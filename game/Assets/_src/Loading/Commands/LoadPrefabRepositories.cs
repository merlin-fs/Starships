using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Defs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Core.Loading
{
    public class LoadPrefabRepositories : LoadRepositories<GameObject>
    {
        protected override AsyncOperationHandle<IList<GameObject>> GetAsyncOperationHandle(IEnumerable keys)
        {
            return Addressables.LoadAssetsAsync<GameObject>(keys, null);
        }

        protected override IEnumerable<IConfig> CastToConfig(IList<GameObject> result)
        {
            return result.Select(obj => obj.GetComponent<IConfig>());
        }
    }
}