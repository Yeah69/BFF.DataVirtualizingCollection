using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageRemoval;
using BFF.DataVirtualizingCollection.PageStorage;
using BFF.DataVirtualizingCollection.Utilities;

namespace BFF.DataVirtualizingCollection
{
    internal enum PageLoadingBehavior
    {
        Preloading,
        NonPreloading
    }

    internal enum FetchersKind
    {
        NonTaskBased,
        TaskBased
    }

    internal enum IndexAccessBehavior
    {
        Asynchronous,
        Synchronous
    }

    public abstract class DataVirtualizingCollectionBuilderBase<TItem, TVirtualizationKind> :
        IPageLoadingBehaviorCollectionBuilder<TItem, TVirtualizationKind>,
        IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind>,
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind>,
        IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        protected const string UninitializedElementsExceptionMessage =
            "The builder used an uninitialized element. This should be impossible. Please open an issue on https://github.com/Yeah69/BFF.DataVirtualizingCollection.";

        protected const int DefaultPageSize = 100;
        protected readonly IScheduler NotificationScheduler;

        private readonly int _pageSize;

        private FetchersKind _fetchersKind =
            FetchersKind.NonTaskBased;

        private IndexAccessBehavior _indexAccessBehavior =
            IndexAccessBehavior.Synchronous;

        private Func<int, int, TItem[]>? _pageFetcher;
        private Func<int, int, TItem>? _placeholderFactory;
        private Func<int, int, TItem>? _preloadingPlaceholderFactory;
        private IScheduler _preloadingBackgroundScheduler;
        private IScheduler _pageBackgroundScheduler;
        protected IScheduler CountBackgroundScheduler;
        private Func<int, int, Task<TItem[]>>? _taskBasedPageFetcher;
        protected Func<int>? CountFetcher;

        private Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> _pageHoldingBehavior =
            HoardingPageNonRemoval.Create();

        private PageLoadingBehavior _pageLoadingBehavior =
            PageLoadingBehavior.NonPreloading;

        protected Func<Task<int>>? TaskBasedCountFetcher;


        protected DataVirtualizingCollectionBuilderBase(
            int pageSize, 
            IScheduler notificationScheduler)
            : this(
                pageSize, 
                notificationScheduler, 
                TaskPoolScheduler.Default)
        {
        }

        protected DataVirtualizingCollectionBuilderBase(
            int pageSize, 
            IScheduler notificationScheduler,
            IScheduler backgroundScheduler)
        {
            _pageSize = pageSize;
            NotificationScheduler = notificationScheduler;
            _preloadingBackgroundScheduler = backgroundScheduler;
            _pageBackgroundScheduler = backgroundScheduler;
            CountBackgroundScheduler = backgroundScheduler;
        }

        /// <inheritdoc />
        public IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonTaskBasedFetchers(
            Func<int, int, TItem[]> pageFetcher, 
            Func<int> countFetcher)
        {
            _fetchersKind = FetchersKind.NonTaskBased;
            _pageFetcher = pageFetcher;
            CountFetcher = countFetcher;
            return this;
        }

        /// <inheritdoc />
        public IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> TaskBasedFetchers(
            Func<int, int, Task<TItem[]>> pageFetcher,
            Func<Task<int>> countFetcher)
        {
            _fetchersKind = FetchersKind.TaskBased;
            _taskBasedPageFetcher = pageFetcher;
            TaskBasedCountFetcher = countFetcher;
            return this;
        }

        /// <inheritdoc />
        public TVirtualizationKind SyncIndexAccess()
        {
            _indexAccessBehavior = IndexAccessBehavior.Synchronous;
            return GenerateCollection();
        }

        /// <inheritdoc />
        public TVirtualizationKind AsyncIndexAccess(
            Func<int, int, TItem> placeholderFactory)
        {
            _indexAccessBehavior = IndexAccessBehavior.Asynchronous;
            _placeholderFactory = placeholderFactory;
            return GenerateCollection();
        }

        public TVirtualizationKind AsyncIndexAccess(
            Func<int, int, TItem> placeholderFactory,
            IScheduler pageBackgroundScheduler)
        {
            _pageBackgroundScheduler = pageBackgroundScheduler;
            return AsyncIndexAccess(placeholderFactory);
        }

        public TVirtualizationKind AsyncIndexAccess(
            Func<int, int, TItem> placeholderFactory,
            IScheduler pageBackgroundScheduler,
            IScheduler countBackgroundScheduler)
        {
            _pageBackgroundScheduler = pageBackgroundScheduler;
            CountBackgroundScheduler = countBackgroundScheduler;
            return AsyncIndexAccess(placeholderFactory);
        }

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> Hoarding()
        {
            return CustomPageRemovalStrategy(HoardingPageNonRemoval.Create());
        }

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> LeastRecentlyUsed(
            int pageLimit)
        {
            return LeastRecentlyUsed(pageLimit, 1);
        }

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> LeastRecentlyUsed(
            int pageLimit,
            int removalCount)
        {
            return CustomPageRemovalStrategy(LeastRecentlyUsedPageRemoval.Create(
                pageLimit,
                removalCount,
                _pageLoadingBehavior == PageLoadingBehavior.Preloading,
                new DateTimeTimestampProvider()));
        }

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> CustomPageRemovalStrategy(
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>>
                pageReplacementStrategyFactory)
        {
            _pageHoldingBehavior = pageReplacementStrategyFactory;
            return this;
        }

        /// <inheritdoc />
        public IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonPreloading()
        {
            _pageLoadingBehavior = PageLoadingBehavior.NonPreloading;
            return this;
        }

        /// <inheritdoc />
        public IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> Preloading(
            Func<int, int, TItem> preloadingPlaceholderFactory)
        {
            _pageLoadingBehavior = PageLoadingBehavior.Preloading;
            _preloadingPlaceholderFactory = preloadingPlaceholderFactory;
            return this;
        }

        /// <inheritdoc />
        public IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> Preloading(
            Func<int, int, TItem> preloadingPlaceholderFactory, 
            IScheduler preloadingBackgroundScheduler)
        {
            _preloadingBackgroundScheduler = preloadingBackgroundScheduler;
            return Preloading(preloadingPlaceholderFactory);
        }

        private TVirtualizationKind GenerateCollection()
        {
            var pageFetchEvents = new Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)>();

            return (_indexAccessBehavior, _fetchersKind) switch
            {
                (IndexAccessBehavior.Synchronous, FetchersKind.NonTaskBased) =>
                    GenerateNonTaskBasedSynchronousCollection(pageFetchEvents),
                (IndexAccessBehavior.Asynchronous, FetchersKind.NonTaskBased) =>
                    GenerateNonTaskBasedAsynchronousCollection(pageFetchEvents),
                (IndexAccessBehavior.Asynchronous, FetchersKind.TaskBased) => 
                    GenerateTaskBasedAsynchronousCollection(pageFetchEvents),
                _ => throw new ArgumentException("Can't build data-virtualizing collection with given input.")
            };
        }

        internal IPageStorage<TItem> PlaceholderOnlyPageStoreFactory(int count)
        {
            var placeholderFactory = _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
            return new PlaceholderOnlyPageStorage<TItem>(
                _pageSize,
                count,
                placeholderFactory,
                CurrentThreadScheduler.Instance);
        }

        private IPageStorage<TItem> PageStoreFactory(
            int count,
            Func<int, int, int, IPage<TItem>> nonPreloadingPageFetcherFactory,
            Func<int, int, int, IPage<TItem>> preloadingPageFetcherFactory)
        {
            return _pageLoadingBehavior == PageLoadingBehavior.Preloading
                ? new PreloadingPageStorage<TItem>(
                    _pageSize,
                    count,
                    nonPreloadingPageFetcherFactory,
                    preloadingPageFetcherFactory,
                    _pageHoldingBehavior)
                : new PageStorage<TItem>(
                    _pageSize,
                    count,
                    nonPreloadingPageFetcherFactory,
                    _pageHoldingBehavior);
        }

        internal Func<int, IPageStorage<TItem>> GenerateTaskBasedAsynchronousPageStorage(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return PageStoreFactoryComposition;

            IPage<TItem> NonPreloadingPageFetcherFactory(
                int pageKey,
                int offset,
                int pageSize)
            {
                var taskBasedPageFetcher = _taskBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var placeholderFactory = _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    taskBasedPageFetcher,
                    placeholderFactory,
                    _pageBackgroundScheduler,
                    pageFetchEvents.AsObserver());
            }

            IPage<TItem> PreloadingPageFetcherFactory(
                int pageKey,
                int offset,
                int pageSize)
            {
                var taskBasedPageFetcher = _taskBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var preloadingPlaceholderFactory = _preloadingPlaceholderFactory ??
                                                   throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    taskBasedPageFetcher,
                    preloadingPlaceholderFactory,
                    _preloadingBackgroundScheduler,
                    pageFetchEvents.AsObserver());
            }

            IPageStorage<TItem> PageStoreFactoryComposition(int count)
            {
                return PageStoreFactory(count, NonPreloadingPageFetcherFactory, PreloadingPageFetcherFactory);
            }
        }

        internal Func<int, IPageStorage<TItem>> GenerateNonTaskBasedAsynchronousPageStorage(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return PageStoreFactoryComposition;

            IPage<TItem> NonPreloadingPageFetcherFactory(
                int pageKey,
                int offset,
                int pageSize)
            {
                var pageFetcher = _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var placeholderFactory = _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncNonTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    pageFetcher,
                    placeholderFactory,
                    _pageBackgroundScheduler,
                    pageFetchEvents.AsObserver());
            }

            IPage<TItem> PreloadingPageFetcherFactory(
                int pageKey,
                int offset,
                int pageSize)
            {
                var pageFetcher = _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var preloadingPlaceholderFactory = _preloadingPlaceholderFactory ??
                                                   throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncNonTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    pageFetcher,
                    preloadingPlaceholderFactory,
                    _preloadingBackgroundScheduler,
                    pageFetchEvents.AsObserver());
            }

            IPageStorage<TItem> PageStoreFactoryComposition(int count)
            {
                return PageStoreFactory(count, NonPreloadingPageFetcherFactory, PreloadingPageFetcherFactory);
            }
        }

        internal Func<int, IPageStorage<TItem>> GenerateNonTaskBasedSynchronousPageStorage(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return PageStoreFactoryComposition;

            IPage<TItem> NonPreloadingPageFetcherFactory(int pageKey, int offset, int pageSize)
            {
                var pageFetcher =
                    _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new SyncNonPreloadingNonTaskBasedPage<TItem>(
                    offset,
                    pageSize,
                    pageFetcher);
            }

            IPage<TItem> PreloadingPageFetcherFactory(int pageKey, int offset, int pageSize)
            {
                var pageFetcher =
                    _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var preloadingPlaceholderFactory = 
                    _preloadingPlaceholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncNonTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    pageFetcher,
                    preloadingPlaceholderFactory,
                    _preloadingBackgroundScheduler,
                    pageFetchEvents.AsObserver());
            }

            IPageStorage<TItem> PageStoreFactoryComposition(int count)
            {
                return PageStoreFactory(count, NonPreloadingPageFetcherFactory, PreloadingPageFetcherFactory);
            }
        }

        protected abstract TVirtualizationKind GenerateTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents);

        protected abstract TVirtualizationKind GenerateNonTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents);

        protected abstract TVirtualizationKind GenerateNonTaskBasedSynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents);
    }
}