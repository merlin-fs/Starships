using System;

using Common.Core;

using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;
using Newtonsoft.Json.Linq;

namespace Game.Core.Spawns
{
    public partial struct Spawn : IComponentData
    {
        public int ID;
        public RefLink<JToken> Data;
        public ObjectID PrefabID;
        
        public struct Tag : IComponentData{}
        public struct ViewTag : IComponentData{}

        public struct Component : IBufferElementData
        {
            public ComponentType ComponentType;
            public static implicit operator Component(ComponentType componentType) =>
                new Component {ComponentType = componentType};
        }
   }
}