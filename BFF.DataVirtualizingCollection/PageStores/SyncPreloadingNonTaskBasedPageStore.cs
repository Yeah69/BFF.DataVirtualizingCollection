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
    internal interface ISyncPreloadingNonTaskBasedPageStore<out T> : ISyncPageStore<T>
    {
    }
    
    internal class SyncPreloadingNonTaskBasedPageStore<T> : SyncPageStoreBase<T>, ISyncPreloadingNonTaskBasedPageStore<T>
    {
        private readonly IPageFetcher<T> _pageFetcher;

        private readonly IDictionary<int, Task<T[]>> _preloadingTasks = new Dictionary<int, Task<T[]>>();

        internal SyncPreloadingNonTaskBasedPageStore(
            int pageSize,
            IPageFetcher<T> pageFetcher,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory) 
            : base(
                pageSize, 
                pageReplacementStrategyFactory)
        {
            _pageFetcher = pageFetcher;
        }

        protected override T OnPageNotContained(int pageKey, int pageIndex)
        {
            if (!_preloadingTasks.TryGetValue(pageKey, out var loadingTask))
                return FetchPage(pageKey)[pageIndex];

            loadingTask.Wait();
            _preloadingTasks.Remove(pageKey);
            return loadingTask.IsFaulted || loadingTask.IsCanceled
                ? FetchPage(pageKey)[pageIndex]
                : loadingTask.Result[pageIndex];
        }

        protected override void OnPageContained(int pageKey)
        {
            var nextPageKey = pageKey + 1;
            if (nextPageKey < Count % PageSize) PreloadPage(nextPageKey);

            var previousPageKey = pageKey - 1;
            if (previousPageKey >= 0) PreloadPage(previousPageKey);

            void PreloadPage(int preloadingPageKey)
            {
                if (PageStore.ContainsKey(preloadingPageKey) || _preloadingTasks.ContainsKey(preloadingPageKey)) return;

                Requests.OnNext((preloadingPageKey, -1));
                _preloadingTasks[preloadingPageKey] = 
                    Task.Run(() => FetchPage(preloadingPageKey));
            }
        }
        private T[] FetchPage(int pageKey)
        {
            var offset = pageKey * PageSize;
            var actualPageSize = Math.Min(PageSize, Count - offset);
            var page = _pageFetcher.PageFetch(offset, actualPageSize);
            PageStore[pageKey] = page;
            return page;
        }
    }
}