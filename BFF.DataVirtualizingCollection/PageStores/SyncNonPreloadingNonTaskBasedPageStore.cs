using System;
using System.Collections.Generic;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in sync way, which means that it does block the current thread if the element isn't available yet.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    internal interface ISyncNonPreloadingNonTaskBasedPageStore<out T> : ISyncPageStore<T>
    {
    }
    
    internal class SyncNonPreloadingNonTaskBasedPageStore<T> : SyncPageStoreBase<T>, ISyncNonPreloadingNonTaskBasedPageStore<T>
    {
        private readonly IPageFetcher<T> _pageFetcher;

        internal SyncNonPreloadingNonTaskBasedPageStore(
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
            var offset = pageKey * PageSize;
            var actualPageSize = Math.Min(PageSize, Count - offset);
            var page = _pageFetcher.PageFetch(offset, actualPageSize);
            PageStore[pageKey] = page;
            return page[pageIndex];
        }
    }
}