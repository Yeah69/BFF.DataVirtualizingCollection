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
    internal interface IHoardingPreloadingAsyncPageStore<T> : IAsyncPageStore<T>
    {
    }

    internal class HoardingPreloadingAsyncPageStore<T> : AsyncPageStoreBase<T>, IHoardingPreloadingAsyncPageStore<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderOptional<TItem>
        {
            IBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingPreloadingAsyncPageStore<TItem> Build();
        }

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> With(IBasicAsyncDataAccess<TItem> dataAccess, IScheduler subscribeScheduler);
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IBasicAsyncDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;
            private IScheduler _subscribeScheduler;

            public IBuilderOptional<TItem> With(IBasicAsyncDataAccess<TItem> dataAccess, IScheduler subscribeScheduler)
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

            public IHoardingPreloadingAsyncPageStore<TItem> Build()
            {
                return new HoardingPreloadingAsyncPageStore<TItem>(_dataAccess, _dataAccess, _subscribeScheduler)
                {
                    PageSize = _pageSize
                };
            }
        }

        private readonly IPageFetcher<T> _pageFetcher;

        private readonly Subject<int> _pageRequests = new Subject<int>();

        private readonly IDictionary<int, Task> _preloadingTasks = new Dictionary<int, Task>();

        private HoardingPreloadingAsyncPageStore(
            IPageFetcher<T> pageFetcher,
            IPlaceholderFactory<T> placeholderFactory,
            IScheduler subscribeScheduler) 
            : base(placeholderFactory, subscribeScheduler)
        {
            _pageFetcher = pageFetcher;
            
            CompositeDisposable.Add(_pageRequests);

            _pageRequests
                .ObserveOn(subscribeScheduler)
                .Distinct()
                .Subscribe(pageKey =>
                {
                    int offset = pageKey * PageSize;
                    int actualPageSize = Math.Min(PageSize, Count - offset);
                    T[] page;
                    if (_preloadingTasks.ContainsKey(pageKey))
                    {
                        _preloadingTasks[pageKey].Wait();
                        if (_preloadingTasks[pageKey].IsFaulted || _preloadingTasks[pageKey].IsCanceled)
                        {
                            page = _pageFetcher.PageFetch(pageKey * PageSize, actualPageSize);
                        }
                        else
                        {
                            page = PageStore[pageKey];
                        }
                        _preloadingTasks.Remove(pageKey);
                    }
                    else
                        page = pageFetcher.PageFetch(offset, actualPageSize);
                    PageStore[pageKey] = page;
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
            int nextPageKey = pk + 1;
            if (!PageStore.ContainsKey(nextPageKey) && !_preloadingTasks.ContainsKey(nextPageKey))
            {
                _preloadingTasks[nextPageKey] = Task.Run(() =>
                {
                    int offset = nextPageKey * PageSize;
                    int actualPageSize = Math.Min(PageSize, Count - offset);
                    PageStore[nextPageKey] = _pageFetcher.PageFetch(offset, actualPageSize);
                });
            }

            int previousPageKey = pk - 1;
            if (previousPageKey >= 0 && !PageStore.ContainsKey(previousPageKey) && !_preloadingTasks.ContainsKey(previousPageKey))
            {
                _preloadingTasks[previousPageKey] = Task.Run(() =>
                {
                    int offset = previousPageKey * PageSize;
                    int actualPageSize = Math.Min(PageSize, Count - offset);
                    PageStore[previousPageKey] = _pageFetcher.PageFetch(offset, actualPageSize);
                });
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