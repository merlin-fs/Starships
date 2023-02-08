using System;
using Unity.Entities;
using Unity.Properties;

namespace Game.Model.Logics
{
    public readonly partial struct LogicAspect : IAspect
    {
        private readonly Entity m_Self;
        public Entity Self => m_Self;
        private readonly RefRW<Logic> m_Logic;

        [CreateProperty]
        public LogicHandle CurrentAction => m_Logic.ValueRO.CurrentAction;

        [CreateProperty]
        public bool IsWork => m_Logic.ValueRO.IsWork;

        public void Done(LogicHandle worldState, bool value)
        {
            m_Logic.ValueRO.SetDone();
        }

        public void SetState(LogicHandle value)
        {
            m_Logic.ValueRW.SetState(value);
        }

        public LogicHandle GetNextState()
        {
            return m_Logic.ValueRO.GetNextState();
        }
    }
}