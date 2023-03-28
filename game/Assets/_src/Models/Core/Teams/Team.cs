using System;
using Unity.Entities;
using Common.Defs;
using UnityEngine;
using Unity.Properties;
using Game.Model.Logics;

namespace Game.Model
{
    [Serializable]
    public struct Team : IComponentData, IDefinable, IDefineableCallback
    {
        private readonly Def<Def> m_Def;

        [CreateProperty]
        public uint SelfTeam => m_Def.Value.SelfTeam;
        [CreateProperty]
        public uint EnemyTeams => m_Def.Value.EnemyTeams;

        public Team(Def<Def> def)
        {
            m_Def = def;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
            context.AddComponentData(entity, new Target());
        }

        public void RemoveComponentData(Entity entity, IDefineableContext context) { }
        #endregion


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

            void ISerializationCallbackReceiver.OnBeforeSerialize() { }

            void ISerializationCallbackReceiver.OnAfterDeserialize() 
            {
                m_EnemyTeamsValue = GetTeams(m_EnemyTeams);
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