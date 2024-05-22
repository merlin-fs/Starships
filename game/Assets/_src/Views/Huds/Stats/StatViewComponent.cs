using System;
using System.Threading;
using Unity.Transforms;
using Unity.Entities;

using Common.Core;
using Common.Defs;
using Game.Model.Stats;
using Game.UI.Huds;
using Game.Core;

using JetBrains.Annotations;

using Reflex.Attributes;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Views.Stats
{
    public struct StatView : IComponentData, IStatView
    {
        [Inject] private static Hud.Manager m_HudManager;
        private RefLink<HudHealth> m_Hud;
        private HudHealth Hud => m_Hud.Value;
        private readonly EnumHandle m_StatID;

        public StatView(EnumHandle statID)
        {
            m_StatID = statID;
            //m_Hud = RefLink<HudHealth>.From(m_HudManager.GetHud<HudHealth>());
            m_Hud = default;
        }
        
        public void Update(in StatAspect stat, in LocalTransform transform)
        {
            //Hud.Update(transform.Position, stat.GetRO(m_StatID).Normalize);
        }
        
        public void Dispose()
        {
            //!!!m_HudManager.ReleaseHud(Hud);
            //!!!m_Hud.Free();
        }

        public Transform Transform => m_HudManager.WorldCamera.transform;
    }
}