// CatchReporter.cs
// Observer pattern: the rod publishes ONE report per cast; any number of listeners react.
// Listeners are notified in the order they subscribed.
using System.Collections.Generic;

namespace Tidebook
{
    public class CatchReporter : ICatchPublisher, ICatchEvents
    {
        readonly List<ICatchListener> listeners = new List<ICatchListener>();

        public void Subscribe(ICatchListener listener)
        {
            if (listener != null && !listeners.Contains(listener)) listeners.Add(listener);
        }

        public void Unsubscribe(ICatchListener listener)
        {
            listeners.Remove(listener);
        }

        public void Publish(CatchReport report)
        {
            for (int i = 0; i < listeners.Count; i++) listeners[i].OnCatchResolved(report);
        }
    }
}
