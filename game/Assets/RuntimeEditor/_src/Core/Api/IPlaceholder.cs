using System;
using Game.Core.Events;
using Unity.Entities;

namespace Buildings
{
    public interface IPlaceHolder
    {
        void Cancel();
    }

    public class EventPlace: EventBase<EventPlace>
    {
        public State Value { get; private set; }
        public Entity Entity { get; private set; }

        public enum State
        {
            New,
            Apply,
            Cancel,
        }

        public static EventPlace GetPooled(Entity entity, State state)
        {
            var e = EventBase<EventPlace>.GetPooled();
            e.Entity = entity;
            e.Value = state;
            return e;
        }
    }
}