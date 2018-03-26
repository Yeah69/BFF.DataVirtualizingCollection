using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in sync way with task-based data access, which means that it does block the current thread if the element isn't available yet.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    public interface IHoardingTaskBasedSyncPageStore<out T> : ISyncPageStore<T>
    {
    }
    
    internal class HoardingTaskBasedSyncPageStore<T> : SyncPageStoreBase<T>, IHoardingTaskBasedSyncPageStore<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderOptional<out TItem>
        {
            IBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingTaskBasedSyncPageStore<TItem> Build();
        }

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> With(IBasicTaskBasedSyncDataAccess<TItem> dataAccess);
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IBasicTaskBasedSyncDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;

            public IBuilderOptional<TItem> With(IBasicTaskBasedSyncDataAccess<TItem> dataAccess)
            {
                _dataAccess = dataAccess;
                return this;
            }

            public IBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = pageSize;
                return this;
            }

            public IHoardingTaskBasedSyncPageStore<TItem> Build()
            {
                return new HoardingTaskBasedSyncPageStore<TItem>(_dataAccess)
                {
                    PageSize = _pageSize
                };
            }
        }

        private readonly ITaskBasedPageFetcher<T> _pageFetcher;

        private HoardingTaskBasedSyncPageStore(ITaskBasedPageFetcher<T> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        protected override void OnPageNotContained(int pageKey, int pageIndex)
        {
            PageStore[pageKey] = _pageFetcher.PageFetchAsync(pageKey * PageSize, PageSize).Result;
        }

        protected override T OnPageContained(int pageKey, int pageIndex)
        {
            return PageStore[pageKey][pageIndex];
        }
    }
}