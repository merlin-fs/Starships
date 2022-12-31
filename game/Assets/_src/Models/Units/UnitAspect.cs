using System;
using Unity.Entities;
using Unity.Properties;

namespace Game.Model.Units
{
    public readonly partial struct UnitAspect: IAspect
    {
        private readonly Entity m_Self;

        readonly RefRW<Unit> m_Unit;

        readonly RefRO<Team> m_Team;

        #region DesignTime

#if UNITY_EDITOR

        [CreateProperty]
        public readonly string Team => GlobalTeamsConfig.Instance.GetName(new TeamValue { Value = m_Team.ValueRO.SelfTeam });
        [CreateProperty]
        public readonly string[] Enemy => GlobalTeamsConfig.Instance.GetNames(new TeamValue { Value = m_Team.ValueRO.EnemyTeams });
#endif
        #endregion
    }
}
