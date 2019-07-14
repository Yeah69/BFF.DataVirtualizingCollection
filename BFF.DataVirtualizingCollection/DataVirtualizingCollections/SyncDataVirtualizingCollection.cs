using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class SyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly ISyncPageStore<T> _pageStore;

        internal SyncDataVirtualizingCollection(
            ISyncPageStore<T> pageStore,
            ICountFetcher countFetcher)
        {
            _pageStore = pageStore.AddTo(CompositeDisposable);

            Count = countFetcher.CountFetch();

            _pageStore.Count = Count;
        }

        protected override int Count { get; }

        protected override T GetItemInner(int index)
        {
            return _pageStore.Fetch(index);
        }
    }
}