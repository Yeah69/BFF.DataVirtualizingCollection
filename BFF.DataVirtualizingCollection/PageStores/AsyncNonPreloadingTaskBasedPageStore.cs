using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in async and task-based way, which means that it doesn't block the current thread and in case the element isn't available a placeholder is provided.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    internal interface IAsyncNonPreloadingTaskBasedPageStore<T> : IAsyncPageStore<T>
    {
    }
    
    internal class AsyncNonPreloadingTaskBasedPageStore<T> : AsyncPageStoreBase<T>, IAsyncNonPreloadingTaskBasedPageStore<T>
    {
        private readonly Subject<int> _pageRequests = new Subject<int>();

        internal AsyncNonPreloadingTaskBasedPageStore(
            int pageSize,
            ITaskBasedPageFetcher<T> pageFetcher,
            IPlaceholderFactory<T> placeholderFactory,
            IScheduler subscribeScheduler,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory) 
            : base(
                pageSize, 
                placeholderFactory, 
                subscribeScheduler, 
                pageReplacementStrategyFactory)
        {
            CompositeDisposable.Add(_pageRequests);

            _pageRequests
                .ObserveOn(subscribeScheduler)
                .Distinct()
                .SelectMany(async pageKey =>
                {
                    var offset = pageKey * PageSize;
                    var actualPageSize = Math.Min(PageSize, Count - offset);
                    var page = await pageFetcher.PageFetchAsync(offset, actualPageSize).ConfigureAwait(false);
                    if (DisposeOnArrival)
                        PageDisposal(page);
                    else
                        PageStore[pageKey] = page;
                    return (PageKey: pageKey, Page: page);
                })
                .Subscribe(e =>
                {
                    if (DeferredRequests.ContainsKey(e.PageKey))
                    {
                        var disposable = DeferredRequests[e.PageKey]
                            .ObserveOn(subscribeScheduler)
                            .Distinct()
                            .Subscribe(tuple =>
                            {
                                OnCollectionChangedReplaceSubject.OnNext(
                                    (e.Page[tuple.Item1], tuple.Item2, e.PageKey * PageSize + tuple.Item1));
                            }, () => DeferredRequests.Remove(e.PageKey));
                        CompositeDisposable.Add(disposable);
                    }
                });
        }

        protected override void OnPageContained(int pageKey)
        {
            if (DeferredRequests.TryGetValue(pageKey, out var deferredRequest))
                deferredRequest.OnCompleted();
        }

        protected override T OnPageNotContained(int pageKey, int pageIndex)
        {
            if (DeferredRequests.TryGetValue(pageKey, out var deferredRequest))
                deferredRequest.OnNext((pageIndex, Placeholder));
            else
            {
                var subject = new ReplaySubject<(int, T)>();
                subject.OnNext((pageIndex, Placeholder));
                DeferredRequests[pageKey] = subject;
            }

            _pageRequests.OnNext(pageKey);

            return Placeholder;
        }
    }
}