using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in async way, which means that it doesn't block the current thread and in case the element isn't available a placeholder is provided.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    internal interface IHoardingAsyncPageStore<T> : IAsyncPageStore<T>
    {
    }
    
    internal class HoardingAsyncPageStore<T> : AsyncPageStoreBase<T>, IHoardingAsyncPageStore<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderOptional<TItem>
        {
            IBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingAsyncPageStore<TItem> Build();
        }

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> With(
                IBasicAsyncDataAccess<TItem> dataAccess, 
                IScheduler subscribeScheduler,
                Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory);
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IBasicAsyncDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;
            private IScheduler _subscribeScheduler;

            private Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>>
                _pageReplacementStrategyFactory;

            public IBuilderOptional<TItem> With(
                IBasicAsyncDataAccess<TItem> dataAccess, 
                IScheduler subscribeScheduler,
                Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
            {
                _dataAccess = dataAccess;
                _subscribeScheduler = subscribeScheduler;
                _pageReplacementStrategyFactory = pageReplacementStrategyFactory;
                return this;
            }

            public IBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = pageSize;
                return this;
            }

            public IHoardingAsyncPageStore<TItem> Build()
            {
                return new HoardingAsyncPageStore<TItem>(_dataAccess, _dataAccess, _subscribeScheduler, _pageReplacementStrategyFactory)
                {
                    PageSize = _pageSize
                };
            }
        }

        private readonly Subject<int> _pageRequests = new Subject<int>();

        private HoardingAsyncPageStore(
            IPageFetcher<T> pageFetcher,
            IPlaceholderFactory<T> placeholderFactory,
            IScheduler subscribeScheduler,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory) 
            : base(placeholderFactory, subscribeScheduler, pageReplacementStrategyFactory)
        {
            CompositeDisposable.Add(_pageRequests);

            _pageRequests
                .ObserveOn(subscribeScheduler)
                .Distinct()
                .Subscribe(pageKey =>
                {
                    var offset = pageKey * PageSize;
                    var actualPageSize = Math.Min(PageSize, Count - offset);
                    T[] page = pageFetcher.PageFetch(offset, actualPageSize);
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

        protected override void OnPageContained(int pageKey)
        {
            if (DeferredRequests.ContainsKey(pageKey))
                DeferredRequests[pageKey].OnCompleted();
        }

        protected override T OnPageNotContained(int pageKey, int pageIndex)
        {
            if (!DeferredRequests.ContainsKey(pageKey))
                DeferredRequests[pageKey] = new ReplaySubject<(int, T)>();
            DeferredRequests[pageKey].OnNext((pageIndex, Placeholder));

            _pageRequests.OnNext(pageKey);

            return Placeholder;
        }
    }
}