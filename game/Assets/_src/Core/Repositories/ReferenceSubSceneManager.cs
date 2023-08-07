using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Entities.Serialization;
using UnityEngine.AddressableAssets;
using Common.Core;

using Unity.Entities;
using Unity.Scenes;

namespace Game.Core.Repositories
{
    public class ReferenceSubSceneManager
    {
        private Dictionary<ObjectID, EntitySceneReference> m_References = new Dictionary<ObjectID, EntitySceneReference>();

        public Task LoadAsync()
        {
            return Addressables.LoadAssetsAsync<ReferenceSubScene>("System", null).Task
                .ContinueWith(task =>
                {
                    foreach (var iter in task.Result)
                    {
                        iter.Values.ToList().ForEach(x => m_References.Add(x.Key, x.Value));
                    }
                });
        }

        public void LoadSubScenes(WorldUnmanaged world, IEnumerable<ObjectID> ids)
        {
            foreach (var id in ids.GroupBy(iter => iter))
            {
                if (m_References.TryGetValue(id.Key, out var value))
                    //SceneSystem.IsSceneLoaded (world, value)
                    SceneSystem.LoadSceneAsync(world, value);
            }
        }
    }
}