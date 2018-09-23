using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in sync way, which means that it does block the current thread if the element isn't available yet.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    internal interface IHoardingPreloadingSyncPageStore<out T> : ISyncPageStore<T>
    {
    }
    
    internal class HoardingPreloadingSyncPageStore<T> : SyncPageStoreBase<T>, IHoardingPreloadingSyncPageStore<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderOptional<out TItem>
        {
            IBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingPreloadingSyncPageStore<TItem> Build();
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

            public IHoardingPreloadingSyncPageStore<TItem> Build()
            {
                return new HoardingPreloadingSyncPageStore<TItem>(_dataAccess)
                {
                    PageSize = _pageSize
                };
            }
        }

        private readonly IPageFetcher<T> _pageFetcher;

        private readonly IDictionary<int, Task<T[]>> _preloadingTasks = new Dictionary<int, Task<T[]>>();

        private HoardingPreloadingSyncPageStore(IPageFetcher<T> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        protected override void OnPageNotContained(int pageKey, int pageIndex)
        {
            if (_preloadingTasks.ContainsKey(pageKey))
            {
                _preloadingTasks[pageKey].Wait();
                if (_preloadingTasks[pageKey].IsFaulted || _preloadingTasks[pageKey].IsCanceled)
                {
                    int offset = pageKey * PageSize;
                    int actualPageSize = Math.Min(PageSize, Count - offset);
                    PageStore[pageKey] = _pageFetcher.PageFetch(offset, actualPageSize);
                }
                _preloadingTasks.Remove(pageKey);
            }
            else
            {
                int offset = pageKey * PageSize;
                int actualPageSize = Math.Min(PageSize, Count - offset);
                PageStore[pageKey] = _pageFetcher.PageFetch(offset, actualPageSize);
            }
        }

        protected override T OnPageContained(int pageKey, int pageIndex)
        {
            int nextPageKey = pageKey + 1;
            if (!PageStore.ContainsKey(nextPageKey) && !_preloadingTasks.ContainsKey(nextPageKey))
                _preloadingTasks[nextPageKey] = Task.Run(() =>
                {
                    int offset = nextPageKey * PageSize;
                    int actualPageSize = Math.Min(PageSize, Count - offset);
                    return _pageFetcher.PageFetch(offset, actualPageSize);
                })
                .ContinueWith(t => PageStore[nextPageKey] = t.Result);

            int previousPageKey = pageKey - 1;
            if (previousPageKey >= 0 && !PageStore.ContainsKey(previousPageKey) && !_preloadingTasks.ContainsKey(previousPageKey))
                _preloadingTasks[previousPageKey] = Task.Run(() =>
                {
                    int offset = previousPageKey * PageSize;
                    int actualPageSize = Math.Min(PageSize, Count - offset);
                    return _pageFetcher.PageFetch(offset, actualPageSize);
                })
                .ContinueWith(t => PageStore[previousPageKey] = t.Result);

            return PageStore[pageKey][pageIndex];
        }
    }
}