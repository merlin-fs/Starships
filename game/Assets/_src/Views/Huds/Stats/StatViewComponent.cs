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

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Views.Stats
{
    public struct StatView : IComponentData, IStatViewComponent
    {
        private static Hud.Manager HudManager => Inject<Hud.Manager>.Value;
        private RefLink<HudHealth> m_Hud;
        private HudHealth Hud => m_Hud.Value;
        private readonly EnumHandle m_StatID;

        public StatView(EnumHandle statID)
        {
            m_StatID = statID;
            m_Hud = RefLink<HudHealth>.From(HudManager.GetHud<HudHealth>());
        }
        public void Update(in StatAspect stat, in LocalTransform transform)
        {
            Hud.Update(transform.Position, stat.GetRO(m_StatID).Normalize);
        }
        
        public void Dispose()
        {
            HudManager.ReleaseHud(Hud);
            RefLink<HudHealth>.Free(m_Hud);
        }
    }
}