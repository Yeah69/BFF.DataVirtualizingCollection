using System;
using System.Collections.Generic;

namespace BFF.DataVirtualizingCollection.Extensions
{
    // ReSharper disable once InconsistentNaming
    internal static class TExtensions
    {
        internal static T AddTo<T>(this T item, ICollection<IDisposable> collection) where T : IDisposable
        {
            collection.Add(item);
            return item;
        }
    }
}
