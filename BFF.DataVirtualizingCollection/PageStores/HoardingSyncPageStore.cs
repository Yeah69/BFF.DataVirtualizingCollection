using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in sync way, which means that it does block the current thread if the element isn't available yet.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    public interface IHoardingSyncPageStore<T> : ISyncPageStore<T>
    {
    }
    
    internal class HoardingSyncPageStore<T> : SyncPageStoreBase<T>, IHoardingSyncPageStore<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderOptional<TItem>
        {
            IBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingSyncPageStore<TItem> Build();
        }

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> With(IBasicSyncDataAccess<TItem> dataAccess);
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IBasicSyncDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;

            public IBuilderOptional<TItem> With(IBasicSyncDataAccess<TItem> dataAccess)
            {
                _dataAccess = dataAccess;
                return this;
            }

            public IBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = pageSize;
                return this;
            }

            public IHoardingSyncPageStore<TItem> Build()
            {
                return new HoardingSyncPageStore<TItem>(_dataAccess)
                {
                    PageSize = _pageSize
                };
            }
        }

        private readonly IPageFetcher<T> _pageFetcher;

        private HoardingSyncPageStore(IPageFetcher<T> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        protected override void OnPageContained(int pageKey, int pageIndex)
        {
            PageStore[pageKey] = _pageFetcher.PageFetch(pageKey * PageSize, PageSize);
        }

        protected override T OnPageNotContained(int pageKey, int pageIndex)
        {
            return PageStore[pageKey][pageIndex];
        }
    }
}