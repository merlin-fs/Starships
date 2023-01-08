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

        private Task m_Task;
        private object m_Look = new object();

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
                return result;
            }
        }

        private Task<DefsRepository<TDef>> NewRepositoryAsync<TDef>()
            where TDef : IIdentifiable<ObjectID>
        {
            lock (m_Look)
            {
                m_Task ??= Addressables.LoadAssetsAsync<TDef>(LABEL_DEF, null).Task
                    .ContinueWith(t =>
                    {
                        var repository = new DefsRepository<TDef>();
                        repository.Repo.Clear();
                        try
                        {
                            repository.Repo.Insert(t.Result);
                            m_Items.TryAdd(typeof(TDef), repository);
                            m_Task = null;
                            return repository;
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogException(ex);
                            return null;
                        }

                    });
            }
            return (Task<DefsRepository<TDef>>)m_Task;
        }
    }
}
