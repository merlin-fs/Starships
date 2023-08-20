using System;

namespace UnityEngine.UIElements
{
    public static class VisualElementScheduledItemHelper
    {
        private static readonly Func<Boolean> TheEndOfTime = () => false;
 
        public static IVisualElementScheduledItem EveryFrame(this IVisualElementScheduledItem scheduledItem)
        {
            return scheduledItem.Until(TheEndOfTime);
        }
 
        public static IVisualElementScheduledItem When(this IVisualElementScheduledItem scheduledItem, Func<Boolean> predicate)
        {
            scheduledItem.Pause();
            return scheduledItem.element.schedule.Execute(() =>
            {
                if (predicate.Invoke())
                {
                    scheduledItem.Resume();
                }
            });
        }    
    }
}