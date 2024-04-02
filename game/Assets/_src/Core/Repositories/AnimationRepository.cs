using System;
using System.Collections.Generic;
using System.Linq;
using Common.Core;
using Common.Defs;

namespace Game.Core.Repositories
{
    public class AnimationRepository: DefsRepository<IIdentifiable<ObjectID>>
    {
        public void Insert(ObjectID id, IConfig config, Action<IIdentifiable<ObjectID>> callback = null)
        {
            m_Repo.Insert(id, new Attribute(config, null), callback);
        }

        public void Insert(IEnumerable<IIdentifiable<ObjectID>> configs, Action<IIdentifiable<ObjectID>> callback = null)
        {
            m_Repo.Insert(configs.Select(config => new Attribute(config, null)), callback);
        }
    }
}
