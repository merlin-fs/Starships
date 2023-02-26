using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public partial struct Data : IComponentData, IDefinable, IDefineableCallback
        {
            private readonly Def<Def> m_Def;
            public Def Define => m_Def.Value;

            public int2 Size;
            public ViewDataStruct ViewData;

            public Data(Def<Def> def)
            {
                m_Def = def;
                Size = default;
                ViewData = default;
                ViewData.WorldToLocalMatrix = float4x4.identity;
                ViewData.LocalToWorldMatrix = float4x4.identity * 1.5f;
            }

            #region IDefineableCallback
            public void AddComponentData(Entity entity, IDefineableContext context)
            {
                context.AddBuffer<Layers.Floor>(entity);
            }

            public void RemoveComponentData(Entity entity, IDefineableContext context) { }
            #endregion

            [Serializable]
            public class Def : IDef<Data>
            {
                public int2 Size;
            }

            public struct ViewDataStruct
            {
                public float4x4 WorldToLocalMatrix;
                public float4x4 LocalToWorldMatrix;
                public Bounds Bounds;
            }
        }
    }
}
