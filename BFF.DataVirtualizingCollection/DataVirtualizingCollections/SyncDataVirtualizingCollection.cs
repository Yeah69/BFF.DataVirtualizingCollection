using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class SyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        internal static IDataVirtualizingCollectionBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IDataVirtualizingCollectionBuilderRequired<TItem>
        {
            IDataVirtualizingCollectionBuilderOptional<TItem> WithPageStore(
                ISyncPageStore<TItem> pageStore,
                ICountFetcher countFetcher);
        }
        internal interface IDataVirtualizingCollectionBuilderOptional<TItem>
        {
            IDataVirtualizingCollection<TItem> Build();
        }

        internal class Builder<TItem> : IDataVirtualizingCollectionBuilderRequired<TItem>, IDataVirtualizingCollectionBuilderOptional<TItem>
        {
            private ISyncPageStore<TItem> _pageStore;
            private ICountFetcher _countFetcher;

            

            public IDataVirtualizingCollection<TItem> Build()
            {
                return new SyncDataVirtualizingCollection<TItem>(
                    _pageStore, 
                    _countFetcher);
            }

            public IDataVirtualizingCollectionBuilderOptional<TItem> WithPageStore(
                ISyncPageStore<TItem> pageStore,
                ICountFetcher countFetcher)
            {
                _pageStore = pageStore;
                _countFetcher = countFetcher;
                return this;
            }
        }

        private readonly ISyncPageStore<T> _pageStore;

        private SyncDataVirtualizingCollection(
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