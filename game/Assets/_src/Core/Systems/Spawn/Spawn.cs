using System;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;
using Newtonsoft.Json.Linq;

namespace Game.Core.Spawns
{
    public partial struct Spawn : IComponentData
    {
        public Entity Prefab;
        public int2 Position;
        
        public struct Tag : IComponentData{}

        public struct Load : IComponentData
        {
            public RefLink<JToken> Data;
            public int ID;
        }

        public struct Component : IBufferElementData
        {
            public ComponentType ComponentType;

            public static implicit operator Component(ComponentType componentType) =>
                new Component {ComponentType = componentType};
        }
        
        public struct EventTag : IComponentData { }
   }
}