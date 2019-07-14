using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in async way, which means that it doesn't block the current thread and in case the element isn't available a placeholder is provided.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    internal interface IAsyncPreloadingNonTaskBasedPageStore<T> : IAsyncPageStore<T>
    {
    }

    internal class AsyncPreloadingNonTaskBasedPageStore<T> : AsyncPageStoreBase<T>, IAsyncPreloadingNonTaskBasedPageStore<T>
    {
        private readonly IPageFetcher<T> _pageFetcher;

        private readonly Subject<int> _pageRequests = new Subject<int>();

        private readonly IDictionary<int, Task<T[]>> _preloadingTasks = new Dictionary<int, Task<T[]>>();

        internal AsyncPreloadingNonTaskBasedPageStore(
            int pageSize,
            IPageFetcher<T> pageFetcher,
            IPlaceholderFactory<T> placeholderFactory,
            IScheduler subscribeScheduler,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory) 
            : base(
                pageSize,
                placeholderFactory,
                subscribeScheduler, 
                pageReplacementStrategyFactory)
        {
            _pageFetcher = pageFetcher;
            
            CompositeDisposable.Add(_pageRequests);

            _pageRequests
                .ObserveOn(subscribeScheduler)
                .Distinct()
                .Subscribe(pageKey =>
                {
                    T[] page;
                    if (!_preloadingTasks.TryGetValue(pageKey, out var loadingTask))
                        page = FetchPage(pageKey);
                    else
                    {
                        loadingTask.Wait();
                        _preloadingTasks.Remove(pageKey);
                        page = loadingTask.IsFaulted || loadingTask.IsCanceled
                            ? FetchPage(pageKey)
                            : loadingTask.Result;
                    }

                    if (DeferredRequests.ContainsKey(pageKey))
                    {
                        var disposable = DeferredRequests[pageKey]
                            .ObserveOn(subscribeScheduler)
                            .Distinct()
                            .Subscribe(tuple =>
                            {
                                OnCollectionChangedReplaceSubject.OnNext(
                                    (page[tuple.Item1], tuple.Item2, pageKey * PageSize + tuple.Item1));
                            }, () => DeferredRequests.Remove(pageKey));
                        CompositeDisposable.Add(disposable);
                    }
                });
        }
        
        private void PreloadingPages(int pk)
        {
            var nextPageKey = pk + 1;
            if (nextPageKey < Count % PageSize) PreloadPage(nextPageKey);

            var previousPageKey = pk - 1;
            if (previousPageKey >= 0) PreloadPage(previousPageKey);

            void PreloadPage(int preloadingPageKey)
            {
                if (PageStore.ContainsKey(preloadingPageKey) || _preloadingTasks.ContainsKey(preloadingPageKey)) return;

                Requests.OnNext((preloadingPageKey, -1));
                _preloadingTasks[preloadingPageKey] = Task.Run(() => FetchPage(preloadingPageKey));
            }
        }

        protected override void OnPageContained(int pageKey)
        {
            if (DeferredRequests.ContainsKey(pageKey))
                DeferredRequests[pageKey].OnCompleted();

            PreloadingPages(pageKey);
        }

        protected override T OnPageNotContained(int pageKey, int pageIndex)
        {
            if (!DeferredRequests.ContainsKey(pageKey))
                DeferredRequests[pageKey] = new ReplaySubject<(int, T)>();
            DeferredRequests[pageKey].OnNext((pageIndex, Placeholder));

            _pageRequests.OnNext(pageKey);

            PreloadingPages(pageKey);

            return Placeholder;
        }

        private T[] FetchPage(int pageKey)
        {
            var offset = pageKey * PageSize;
            var actualPageSize = Math.Min(PageSize, Count - offset);
            var page = _pageFetcher.PageFetch(offset, actualPageSize);
            if (DisposeOnArrival)
                PageDisposal(page);
            else
                PageStore[pageKey] = page;
            return page;
        }
    }
}