using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
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
        TaskBased,
        AsyncEnumerableBased
    }

    internal enum IndexAccessBehavior
    {
        Asynchronous,
        Synchronous
    }

    internal static class DataVirtualizingCollectionBuilderBase
    {
        internal const int DefaultPageSize = 100;
    }

    internal abstract class DataVirtualizingCollectionBuilderBase<TItem, TVirtualizationKind> :
        IPageLoadingBehaviorCollectionBuilder<TItem, TVirtualizationKind>,
        IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind>,
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind>,
        IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        protected const string UninitializedElementsExceptionMessage =
            "The builder used an uninitialized element. This should be impossible. Please open an issue on https://github.com/Yeah69/BFF.DataVirtualizingCollection.";

        protected readonly IScheduler NotificationScheduler;

        private readonly int _pageSize;

        private FetchersKind _fetchersKind =
            FetchersKind.NonTaskBased;

        private IndexAccessBehavior _indexAccessBehavior =
            IndexAccessBehavior.Synchronous;

        private Func<int, int, CancellationToken, TItem[]>? _pageFetcher;
        private Func<int, int, TItem>? _placeholderFactory;
        private Func<int, int, TItem>? _preloadingPlaceholderFactory;
        private IScheduler _preloadingBackgroundScheduler;
        private IScheduler _pageBackgroundScheduler;
        protected IScheduler CountBackgroundScheduler;
        private Func<int, int, CancellationToken, Task<TItem[]>>? _taskBasedPageFetcher;
        private Func<int, int, CancellationToken, IAsyncEnumerable<TItem>>? _asyncEnumerableBasedPageFetcher;
        protected Func<CancellationToken, int>? CountFetcher;

        private Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> _pageHoldingBehavior =
            HoardingPageNonRemoval.Create();

        private PageLoadingBehavior _pageLoadingBehavior =
            PageLoadingBehavior.NonPreloading;

        protected Func<CancellationToken, Task<int>>? TaskBasedCountFetcher;


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

        public IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonTaskBasedFetchers(
            Func<int, int, TItem[]> pageFetcher, 
            Func<int> countFetcher)
        {
            _fetchersKind = FetchersKind.NonTaskBased;
            _pageFetcher = (offset, size, _) => pageFetcher(offset, size);
            CountFetcher = _ => countFetcher();
            return this;
        }

        public IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> TaskBasedFetchers(
            Func<int, int, Task<TItem[]>> pageFetcher,
            Func<Task<int>> countFetcher)
        {
            _fetchersKind = FetchersKind.TaskBased;
            _taskBasedPageFetcher = (offset, size, _) => pageFetcher(offset, size);
            TaskBasedCountFetcher = _ => countFetcher();
            return this;
        }

        public IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonTaskBasedFetchers(Func<int, int, CancellationToken, TItem[]> pageFetcher, Func<CancellationToken, int> countFetcher)
        {
            _fetchersKind = FetchersKind.NonTaskBased;
            _pageFetcher = pageFetcher;
            CountFetcher = countFetcher;
            return this;
        }

        public IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> TaskBasedFetchers(Func<int, int, CancellationToken, Task<TItem[]>> pageFetcher, Func<CancellationToken, Task<int>> countFetcher)
        {
            _fetchersKind = FetchersKind.TaskBased;
            _taskBasedPageFetcher = pageFetcher;
            TaskBasedCountFetcher = countFetcher;
            return this;
        }

        public IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> AsyncEnumerableBasedFetchers(Func<int, int, CancellationToken, IAsyncEnumerable<TItem>> pageFetcher, Func<CancellationToken, Task<int>> countFetcher)
        {
            _fetchersKind = FetchersKind.AsyncEnumerableBased;
            _asyncEnumerableBasedPageFetcher = pageFetcher;
            TaskBasedCountFetcher = countFetcher;
            return this;
        }

        public TVirtualizationKind SyncIndexAccess()
        {
            _indexAccessBehavior = IndexAccessBehavior.Synchronous;
            return GenerateCollection();
        }

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

        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> Hoarding()
        {
            return CustomPageRemovalStrategy(HoardingPageNonRemoval.Create());
        }

        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> LeastRecentlyUsed(
            int pageLimit)
        {
            return LeastRecentlyUsed(pageLimit, 1);
        }

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

        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> CustomPageRemovalStrategy(
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>>
                pageReplacementStrategyFactory)
        {
            _pageHoldingBehavior = pageReplacementStrategyFactory;
            return this;
        }

        public IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonPreloading()
        {
            _pageLoadingBehavior = PageLoadingBehavior.NonPreloading;
            return this;
        }

        public IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> Preloading(
            Func<int, int, TItem> preloadingPlaceholderFactory)
        {
            _pageLoadingBehavior = PageLoadingBehavior.Preloading;
            _preloadingPlaceholderFactory = preloadingPlaceholderFactory;
            return this;
        }

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
                (IndexAccessBehavior.Asynchronous, FetchersKind.AsyncEnumerableBased) => 
                    GenerateAsyncEnumerableBasedAsynchronousCollection(pageFetchEvents),
                _ => throw new ArgumentException("Can't build data-virtualizing collection with given input.")
            };
        }

        private IPageStorage<TItem> PageStoreFactory(
            int count,
            Func<int, int, int, IDisposable, IPage<TItem>> nonPreloadingPageFetcherFactory,
            Func<int, int, int, IDisposable, IPage<TItem>> preloadingPageFetcherFactory)
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
                int pageSize,
                IDisposable onDisposalAfterFetchCompleted)
            {
                var taskBasedPageFetcher = _taskBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var placeholderFactory = _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    onDisposalAfterFetchCompleted,
                    taskBasedPageFetcher,
                    placeholderFactory,
                    _pageBackgroundScheduler,
                    pageFetchEvents.AsObserver());
            }

            IPage<TItem> PreloadingPageFetcherFactory(
                int pageKey,
                int offset,
                int pageSize,
                IDisposable onDisposalAfterFetchCompleted)
            {
                var taskBasedPageFetcher = _taskBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var preloadingPlaceholderFactory = _preloadingPlaceholderFactory ??
                                                   throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    onDisposalAfterFetchCompleted,
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

        internal Func<int, IPageStorage<TItem>> GenerateAsyncEnumerableBasedAsynchronousPageStorage(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return PageStoreFactoryComposition;

            IPage<TItem> NonPreloadingPageFetcherFactory(
                int pageKey,
                int offset,
                int pageSize,
                IDisposable onDisposalAfterFetchCompleted)
            {
                var asyncEnumerableBasedPageFetcher = _asyncEnumerableBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var placeholderFactory = _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncEnumerableBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    onDisposalAfterFetchCompleted,
                    asyncEnumerableBasedPageFetcher,
                    placeholderFactory,
                    _pageBackgroundScheduler,
                    pageFetchEvents.AsObserver());
            }

            IPage<TItem> PreloadingPageFetcherFactory(
                int pageKey,
                int offset,
                int pageSize,
                IDisposable onDisposalAfterFetchCompleted)
            {
                var asyncEnumerableBasedPageFetcher = _asyncEnumerableBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var preloadingPlaceholderFactory = _preloadingPlaceholderFactory ??
                                                   throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncEnumerableBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    onDisposalAfterFetchCompleted,
                    asyncEnumerableBasedPageFetcher,
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
                int pageSize,
                IDisposable onDisposalAfterFetchCompleted)
            {
                var pageFetcher = _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var placeholderFactory = _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncNonTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    onDisposalAfterFetchCompleted,
                    pageFetcher,
                    placeholderFactory,
                    _pageBackgroundScheduler,
                    pageFetchEvents.AsObserver());
            }

            IPage<TItem> PreloadingPageFetcherFactory(
                int pageKey,
                int offset,
                int pageSize,
                IDisposable onDisposalAfterFetchCompleted)
            {
                var pageFetcher = _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var preloadingPlaceholderFactory = _preloadingPlaceholderFactory ??
                                                   throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncNonTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    onDisposalAfterFetchCompleted,
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

            IPage<TItem> NonPreloadingPageFetcherFactory(
                int pageKey,
                int offset,
                int pageSize,
                IDisposable onDisposalAfterFetchCompleted)
            {
                var pageFetcher =
                    _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new SyncNonPreloadingNonTaskBasedPage<TItem>(
                    offset,
                    pageSize,
                    onDisposalAfterFetchCompleted,
                    pageFetcher);
            }

            IPage<TItem> PreloadingPageFetcherFactory(
                int pageKey, 
                int offset,
                int pageSize,
                IDisposable onDisposalAfterFetchCompleted)
            {
                var pageFetcher =
                    _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                var preloadingPlaceholderFactory = 
                    _preloadingPlaceholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
                return new AsyncNonTaskBasedPage<TItem>(
                    pageKey,
                    offset,
                    pageSize,
                    onDisposalAfterFetchCompleted,
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

        protected abstract TVirtualizationKind GenerateAsyncEnumerableBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents);

        protected abstract TVirtualizationKind GenerateNonTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents);

        protected abstract TVirtualizationKind GenerateNonTaskBasedSynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents);
    }
}