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
    public interface IHoardingPreloadingTaskBasedAsyncPageStore<T> : IAsyncPageStore<T>
    {
    }

    internal class HoardingPreloadingTaskBasedAsyncPageStore<T> : AsyncPageStoreBase<T>, IHoardingPreloadingTaskBasedAsyncPageStore<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderOptional<TItem>
        {
            IBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingPreloadingTaskBasedAsyncPageStore<TItem> Build();
        }

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> With(IBasicTaskBasedAsyncDataAccess<TItem> dataAccess, IScheduler subscribeScheduler);
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IBasicTaskBasedAsyncDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;
            private IScheduler _subscribeScheduler;

            public IBuilderOptional<TItem> With(IBasicTaskBasedAsyncDataAccess<TItem> dataAccess, IScheduler subscribeScheduler)
            {
                _dataAccess = dataAccess;
                _subscribeScheduler = subscribeScheduler;
                return this;
            }

            public IBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = pageSize;
                return this;
            }

            public IHoardingPreloadingTaskBasedAsyncPageStore<TItem> Build()
            {
                return new HoardingPreloadingTaskBasedAsyncPageStore<TItem>(_dataAccess, _dataAccess, _subscribeScheduler)
                {
                    PageSize = _pageSize
                };
            }
        }

        private readonly ITaskBasedPageFetcher<T> _pageFetcher;

        private readonly Subject<int> _pageRequests = new Subject<int>();

        private readonly IDictionary<int, Task> _preloadingTasks = new Dictionary<int, Task>();

        private HoardingPreloadingTaskBasedAsyncPageStore(
            ITaskBasedPageFetcher<T> pageFetcher,
            IPlaceholderFactory<T> placeholderFactory,
            IScheduler subscribeScheduler) 
            : base(placeholderFactory, subscribeScheduler)
        {
            _pageFetcher = pageFetcher;
            
            CompositeDisposable.Add(_pageRequests);

            _pageRequests
                .ObserveOn(subscribeScheduler)
                .Distinct()
                .SelectMany(async pageKey =>
                {
                    int offset = pageKey * PageSize;
                    T[] page;
                    if (_preloadingTasks.ContainsKey(pageKey))
                    {
                        await _preloadingTasks[pageKey];
                        if (_preloadingTasks[pageKey].IsFaulted || _preloadingTasks[pageKey].IsCanceled)
                        {
                            page = await _pageFetcher.PageFetchAsync(pageKey * PageSize, PageSize);
                        }
                        else
                        {
                            page = await Task.FromResult(PageStore[pageKey]);
                        }
                        _preloadingTasks.Remove(pageKey);
                    }
                    else
                        page = await pageFetcher.PageFetchAsync(offset, PageSize);

                    return (PageKey: pageKey, Page: page);
                })
                .Subscribe(e =>
                {
                    PageStore[e.PageKey] = e.Page;
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
        
        private void PreloadingPages(int pk)
        {
            int nextPageKey = pk + 1;
            if (!PageStore.ContainsKey(nextPageKey) && !_preloadingTasks.ContainsKey(nextPageKey))
            {
                _preloadingTasks[nextPageKey] = Task.Run(() => _pageFetcher.PageFetchAsync(nextPageKey * PageSize, PageSize))
                    .ContinueWith(t => PageStore[nextPageKey] = t.Result);
            }

            int previousPageKey = pk - 1;
            if (previousPageKey >= 0 && !PageStore.ContainsKey(previousPageKey) && !_preloadingTasks.ContainsKey(previousPageKey))
            {
                _preloadingTasks[previousPageKey] = Task.Run(() => _pageFetcher.PageFetchAsync(previousPageKey * PageSize, PageSize))
                    .ContinueWith(t => PageStore[previousPageKey] = t.Result);
            }
        }

        protected override T OnPageContained(int pageKey, int pageIndex)
        {
            if (DeferredRequests.ContainsKey(pageKey))
                DeferredRequests[pageKey].OnCompleted();

            PreloadingPages(pageKey);

            return PageStore[pageKey][pageIndex];
        }

        protected override T OnPageNotContained(int pageKey, int pageIndex)
        {
            PreloadingPages(pageKey);

            if (!DeferredRequests.ContainsKey(pageKey))
                DeferredRequests[pageKey] = new ReplaySubject<(int, T)>();
            DeferredRequests[pageKey].OnNext((pageIndex, Placeholder));

            _pageRequests.OnNext(pageKey);

            return Placeholder;
        }
    }
}