using System;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Weapons
{
    [Serializable]
    public partial struct TurretRotateHorizontal : IDefinable, IComponentData
    {
        private readonly RefLink<TurretRotateHorizontalDef> m_RefLink;
        public TurretRotateHorizontalDef Def => m_RefLink.Value;

        public TurretRotateHorizontal(RefLink<TurretRotateHorizontalDef> config)
        {
            m_RefLink = config;
        }

        [Serializable]
        public class TurretRotateHorizontalDef : IDef<TurretRotateHorizontal>
        {
        }
    }
}
