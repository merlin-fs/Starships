using System;
using System.Collections.Generic;

using Game.Core;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

using static Game.Model.Logics.Logic;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public struct GoapAction
        {
            private States m_Preconditions;
            private States m_Effects;
            private Map<LogicHandle, LogicActionHandle> m_Actions;
            private unsafe byte* m_Data;
            public LogicActionHandle Handle { get; }
            public LogicHandle System { get; }

            public float Cost { get; private set; }

            public unsafe GoapAction(LogicHandle system, LogicActionHandle handle)
            {
                Handle = handle;
                System = system;
                Cost = 1;
                m_Preconditions = new States(Allocator.Persistent);
                m_Effects = new States(Allocator.Persistent);
                m_Actions = new Map<LogicHandle, LogicActionHandle>(1, Allocator.Persistent, true);
                m_Actions.Add(system, handle);
                m_Data = null;
            }

            public void Dispose()
            {
                m_Preconditions.Dispose();
                m_Effects.Dispose();
            }

            
            public bool TryGetActions<TContext>(TContext context, out IEnumerable<LogicActionHandle> values)
                where TContext : ILogicContext
            {
                return m_Actions.TryGetValues(context.LogicHandle, out values);
            }

            public bool CanTransition(DynamicBuffer<WorldState> states, LogicDef def)
            {
                foreach (var iter in m_Preconditions.GetReadOnly())
                {
                    var index = def.StateMapping[iter.Key].Index;
                    if (states[index].Value == iter.Value) continue;
#if LOGIC_DEBUG                
                    UnityEngine.Debug.Log($"[Logic] CanTransition failed - {iter.Key} - \"{!iter.Value}\"");
#endif
                    return false;

                }
                return true;
            }

            public States GetPreconditions()
            {
                return m_Preconditions;
            }

            public bool IsSuccess(DynamicBuffer<WorldState> states, LogicDef def)
            {
                foreach (var iter in m_Effects.GetReadOnly())
                {
                    var index = def.StateMapping[iter.Key].Index;
                    if (states[index].Value != iter.Value)
                        return false;
                }
                return true;
            }

            public Writer GetWriter() => new Writer(ref this);
            public GoalTools GetGoalTools() => new GoalTools(ref this);

            public readonly unsafe struct GoalTools
            {
                private readonly byte* m_Data;
                private ref GoapAction Action => ref UnsafeUtility.AsRef<GoapAction>(m_Data);
                public GoalTools(ref GoapAction action)
                {
                    if (action.m_Data == null)
                    {
                        UnsafeUtility.PinGCObjectAndGetAddress(action, out ulong handle);
                        action.m_Data = (byte*)UnsafeUtility.AddressOf(ref action);
                        UnsafeUtility.ReleaseGCObject(handle);
                    }
                    m_Data = action.m_Data;
                }
                
                public bool LeadsToGoal(EnumHandle worldState)
                {
                    return Action.m_Effects.GetReadOnly().ContainsKey(worldState);
                }

                public void ApplyPreconditions(States states)
                {
                    states.SetState(Action.m_Preconditions);
                }

                public void ApplyPreconditionsWithOutWorld(States states, States worldState)
                {
                    states.AddIntersect(Action.m_Preconditions, worldState);
                }

                public void RemoveEffect(States states)
                {
                    states.RemoveState(Action.m_Effects);
                }
            }

            public readonly unsafe struct Writer
            {
                private unsafe readonly byte* m_Data;
                
                private ref GoapAction Action => ref UnsafeUtility.AsRef<GoapAction>(m_Data);

                public Writer(ref GoapAction action)
                {
                    if (action.m_Data == null)
                    {
                        UnsafeUtility.PinGCObjectAndGetAddress(action, out ulong handle);
                        action.m_Data = (byte*)UnsafeUtility.AddressOf(ref action);
                        UnsafeUtility.ReleaseGCObject(handle);
                    }
                    m_Data = action.m_Data;
                }

                public void AddPreconditions(EnumHandle condition, bool value)
                {
                    Action.m_Preconditions.SetState(condition, value);
                }

                public void AddAction(LogicHandle system, LogicActionHandle action)
                {
                    Action.m_Actions.Add(system, action);
                }

                public void AddEffect(EnumHandle effect, bool value)
                {
                    Action.m_Effects.SetState(effect, value);
                }

                public void SetCost(float value)
                {
                    Action.Cost = value;
                }
            }
        }
    }
}
