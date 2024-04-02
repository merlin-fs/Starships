using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public partial struct Layers
        {
            public interface ILayer : IBufferElementData
            {
                Entity Entity { get; set; }
            }
            
            public interface ILayerValidator
            {
                bool CanPlace(Aspect aspect, int2 pos, Entity entity);
            }
        }
    }
}
