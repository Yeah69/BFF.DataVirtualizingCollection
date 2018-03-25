using System;
using System.Collections.Generic;

namespace BFF.DataVirtualizingCollection.Extensions
{
    internal static class CollectionExtensions
    {
        public static T AddTo<T>(this T item, ICollection<T> collection)
        {
            collection.Add(item);
            return item;
        }

        public static T AddTo<T>(this T item, ICollection<IDisposable> collection) where T : IDisposable
        {
            collection.Add(item);
            return item;
        }
    }
}
