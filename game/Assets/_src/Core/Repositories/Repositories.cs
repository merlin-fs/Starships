using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using Common.Core;
using Common.Defs;

using Unity.Entities;

namespace Game.Core.Repositories
{
    public struct RepositoryLoadTag: IComponentData { }
    
    public class Repository: DefsRepository<IConfig>
    {
        private readonly ConcurrentDictionary<string, byte> m_Labels = new ConcurrentDictionary<string, byte>();
        public IEnumerable<string> Labels => m_Labels.Keys;
        public void Insert(ObjectID id, IConfig config, params string[] labels)
        {
            foreach (var iter in labels)
                m_Labels[iter] = 0;
            m_Repo.Insert(id, new Attribute(config, labels));
        }

        public void Insert(IEnumerable<IConfig> configs, params string[] labels)
        {
            foreach (var iter in labels)
                m_Labels[iter] = 0;
            m_Repo.Insert(configs.Select(config => new Attribute(config, labels)));
        }
    }
}
