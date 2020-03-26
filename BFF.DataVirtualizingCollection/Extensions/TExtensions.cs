using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace BFF.DataVirtualizingCollection.Extensions
{
    internal static class TExtensions
    {
        internal static T AddTo<T>(this T item, ICollection<IDisposable> collection) where T : IDisposable
        {
            collection.Add(item);
            return item;
        }
        
        internal static T AssignTo<T>(this T item, SerialDisposable serialDisposable) where T : IDisposable
        {
            serialDisposable.Disposable = item;
            return item;
        }
    }
}
