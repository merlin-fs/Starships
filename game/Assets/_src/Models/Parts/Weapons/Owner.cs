using System;
using Unity.Entities;

namespace Game.Model
{
    public struct Root: IComponentData
    {
        public Entity Value;
    }
}