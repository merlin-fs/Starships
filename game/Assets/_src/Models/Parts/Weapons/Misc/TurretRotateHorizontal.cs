using System;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Weapons
{
    [Serializable]
    public partial struct TurretRotateHorizontal : IDefinable, IComponentData
    {
        private readonly Def<TurretRotateHorizontalDef> m_Def;
        public TurretRotateHorizontalDef Def => m_Def.Value;

        public TurretRotateHorizontal(Def<TurretRotateHorizontalDef> config)
        {
            m_Def = config;
        }

        [Serializable]
        public class TurretRotateHorizontalDef : IDef<TurretRotateHorizontal>
        {
        }
    }
}
