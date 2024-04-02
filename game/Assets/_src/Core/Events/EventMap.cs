using System;
using Unity.Entities;

namespace Game.Core.Events
{
    public class EventMap: EventBase<EventMap>
    {
        public enum EventType
        {
            Initialize,
            
        } 
        
        public EventType Type { get; private set; }
        public Entity Entity { get; private set; }

        public EventMap() { }

        public static EventMap GetPooled(Entity entity, EventType type)
        {
            var e = EventBase<EventMap>.GetPooled();
            e.Entity = entity;
            e.Type = type;
            return e;
        }
    }
}