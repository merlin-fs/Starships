using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Events
{
    public class EventRepository: EventBase<EventRepository>
    {
        public Enum State { get; private set; }
        public object Repository { get; private set; }

        public enum Enum
        {
            Loading,
            Done,
        }

        public static EventRepository GetPooled(object repository, Enum state)
        {
            var e = EventBase<EventRepository>.GetPooled();
            e.State = state;
            e.Repository = repository;
            return e;
        }
    }
}