using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Game.Core;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial class LogicDef
        {
            private Dictionary<EnumHandle, ActionInfo> m_CustomBeforeActions = new();
            private Dictionary<GoalHandle, ActionInfo> m_CustomAfterActions = new();

            private struct ActionInfo
            {
                public IAction Action;
                public CustomHandle LogicHandle;
            }

            public class RegistryAction
            {
                private readonly LogicDef m_Owner;
                private readonly IAction m_Action;
                private readonly CustomHandle m_LogicHandle; 
                
                public RegistryAction(LogicDef owner, CustomHandle logicHandle, IAction action)
                {
                    m_Owner = owner;
                    m_Action = action;
                    m_LogicHandle = logicHandle;
                }
                
                public RegistryAction BeforeAction<T>(T action)
                    where T : struct, IConvertible
                {
                    m_Owner.m_CustomBeforeActions.Add(EnumHandle.FromEnum(action), 
                        new ActionInfo 
                        {
                            Action = m_Action,
                            LogicHandle = m_LogicHandle,
                        });
                    return this;
                }

                public RegistryAction AfterChangeState<T>(T state, bool value)
                    where T : struct, IConvertible
                {
                    m_Owner.m_CustomAfterActions.Add(GoalHandle.FromEnum(state, value), 
                        new ActionInfo 
                        {
                            Action = m_Action,
                            LogicHandle = m_LogicHandle,
                        });
                    return this;
                }
            }

            public RegistryAction CustomAction<TLogic, TAction>()
                where TLogic  : IStateData
                where TAction : IAction
            {
                Assert.IsFalse(typeof(TAction).IsAssignableFrom(typeof(IAction<>)));
                return new RegistryAction(this, CustomHandle.From<TLogic>(), Activator.CreateInstance<TAction>());
            }

            public void ExecuteBeforeAction<TContext>(ref TContext context, EnumHandle action)
                where TContext: unmanaged, ILogicContext
            {
                if (m_CustomBeforeActions.TryGetValue(action, out ActionInfo info) && info.LogicHandle == context.LogicHandle)
                    ((IAction<TContext>)info.Action).Execute(ref context);
            }

            public void ExecuteAfterChangeState<TContext>(ref TContext context, GoalHandle state)
                where TContext: unmanaged, ILogicContext
            {
                if (m_CustomAfterActions.TryGetValue(state, out ActionInfo info) && info.LogicHandle == context.LogicHandle)
                    ((IAction<TContext>)info.Action).Execute(ref context);
            }
        }
    }
}