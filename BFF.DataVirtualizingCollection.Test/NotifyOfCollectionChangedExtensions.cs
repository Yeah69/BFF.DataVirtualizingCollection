using System;
using System.Collections.Specialized;
using System.Reactive.Linq;

namespace BFF.DataVirtualizingCollection.Test
{
    public static class NotifyCollectionChangedExtensions
    {
        public static IObservable<NotifyCollectionChangedEventArgs> ObserveCollectionChanges(
            this INotifyCollectionChanged source)
        {
            return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler => handler.Invoke,
                    h => source.CollectionChanged += h,
                    h => source.CollectionChanged -= h)
                .Select(_ => _.EventArgs);
        }
    }
}