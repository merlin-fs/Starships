using System;
using Unity.Entities;
using Unity.Properties;

namespace Game.Model.Units
{
    public readonly partial struct UnitAspect: IAspect
    {
        private readonly Entity m_Self;
        public Entity Self => m_Self;

        readonly RefRW<Unit> m_Unit;

        readonly RefRO<Team> m_Team;

        public Team Team => m_Team.ValueRO;

        #region DesignTime
#if UNITY_EDITOR

        [CreateProperty]
        public readonly string TeamName => GlobalTeamsConfig.Instance.GetName(new TeamValue { Value = m_Team.ValueRO.SelfTeam });
        [CreateProperty]
        public readonly string[] EnemyName => GlobalTeamsConfig.Instance.GetNames(new TeamValue { Value = m_Team.ValueRO.EnemyTeams });
#endif
        #endregion
    }
}
