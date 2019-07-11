using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in sync way with task-based data access, which means that it does block the current thread if the element isn't available yet.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    internal interface IHoardingPreloadingTaskBasedSyncPageStore<out T> : ISyncPageStore<T>
    {
    }
    
    internal class HoardingPreloadingTaskBasedSyncPageStore<T> : SyncPageStoreBase<T>, IHoardingPreloadingTaskBasedSyncPageStore<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderOptional<out TItem>
        {
            IBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingPreloadingTaskBasedSyncPageStore<TItem> Build();
        }

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> With(
                IBasicTaskBasedSyncDataAccess<TItem> dataAccess,
                Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory);
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IBasicTaskBasedSyncDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;
            private Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>>
                _pageReplacementStrategyFactory;

            public IBuilderOptional<TItem> With(
                IBasicTaskBasedSyncDataAccess<TItem> dataAccess,
                Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
            {
                _dataAccess = dataAccess;
                _pageReplacementStrategyFactory = pageReplacementStrategyFactory;
                return this;
            }

            public IBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = pageSize;
                return this;
            }

            public IHoardingPreloadingTaskBasedSyncPageStore<TItem> Build()
            {
                return new HoardingPreloadingTaskBasedSyncPageStore<TItem>(_dataAccess, _pageReplacementStrategyFactory)
                {
                    PageSize = _pageSize
                };
            }
        }

        private readonly ITaskBasedPageFetcher<T> _pageFetcher;

        private readonly IDictionary<int, Task<T[]>> _preloadingTasks = new Dictionary<int, Task<T[]>>();

        private HoardingPreloadingTaskBasedSyncPageStore(
            ITaskBasedPageFetcher<T> pageFetcher,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
            : base(pageReplacementStrategyFactory)
        {
            _pageFetcher = pageFetcher;
        }

        protected override T OnPageNotContained(int pageKey, int pageIndex)
        {
            if (!_preloadingTasks.TryGetValue(pageKey, out var loadingTask))
                return FetchPageAsync(pageKey).Result[pageIndex];

            loadingTask.Wait();
            _preloadingTasks.Remove(pageKey);
            return loadingTask.IsFaulted || loadingTask.IsCanceled
                ? FetchPageAsync(pageKey).Result[pageIndex]
                : loadingTask.Result[pageIndex];
        }

        protected override void OnPageContained(int pageKey)
        {
            int nextPageKey = pageKey + 1;
            if (nextPageKey < Count % PageSize) PreloadPage(nextPageKey);

            int previousPageKey = pageKey - 1;
            if (previousPageKey >= 0) PreloadPage(previousPageKey);

            void PreloadPage(int preloadingPageKey)
            {
                if (PageStore.ContainsKey(preloadingPageKey) || _preloadingTasks.ContainsKey(preloadingPageKey)) return;

                Requests.OnNext((preloadingPageKey, -1));
                _preloadingTasks[preloadingPageKey] =
                    Task.Run(() => FetchPageAsync(preloadingPageKey));
            }
        }

        private async Task<T[]> FetchPageAsync(int pageKey)
        {
            var offset = pageKey * PageSize;
            var actualPageSize = Math.Min(PageSize, Count - offset);
            var page = await _pageFetcher.PageFetchAsync(offset, actualPageSize).ConfigureAwait(false);
            PageStore[pageKey] = page;
            return page;
        }
    }
}