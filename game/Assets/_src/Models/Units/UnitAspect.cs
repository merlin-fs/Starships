using System;
using Unity.Entities;
using Unity.Properties;

namespace Game.Model.Units
{
    public readonly partial struct UnitAspect: IAspect
    {
        public readonly Entity Self;

        readonly RefRW<Unit> m_Unit;

        readonly RefRO<Team> m_Team;
        
        [CreateProperty]
        public string Team => GlobalTeamsConfig.Instance.GetName(new TeamValue { Value = m_Team.ValueRO.SelfTeam });
        [CreateProperty]
        public string[] Enemy => GlobalTeamsConfig.Instance.GetNames(new TeamValue { Value = m_Team.ValueRO.EnemyTeams });
    }
}
