using System;
using System.Reflection;
using Common.Core;
using Unity.Entities;
using Unity.Mathematics;

using static Game.Model.Logics.Logic;
using static Game.Model.Worlds.Map;
using static UnityEngine.EventSystems.EventTrigger;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public readonly partial struct Aspect: IAspect 
        {
            private readonly Entity m_Self;

            readonly RefRW<Data> m_Data;
            public Entity Self => m_Self;
            public Data Value => m_Data.ValueRO;

            public void Init(ref SystemState systemState, Aspect aspect)
            {
                m_Data.ValueRW.Size = m_Data.ValueRO.Define.Size;
                Layers.Init(ref systemState, aspect);
            }

            public void SetObject(TypeIndex layerType, int2 pos, Entity entity)
            {
                Layers.SetObject(this, layerType, pos, entity);
            }

            public bool TryGetObject(TypeIndex layerType, int2 pos, out Entity entity)
            {
                return Layers.TryGetObject(this, layerType, pos, out entity);
            }
            
            public Entity GetObject(TypeIndex layerType, int2 pos)
            {
                return Layers.GetObject(this, layerType, pos);
            }

        }
    }
}
