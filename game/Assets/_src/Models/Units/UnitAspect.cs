using System;
using Game.Model.Stats;
using Unity.Collections;
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

        [ReadOnly] readonly DynamicBuffer<Stat> m_Stats;

        public Team Team => m_Team.ValueRO;
        public Stat Stat<T>(T stat) where T: struct, IConvertible => m_Stats.GetRO(stat);

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
