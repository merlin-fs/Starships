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

        public Config(ObjectID iD, Entity prefab)
        {
            m_ID = iD;
            m_Prefab = prefab;
        }

        void IConfig.Configurate(Entity root, IDefineableContext context)
        {
            m_Prefab = root;
        }
    }
}
