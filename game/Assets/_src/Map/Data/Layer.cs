using System;
using Unity.Entities;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public struct Layers
        {
            public struct Layer : IBufferElementData
            {
                public Entity Entity;
                public static implicit operator Layer(Entity value) => new Layer { Entity = value };
            }

            public struct Floor : IBufferElementData
            {
                public Entity Entity;
                public static implicit operator Floor(Entity value) => new Floor { Entity = value };
            }
        }
    }
}
