using System;
using System.Collections.Generic;

using Unity.Entities;

namespace Game.Model.Logics
{
    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public abstract partial class LogicConcreteSystem : SystemBase, Logic.IConfigurator
    {
        protected EntityQuery m_Query;
        
        private static readonly Dictionary<Logic.Config, Type> m_Actions = new Dictionary<Logic.Config, Type>();

        public static void AddInit(Logic.Config config, Type type)
        {
            if (!m_Actions.ContainsKey(config))
                m_Actions.Add(config, type);
        }

        protected override void OnCreate()
        {
            var self = GetType();
            foreach (var iter in m_Actions)
            {
                if (iter.Value == self)
                    Init(iter.Key);
            }
        }

        void Logic.IConfigurator.Init(Logic.Config config) => Init(config);

        protected abstract void Init(Logic.Config config);

    }
}
