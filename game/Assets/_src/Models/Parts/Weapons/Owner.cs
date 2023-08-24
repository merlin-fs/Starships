using System;
using Unity.Entities;

namespace Game.Model
{
    public struct Root: IComponentData
    {
        public Entity Value;
    }

    public struct ChildEntity: IBufferElementData
    {
        public Entity Value;
    }
}