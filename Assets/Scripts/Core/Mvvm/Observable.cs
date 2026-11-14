// Observable.cs
// The data-binding primitive for MVVM. A ViewModel exposes its state as Observable<T> fields; a View
// binds a handler to one and is told whenever the value changes. The ViewModel never references the View,
// so it has no UI types in it and can be tested without Unity.
//
//   vm.Coins.Bind(OnCoins);    // calls OnCoins(current) right away, then again on every change
//   vm.Coins.Unbind(OnCoins);  // stop listening (a View does this when it goes away)
//
// Setting the same value again does nothing. For a reference type, assigning a NEW object always counts as a
// change, which is how CatchResultViewModel makes two identical catches in a row show the banner twice.

using System.Collections.Generic;

namespace Tidebook
{
    public class Observable<T>
    {
        T current;

        /// <summary>Raised after the value changes, with the new value.</summary>
        public event System.Action<T> Changed;

        public Observable() { }

        public Observable(T initial)
        {
            current = initial;
        }

        public T Value
        {
            get { return current; }
            set
            {
                if (EqualityComparer<T>.Default.Equals(current, value)) return;
                current = value;

                System.Action<T> handler = Changed;
                if (handler != null) handler(current);
            }
        }

        /// <summary>Listens for changes, and also calls the handler once with the current value so the View starts in sync.</summary>
        public void Bind(System.Action<T> handler)
        {
            Changed += handler;
            handler(current);
        }

        public void Unbind(System.Action<T> handler)
        {
            Changed -= handler;
        }
    }
}
