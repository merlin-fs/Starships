using System;
using System.Threading.Tasks;
using Common.Core;
using Common.Defs;
using UnityEngine.AddressableAssets;

namespace Game.Core.Repositories
{
    public partial struct RepositoryLoadSystem
    {
        readonly static DIContext.Var<Repository> m_Repository;

        public static Task Load()
        {
            var repository = m_Repository.Value; 
            return Addressables.LoadAssetsAsync<IConfig>("defs", null)
                .Task
                .ContinueWith(task =>
                {
                    repository.Insert(task.Result);
                });
        }
    }
}