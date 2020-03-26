using System;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStorage;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    internal sealed class SyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly IPageStorage<T> _pageStorage;

        internal SyncDataVirtualizingCollection(
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<int> countFetcher)
        {
            Count = countFetcher();
            _pageStorage = pageStoreFactory(Count).AddTo(CompositeDisposable);
        }

        protected override int Count { get; }

        protected override T GetItemInner(int index)
        {
            return _pageStorage[index];
        }

        public override Task InitializationCompleted { get; } = Task.CompletedTask;
    }
}