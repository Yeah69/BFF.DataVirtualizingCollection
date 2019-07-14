using System;
using System.Collections.Generic;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in sync way with task-based data access, which means that it does block the current thread if the element isn't available yet.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    internal interface ISyncNonPreloadingTaskBasedPageStore<out T> : ISyncPageStore<T>
    {
    }
    
    internal class SyncNonPreloadingTaskBasedPageStore<T> : SyncPageStoreBase<T>, ISyncNonPreloadingTaskBasedPageStore<T>
    {
        private readonly ITaskBasedPageFetcher<T> _pageFetcher;

        internal SyncNonPreloadingTaskBasedPageStore(
            int pageSize,
            ITaskBasedPageFetcher<T> pageFetcher,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
            : base(
                pageSize,
                pageReplacementStrategyFactory)
        {
            _pageFetcher = pageFetcher;
        }

        protected override T OnPageNotContained(int pageKey, int pageIndex)
        {
            var offset = pageKey * PageSize;
            var actualPageSize = Math.Min(PageSize, Count - offset);
            var page = _pageFetcher.PageFetchAsync(offset, actualPageSize).Result;
            PageStore[pageKey] = page;
            return page[pageIndex];
        }
    }
}