using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public struct GoapAction
        {
            private States m_Preconditions;
            private States m_Effects;
            private unsafe byte* m_Data;

            public LogicHandle Handle { get; }

            public float Cost { get; private set; }

            public unsafe GoapAction(LogicHandle handle)
            {
                Handle = handle;
                Cost = 1;
                m_Preconditions = new States(Allocator.Persistent);
                m_Effects = new States(Allocator.Persistent);
                m_Data = null;
            }


            public NativeHashMap<LogicHandle, bool>.ReadOnly GetEffects()
            {
                return m_Effects.GetReadOnly();
            }


            public void Dispose()
            {
                m_Preconditions.Dispose();
                m_Effects.Dispose();
            }

            public bool CanTransition(States states)
            {
                return m_Preconditions.All(states);
            }

            public bool CanTransition(DynamicBuffer<WorldState> states, LogicDef def)
            {
                foreach (var iter in m_Preconditions.GetReadOnly())
                {
                    var index = def.StateMapping[iter.Key].Index;
                    if (states[index].Value != iter.Value)
                        return false;
                }
                return true;
            }

            public States GetPreconditions()
            {
                return m_Preconditions;
            }

            public void ApplyEffect(BufferLookup<WorldState> states, LogicDef def)
            {
                //states.SetState(m_Effects);
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
                private unsafe readonly byte* m_Data;
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
                
                public bool LeadsToGoal(States states)
                {
                    return Action.m_Effects.Any(states);
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

                public void AddPreconditions(LogicHandle condition, bool value)
                {
                    Action.m_Preconditions.SetState(condition, value);
                }

                public void AddEffect(LogicHandle effect, bool value)
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
