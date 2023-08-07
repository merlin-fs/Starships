using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Core;
using Common.Defs;
using UnityEngine.AddressableAssets;

namespace Game.Core.Repositories
{
    public partial struct RepositoryLoadSystem
    {
        readonly static DIContext.Var<ObjectRepository> m_ObjectRepository;
        readonly static DIContext.Var<AnimationRepository> m_AnimationRepository;

        public static Task<IList<IIdentifiable<ObjectID>>> LoadObjects()
        {
            var repository = m_ObjectRepository.Value; 
            return Addressables.LoadAssetsAsync<IIdentifiable<ObjectID>>("defs", null)
                .Task
                .ContinueWith(task =>
                {
                    repository.Insert(task.Result.Cast<IConfig>(), "defs");
                    return task.Result;
                });
        }

        public static Task<IList<IIdentifiable<ObjectID>>> LoadAnimations()
        {
            var repository = m_AnimationRepository.Value; 
            return Addressables.LoadAssetsAsync<IIdentifiable<ObjectID>>("animation", null)
                .Task
                .ContinueWith(task =>
                {
                    repository.Insert(task.Result, (iter) =>
                    {
                        if (iter is IInitiated initiated)
                            initiated.Init();
                    });
                    return task.Result;
                });
        }
    }
}