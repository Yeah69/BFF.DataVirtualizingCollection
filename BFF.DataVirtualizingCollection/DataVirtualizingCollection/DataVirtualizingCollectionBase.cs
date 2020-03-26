using System;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    internal abstract class DataVirtualizingCollectionBase<T> : VirtualizationBase<T>, IDataVirtualizingCollection<T>
    {
        protected override T IndexerInnerGet(int index) =>
            index >= Count || index < 0
                ? throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                : GetItemInner(index);

        protected abstract T GetItemInner(int index);
    }
}