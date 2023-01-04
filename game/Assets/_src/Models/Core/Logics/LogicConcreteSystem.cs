using System;
using System.Collections.Generic;

using Unity.Entities;

namespace Game.Model.Logics
{
    [UpdateInGroup(typeof(GameLogicSystemGroup), OrderFirst = true)]
    public abstract partial class LogicConcreteSystem : SystemBase, Logic.IConfigurator
    {
        protected EntityQuery m_Query;
        
        private static readonly Dictionary<Logic.Config, Type> m_Actions = new Dictionary<Logic.Config, Type>();

        protected int LogicID { get; private set; }

        public static void AddInit(Logic.Config config, Type type)
        {
            if (!m_Actions.ContainsKey(config))
            {
                m_Actions.Add(config, type);
                var system = World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged(type) as LogicConcreteSystem;
                system?.Init(config);
            }
        }

        protected override void OnCreate()
        {
            var type = GetType();
            LogicID = type.GetHashCode();
            foreach (var iter in m_Actions)
            {
                if (iter.Value == type)
                    Init(iter.Key);
            }
        }

        void Logic.IConfigurator.Init(Logic.Config config) => Init(config);

        protected abstract void Init(Logic.Config config);

    }
}
