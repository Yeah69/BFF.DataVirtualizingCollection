using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
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
    public interface IHoardingPreloadingAsyncPageStore<T> : IAsyncPageStore<T>
    {
    }

    /// <inheritdoc />
    internal class HoardingPreloadingAsyncPageStore<T> : IHoardingPreloadingAsyncPageStore<T>
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
                    _pageSize = _pageSize
                };
            }
        }

        private readonly IPageFetcher<T> _pageFetcher;
        private readonly IPlaceholderFactory<T> _placeholderFactory;
        private int _pageSize = 100;

        private readonly Subject<int> _pageRequests = new Subject<int>();

        private readonly IDictionary<int, Task> _preloadingTasks = new Dictionary<int, Task>();

        private readonly IDictionary<int, ReplaySubject<(int, T)>> _deferredRequests = new Dictionary<int, ReplaySubject<(int, T)>>();
        private readonly IDictionary<int, T[]> _pageStore = new Dictionary<int, T[]>();

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private HoardingPreloadingAsyncPageStore(IPageFetcher<T> pageFetcher, IPlaceholderFactory<T> placeholderFactory, IScheduler subscribeScheduler)
        {
            _pageFetcher = pageFetcher;
            _placeholderFactory = placeholderFactory;

            var onCollectionChangedReplace = new Subject<(T, T, int)>();
            OnCollectionChangedReplace = onCollectionChangedReplace;

            _compositeDisposable.Add(onCollectionChangedReplace);
            _compositeDisposable.Add(_pageRequests);

            _pageRequests.Distinct()
                .SubscribeOn(subscribeScheduler)
                .ObserveOn(subscribeScheduler)
                .Subscribe(pageKey =>
                {
                    int offset = pageKey * _pageSize;
                    T[] page;
                    if (_preloadingTasks.ContainsKey(pageKey))
                    {
                        _preloadingTasks[pageKey].Wait();
                        if (_preloadingTasks[pageKey].IsFaulted || _preloadingTasks[pageKey].IsCanceled)
                        {
                            page = _pageFetcher.PageFetch(pageKey * _pageSize, _pageSize);
                        }
                        else
                        {
                            page = _pageStore[pageKey];
                        }
                        _preloadingTasks.Remove(pageKey);
                    }
                    else
                        page = pageFetcher.PageFetch(offset, _pageSize);
                    _pageStore[pageKey] = page;
                    if (_deferredRequests.ContainsKey(pageKey))
                    {
                        var disposable = _deferredRequests[pageKey].Distinct()
                            .SubscribeOn(subscribeScheduler)
                            .ObserveOn(subscribeScheduler)
                            .Subscribe(tuple =>
                            {
                                onCollectionChangedReplace.OnNext(
                                    (page[tuple.Item1], tuple.Item2, pageKey * _pageSize + tuple.Item1));
                            }, () => _deferredRequests.Remove(pageKey));
                        _compositeDisposable.Add(disposable);
                    }
                });
        }

        public T Fetch(int index)
        {
            void preloadingPages(int pk)
            {
                int nextPageKey = pk + 1;
                if (!_pageStore.ContainsKey(nextPageKey) && !_preloadingTasks.ContainsKey(nextPageKey))
                {
                    _preloadingTasks[nextPageKey] = Task.Run(() =>
                    {
                        _pageStore[nextPageKey] = _pageFetcher.PageFetch(nextPageKey * _pageSize, _pageSize);
                    });
                }

                int previousPageKey = pk - 1;
                if (previousPageKey >= 0 && !_pageStore.ContainsKey(previousPageKey) && !_preloadingTasks.ContainsKey(previousPageKey))
                {
                    _preloadingTasks[previousPageKey] = Task.Run(() =>
                    {
                        _pageStore[previousPageKey] = _pageFetcher.PageFetch(previousPageKey * _pageSize, _pageSize);
                    });
                }
            }
            int pageKey = index / _pageSize;
            int pageIndex = index % _pageSize;

            if (_pageStore.ContainsKey(pageKey))
            {
                if (_deferredRequests.ContainsKey(pageKey))
                    _deferredRequests[pageKey].OnCompleted();

                preloadingPages(pageKey);

                return _pageStore[pageKey][pageIndex];
            }

            preloadingPages(pageKey);

            var placeholder = _placeholderFactory.CreatePlaceholder();

            if (!_deferredRequests.ContainsKey(pageKey))
                _deferredRequests[pageKey] = new ReplaySubject<(int, T)>();
            _deferredRequests[pageKey].OnNext((pageIndex, placeholder));

            _pageRequests.OnNext(pageKey);

            return placeholder;
        }

        public IObservable<(T, T, int)> OnCollectionChangedReplace { get; }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
            foreach (var disposable in _pageStore.SelectMany(ps => ps.Value).OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            foreach (var subject in _deferredRequests.Values)
            {
                subject.Dispose();
            }
        }
    }
}