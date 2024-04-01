using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;
using Game.UI;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public partial struct Data : IComponentData, IDefinable, IDefinableCallback
        {
            private readonly RefLink<Def> m_RefLink;
            public Def Define => m_RefLink.Value;

            public int2 Size;
            public ViewDataStruct ViewData;

            public Data(RefLink<Def> refLink)
            {
                m_RefLink = refLink;
                Size = default;
                ViewData = default;
                ViewData.WorldToLocalMatrix = float4x4.identity / 1.5f;
                ViewData.LocalToWorldMatrix = float4x4.identity * 1.5f;
            }

            #region IDefineableCallback
            public void AddComponentData(Entity entity, IDefinableContext context)
            {
            }

            public void RemoveComponentData(Entity entity, IDefinableContext context) { }
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
