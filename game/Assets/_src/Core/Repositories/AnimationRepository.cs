using System;
using System.Collections.Generic;
using System.Linq;
using Common.Core;
using Common.Defs;

namespace Game.Core.Repositories
{
    public class AnimationRepository: DefsRepository<IIdentifiable<ObjectID>>
    {
        public void Insert(ObjectID id, IConfig config)
        {
            m_Repo.Insert(id, new Attribute(config, null));
        }

        public void Insert(IEnumerable<IIdentifiable<ObjectID>> configs)
        {
            m_Repo.Insert(configs.Select(config => new Attribute(config, null)));
        }
    }
}
