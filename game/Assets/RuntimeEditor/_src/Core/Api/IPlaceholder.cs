using System;
using Game.Core.Events;

namespace Buildings
{
    public interface IPlaceHolder
    {
        IEventHandler Events { get; }
    }

    public class PlaceEvent: EventBase<PlaceEvent>
    {
        public eState State { get; private set; }

        public enum eState
        {
            Apply,
            Cancel,
        }

        public PlaceEvent() { }

        public static PlaceEvent GetPooled(eState state)
        {
            var e = EventBase<PlaceEvent>.GetPooled();
            e.State = state;
            return e;
        }
    }
}