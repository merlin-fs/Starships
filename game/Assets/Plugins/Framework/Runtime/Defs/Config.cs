using System;
using Unity.Entities;

namespace Common.Defs
{
    using Core;

    public class Config : IConfig, IIdentifiable<ObjectID>
    {
        private ObjectID m_ID;
        private Entity m_Prefab;

        public ObjectID ID => m_ID;
        public Entity Prefab => m_Prefab;

        public Config(ObjectID id, Entity prefab)
        {
            m_ID = id;
            m_Prefab = prefab;
        }

        void IConfig.Configure(Entity root, IDefinableContext context)
        {
            m_Prefab = root;
        }
    }
}
