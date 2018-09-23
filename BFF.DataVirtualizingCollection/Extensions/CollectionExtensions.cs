using System;
using System.Collections.Generic;

namespace BFF.DataVirtualizingCollection.Extensions
{
    internal static class CollectionExtensions
    {
        internal static T AddTo<T>(this T item, ICollection<IDisposable> collection) where T : IDisposable
        {
            collection.Add(item);
            return item;
        }
    }
}
