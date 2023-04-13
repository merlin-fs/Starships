using System;
using Unity.Entities;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public partial struct Layers
        {
            public interface ILayer : IBufferElementData
            {
                public Entity Entity { get; set; }
            }
        }
    }
}
