using System;
using Unity.Entities;
using Common.Defs;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public struct Team : IComponentData, IDefineable
    {
        private readonly Def<Def> m_Def;

        public uint SelfTeam => m_Def.Value.SelfTeam;
        public uint EnemyTeams => m_Def.Value.EnemyTeams;

        public Team(Def<Def> def)
        {
            m_Def = def;
        }

        [Serializable]
        public struct Def: IDef<Team>, ISerializationCallbackReceiver
        {
            [SerializeField]
            private TeamValue m_SelfTeam;
            [SerializeField]
            private TeamValue[] m_EnemyTeams;

            private uint m_EnemyTeamsValue;

            public uint SelfTeam => m_SelfTeam;
            public uint EnemyTeams => m_EnemyTeamsValue;

            void ISerializationCallbackReceiver.OnBeforeSerialize()
            {

            }

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {

            }

            private TeamValue GetTeams(TeamValue[] values)
            {
                uint teams = 0;
                foreach (var iter in values)
                    teams |= iter;
                return teams;
            }
        }
    }
}