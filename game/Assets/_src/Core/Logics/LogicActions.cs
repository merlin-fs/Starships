using System;
using System.Collections.Generic;
using System.Linq;

using Game.Core;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial class LogicDef
        {
            public void ActionExecute<TContext>(TContext context, LogicActionHandle action)
                where TContext: ILogicContext
            {
                if (!m_Transitions.TryGetValue(action, out var transition) ||
                    !transition.Action.TryGetActions(context, out var handles)) return;
                
                foreach (var handle in handles)
                {
                    ((IAction<TContext>)m_Actions[handle]).Execute(context);
                }
            }
        }
    }
}