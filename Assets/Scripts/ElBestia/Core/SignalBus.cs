using System;
using System.Collections.Generic;

namespace ElBestia.Core
{
    public static class SignalBus
    {
        private static readonly Dictionary<Type, List<Delegate>> SubscribersByType = new Dictionary<Type, List<Delegate>>();

        public static IDisposable Subscribe<TSignal>(Action<TSignal> callback)
        {
            if (callback == null)
            {
                return EmptySubscription.Instance;
            }

            Type signalType = typeof(TSignal);
            if (!SubscribersByType.TryGetValue(signalType, out List<Delegate> subscribers))
            {
                subscribers = new List<Delegate>();
                SubscribersByType.Add(signalType, subscribers);
            }

            subscribers.Add(callback);
            return new SignalSubscription(() => Unsubscribe(callback));
        }

        public static IDisposable Suscribe<TSignal>(Action<TSignal> callback)
        {
            return Subscribe(callback);
        }

        public static void Unsubscribe<TSignal>(Action<TSignal> callback)
        {
            if (callback == null)
            {
                return;
            }

            Type signalType = typeof(TSignal);
            if (!SubscribersByType.TryGetValue(signalType, out List<Delegate> subscribers))
            {
                return;
            }

            subscribers.Remove(callback);
            if (subscribers.Count == 0)
            {
                SubscribersByType.Remove(signalType);
            }
        }

        public static void Fire<TSignal>(TSignal signal)
        {
            Type signalType = typeof(TSignal);
            if (!SubscribersByType.TryGetValue(signalType, out List<Delegate> subscribers) || subscribers.Count == 0)
            {
                return;
            }

            Delegate[] snapshot = subscribers.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i] is Action<TSignal> callback)
                {
                    callback.Invoke(signal);
                }
            }
        }

        public static void Clear()
        {
            SubscribersByType.Clear();
        }

        private sealed class SignalSubscription : IDisposable
        {
            private readonly Action dispose;
            private bool isDisposed;

            public SignalSubscription(Action dispose)
            {
                this.dispose = dispose;
            }

            public void Dispose()
            {
                if (isDisposed)
                {
                    return;
                }

                isDisposed = true;
                dispose?.Invoke();
            }
        }

        private sealed class EmptySubscription : IDisposable
        {
            public static readonly EmptySubscription Instance = new EmptySubscription();

            private EmptySubscription()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
