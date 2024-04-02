using System;
using Unity.Entities;

namespace Game.Core.Events
{
    using Common.Defs;

    public class EventSpawn: EventBase<EventSpawn>
    {
        public IConfig Config { get; private set; }
        public Entity Entity { get; private set; }

        public EventSpawn() { }

        public static EventSpawn GetPooled(Entity entity, IConfig config)
        {
            var e = EventBase<EventSpawn>.GetPooled();
            e.Config = config;
            e.Entity = entity;
            return e;
        }
    }
}