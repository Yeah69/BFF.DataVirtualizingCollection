using System;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal interface IPage<out T> : IDisposable
    {
        T this[int index] { get; }
    }
}
