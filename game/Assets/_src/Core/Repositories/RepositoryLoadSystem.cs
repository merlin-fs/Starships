using System;
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

        public static Task LoadObjects()
        {
            var repository = m_ObjectRepository.Value; 
            return Addressables.LoadAssetsAsync<IConfig>("defs", null)
                .Task
                .ContinueWith(task =>
                {
                    repository.Insert(task.Result);
                });
        }

        public static Task LoadAnimations()
        {
            var repository = m_AnimationRepository.Value; 
            return Addressables.LoadAssetsAsync<IIdentifiable<ObjectID>>("animation", null)
                .Task
                .ContinueWith(task =>
                {
                    repository.Insert(task.Result);
                    Parallel.ForEach(task.Result, iter =>
                    {
                        if (iter is IInitiated initiated)
                            initiated.Init();
                    });
                    
                });
        }
    }
}