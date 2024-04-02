using System;

using Unity.Entities;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public partial struct Layers
        {
            public struct Floor : ILayer
            {
                private Entity m_Entity;
                public Entity Entity { get => m_Entity; set => m_Entity = value; }
            }
        }
    }
}