using System;
using Unity.Entities;

namespace Game.Model
{
    public struct Part: IComponentData
    {
        public Entity Unit;
    }
}