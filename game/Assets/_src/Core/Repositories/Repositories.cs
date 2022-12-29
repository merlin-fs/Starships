using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Common.Core;
using Common.Defs;
using Common.Singletons;
using Common.Repositories;
using UnityEngine.AddressableAssets;

namespace Game.Core.Repositories
{
    public class Repositories : Singleton<Repositories>, ISingleton
    {
        private const string LABEL_DEF = "defs";

        public static Repositories Instance => Inst;

        private ConcurrentDictionary<Type, object> m_Items = new ConcurrentDictionary<Type, object>();

        public Task<IReadonlyRepository<ObjectID, IConfig>> ConfigsAsync()
        {
            return RepositoryAsync<IConfig>();
        }

        private async Task<IReadonlyRepository<ObjectID, TDef>> RepositoryAsync<TDef>()
            where TDef : IIdentifiable<ObjectID>
        {
            var type = typeof(TDef);

            if (m_Items.TryGetValue(type, out object value))
                return (DefsRepository<TDef>)value;
            else
            {
                var result = await NewRepositoryAsync<TDef>();
                m_Items.TryAdd(type, result);
                return result;
            }
        }

        private Task<DefsRepository<TDef>> NewRepositoryAsync<TDef>()
            where TDef : IIdentifiable<ObjectID>
        {
            return Addressables.LoadAssetsAsync<TDef>(LABEL_DEF, null).Task
                .ContinueWith(t =>
                {
                    var repository = new DefsRepository<TDef>();
                    repository.Repo.Clear();
                    try
                    {
                        repository.Repo.Insert(t.Result);
                        return repository;
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                        return null;
                    }
                });
        }
    }
}
