using System;
using System.Collections.Generic;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal class PreloadingPageStorage<T> : PageStorage<T>
    {
        private readonly Func<int, int, int, IPage<T>> _preloadingPageFactory;

        internal PreloadingPageStorage(
            int pageSize,
            int count,
            Func<int, int, int, IPage<T>> nonPreloadingPageFactory,
            Func<int, int, int, IPage<T>> preloadingPageFactory,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
            : base (
                pageSize, 
                count, 
                nonPreloadingPageFactory, 
                pageReplacementStrategyFactory)
        {
            _preloadingPageFactory = preloadingPageFactory;
        }

        protected override void Preloading(int pageKey)
        {
            var previousPageKey = pageKey - 1;
            if (previousPageKey >= 0)
                Pages.GetOrAdd(
                    previousPageKey,
                    FetchPreloadingPage);
            var nextPageKey = pageKey + 1;
            if (nextPageKey < PageCount)
                Pages.GetOrAdd(
                    nextPageKey,
                    FetchPreloadingPage);

            IPage<T> FetchPreloadingPage(int key)
            {
                lock (IsDisposedLock)
                {
                    if (!IsDisposed) Requests.OnNext((key, -1));
                }

                return FetchPage(key, _preloadingPageFactory);
            }
        }
    }
}
