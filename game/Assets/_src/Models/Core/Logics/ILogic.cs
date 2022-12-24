using System;
using Unity.Entities;
using Unity.Jobs;

namespace Game.Model
{
    public interface ILogic
    {
        public enum Result
        {
            Error,
            Done,
            Busy,
        }
    }

    public interface ILogicTransition: IJob
    {
        int ID { get; set; }
        void Init(Entity entity, ref SystemState state, LogicAspect logic);
    }
}