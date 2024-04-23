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
            private List<LogicAction> m_BeforeActions = new();
            private List<LogicAction> m_AfterActions = new();
            private List<LogicAction> m_Actions = new();

            public LogicAction.ActionInfo Action<TAction>()
                where TAction : IAction
            {
                Assert.IsFalse(typeof(TAction).IsAssignableFrom(typeof(IAction<>)));
                return new LogicAction.ActionInfo(this, LogicHandle.FromType(this.logicInst.GetType()), Activator.CreateInstance<TAction>());
            }
            
            public void ActionExecuteBefore<TContext>(ref TContext context, EnumHandle action)
                where TContext: unmanaged, ILogicContext
            {
                /*
                //m_BeforeActions.Where()
                if (m_BeforeActions.TryGetValue(action, out ActionInfo info) && info.LogicHandle == context.LogicHandle)
                    ((IAction<TContext>)info.Action).Execute(ref context);
                    */
            }
        
            public void ActionExecuteAfter<TContext>(ref TContext context, GoalHandle state)
                where TContext: unmanaged, ILogicContext
            {
                /*
                if (m_AfterActions.TryGetValue(state, out ActionInfo info) && info.LogicHandle == context.LogicHandle)
                    ((IAction<TContext>)info.Action).Execute(ref context);
                    */
            }
            
            public void ActionExecute<TContext>(ref TContext context, GoalHandle state)
                where TContext: unmanaged, ILogicContext
            {
                /*
                if (m_Actions.TryGetValue(state, out ActionInfo info) && info.LogicHandle == context.LogicHandle)
                    ((IAction<TContext>)info.Action).Execute(ref context);
                    */
            }

            public class LogicAction
            {
                private readonly States m_Preconditions = new States(Allocator.Persistent);
                private readonly IAction m_Action;

                private LogicAction(IAction action)
                {
                    m_Action = action;
                }

                private Writer GetWriter() => new Writer(this);

                private readonly struct Writer
                {
                    private readonly LogicAction m_Action;

                    public Writer(LogicAction action)
                    {
                        m_Action = action;
                    }

                    public void AddPreconditions(EnumHandle condition, bool value)
                    {
                        m_Action.m_Preconditions.SetState(condition, value);
                    }
                }


                public class ActionConfig
                {
                    private readonly LogicDef m_Owner;
                    private readonly LogicAction m_Action;
                    private readonly LogicHandle m_LogicHandle;

                    public void AddBefore()
                    {
                        m_Owner.m_BeforeActions.Add(m_Action);
                    }

                    public void AddAfter()
                    {
                        /*
                        m_Owner.m_CustomAfterActions.Add(GoalHandle.FromEnum(state, value),
                            new ActionInfo {Action = m_Action, LogicHandle = m_LogicHandle,});
                            */
                    }
                    public void Add()
                    {
                        /*
                        m_Owner.m_CustomAfterActions.Add(GoalHandle.FromEnum(state, value),
                            new ActionInfo {Action = m_Action, LogicHandle = m_LogicHandle,});
                            */
                    }
                }
                
                public class ActionInfo
                {
                    private readonly LogicDef m_Owner;
                    private readonly LogicAction m_Action;
                    private readonly LogicHandle m_LogicHandle;
                    private Order m_Order;

                    private enum Order
                    {
                        Default,
                        Before,
                        After,
                    };

                    public ActionInfo(LogicDef owner, LogicHandle logicHandle, IAction action)
                    {
                        m_Owner = owner;
                        m_Action = new LogicAction(action);
                        m_LogicHandle = logicHandle;
                        m_Order = Order.Default;
                    }

                    public ActionConfig AddPreconditions<T>(T condition, bool value)
                        where T : struct, IConvertible
                    {
                        m_Action.GetWriter().AddPreconditions(EnumHandle.FromEnum(condition), value);
                        //new ActionConfig()
                        return null;
                    }
                }
            }
        }
    }
}