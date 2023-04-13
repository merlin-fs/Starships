using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Common.Core;
using Common.Defs;

namespace Game.Core.Repositories
{
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
    }
}
