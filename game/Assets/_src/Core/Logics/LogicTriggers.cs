using System;
using System.Collections.Generic;

using Game.Core;

using UnityEngine;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial class LogicDef
        {
            private Dictionary<GoalHandle, ITrigger> m_TriggersState = new Dictionary<GoalHandle, ITrigger>();
            private Dictionary<EnumHandle, ITrigger> m_TriggersAction = new Dictionary<EnumHandle, ITrigger>();

            public void AddTriggerState<T>(GoalHandle state)
                where T : ITrigger
            {
                m_TriggersState.Add(state, Activator.CreateInstance<T>());
            }
            
            public void ExecuteTriggersState(ref LogicContext context, GoalHandle state)
            {
                if (m_TriggersState.TryGetValue(state, out ITrigger trigger))
                    trigger.Execute(ref context);
            }

            public void AddTriggerAction<T>(EnumHandle action)
                where T : ITrigger
            {
                m_TriggersAction.Add(action, Activator.CreateInstance<T>());
            }
            
            public void ExecuteTriggersAction(ref LogicContext context, EnumHandle action)
            {
                if (m_TriggersAction.TryGetValue(action, out ITrigger trigger))
                    trigger.Execute(ref context);
            }
        }
    }
}