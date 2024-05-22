using System;

using Common.Core;

using Game.Core.Storages;

using Unity.Entities;

namespace Game.Core.Prefabs
{
    [Serializable, Storage]
    public partial struct PrefabInfo: IComponentData
    {
        private ObjectID m_ConfigID;
        public ObjectID ConfigID { get => m_ConfigID; set => m_ConfigID = value; }
    }
}
