using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public readonly partial struct Aspect: IAspect 
        {
            private readonly Entity m_Self;
            readonly RefRW<Data> m_Data;
            readonly DynamicBuffer<Layers.Floor> m_Floor;
            public Entity Self => m_Self;
            public Data Value => m_Data.ValueRO;

            public DynamicBuffer<Layers.Floor> Floor => m_Floor;

            public void SetObject(int2 pos, Entity entity)
            {
                m_Floor.ElementAt(Value.At(pos)).Entity = entity;
            }

            public Entity GetObject(int2 pos)
            {
                return m_Floor[Value.At(pos)].Entity;
            }

            public Entity GetObject(int x, int y)
            {
                return m_Floor[Value.At(new int2(x, y))].Entity;
            }

            public void Init()
            {
                m_Data.ValueRW.Size = m_Data.ValueRO.Define.Size;
                m_Floor.Resize(m_Data.ValueRO.Length, Unity.Collections.NativeArrayOptions.ClearMemory);
            }
        }
    }
}
