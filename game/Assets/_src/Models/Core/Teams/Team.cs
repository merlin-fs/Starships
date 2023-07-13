using System;
using Unity.Entities;
using Common.Defs;

using Game.Core.Saves;

using UnityEngine;
using Unity.Properties;
using Game.Model.Logics;

namespace Game.Model
{
    [Serializable]
    public struct Team : IComponentData, IDefinable, IDefineableCallback
    {
        private readonly RefLink<Def> m_RefLink;

        [CreateProperty]
        public uint SelfTeam => m_RefLink.Value.SelfTeam;
        [CreateProperty]
        public uint EnemyTeams => m_RefLink.Value.EnemyTeams;

        public Team(RefLink<Def> refLink)
        {
            m_RefLink = refLink;
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