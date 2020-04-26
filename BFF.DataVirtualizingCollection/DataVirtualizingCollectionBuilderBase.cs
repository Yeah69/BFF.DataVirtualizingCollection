using System;
using System.Collections.Generic;
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

        protected readonly int PageSize;

        protected Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> PageHoldingBehavior =
            HoardingPageNonRemoval.Create();

        internal PageLoadingBehavior PageLoadingBehavior =
            PageLoadingBehavior.NonPreloading;

        private FetchersKind _fetchersKind =
            FetchersKind.NonTaskBased;

        private IndexAccessBehavior _indexAccessBehavior =
            IndexAccessBehavior.Synchronous;

        protected Func<int, int, TItem[]>? PageFetcher;
        protected Func<int>? CountFetcher;
        protected Func<int, int, Task<TItem[]>>? TaskBasedPageFetcher;
        protected Func<Task<int>>? TaskBasedCountFetcher;
        protected Func<int, int, TItem>? PlaceholderFactory;
        protected Func<int, int, TItem>? PreloadingPlaceholderFactory;
        protected readonly IScheduler BackgroundScheduler;
        protected IScheduler PreloadingScheduler;
        protected readonly IScheduler NotificationScheduler;

        protected DataVirtualizingCollectionBuilderBase(int pageSize, IScheduler notificationScheduler)
            : this (pageSize, notificationScheduler, TaskPoolScheduler.Default)
        {
        }

        protected DataVirtualizingCollectionBuilderBase(int pageSize, IScheduler notificationScheduler, IScheduler backgroundScheduler) 
        {
            PageSize = pageSize;
            NotificationScheduler = notificationScheduler;
            BackgroundScheduler = backgroundScheduler;
            PreloadingScheduler = BackgroundScheduler;
        }

        /// <inheritdoc />
        public IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonPreloading()
        {
            PageLoadingBehavior = PageLoadingBehavior.NonPreloading;
            return this;
        }

        /// <inheritdoc />
        public IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> Preloading(Func<int, int, TItem> preloadingPlaceholderFactory)
        {
            PageLoadingBehavior = PageLoadingBehavior.Preloading;
            PreloadingScheduler = TaskPoolScheduler.Default;
            PreloadingPlaceholderFactory = preloadingPlaceholderFactory;
            return this;
        }

        /// <inheritdoc />
        public IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> Preloading(Func<int, int, TItem> preloadingPlaceholderFactory, IScheduler scheduler)
        {
            PageLoadingBehavior = PageLoadingBehavior.Preloading;
            PreloadingScheduler = scheduler;
            return this;
        }

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> Hoarding() => CustomPageRemovalStrategy(HoardingPageNonRemoval.Create());

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> LeastRecentlyUsed(int pageLimit) => LeastRecentlyUsed(pageLimit, 1);

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> LeastRecentlyUsed(int pageLimit, int removalCount) =>
            CustomPageRemovalStrategy(LeastRecentlyUsedPageRemoval.Create(
                pageLimit,
                removalCount, 
                PageLoadingBehavior == PageLoadingBehavior.Preloading, 
                new DateTimeTimestampProvider()));

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> CustomPageRemovalStrategy(Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
        {
            PageHoldingBehavior = pageReplacementStrategyFactory;
            return this;
        }

        /// <inheritdoc />
        public IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonTaskBasedFetchers(Func<int, int, TItem[]> pageFetcher, Func<int> countFetcher)
        {
            _fetchersKind = FetchersKind.NonTaskBased;
            PageFetcher = pageFetcher;
            CountFetcher = countFetcher;
            return this;
        }

        /// <inheritdoc />
        public IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> TaskBasedFetchers(Func<int, int, Task<TItem[]>> pageFetcher, Func<Task<int>> countFetcher)
        {
            _fetchersKind = FetchersKind.TaskBased;
            TaskBasedPageFetcher = pageFetcher;
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
        public TVirtualizationKind AsyncIndexAccess(Func<int, int, TItem> placeholderFactory)
        {
            _indexAccessBehavior = IndexAccessBehavior.Asynchronous;
            PlaceholderFactory = placeholderFactory;
            return GenerateCollection();
        }

        private TVirtualizationKind GenerateCollection()
        {
            var pageFetchEvents = new Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)>();
            
            switch (_indexAccessBehavior, _fetchersKind)
            {
                case (IndexAccessBehavior.Synchronous, FetchersKind.NonTaskBased):
                {
                    return GenerateNonTaskBasedSynchronousCollection(pageFetchEvents);
                }
                case (IndexAccessBehavior.Asynchronous, FetchersKind.NonTaskBased):
                {
                    return GenerateNonTaskBasedAsynchronousCollection(pageFetchEvents);
                }
                case (IndexAccessBehavior.Asynchronous, FetchersKind.TaskBased):
                {
                    return GenerateTaskBasedAsynchronousCollection(pageFetchEvents);
                }
                default: throw new ArgumentException("Can't build data-virtualizing collection with given input.");
            }
        }

        protected abstract TVirtualizationKind GenerateTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents);

        protected abstract TVirtualizationKind GenerateNonTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents);

        protected abstract TVirtualizationKind GenerateNonTaskBasedSynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents);

        internal IPageStorage<TItem> PlaceholderOnlyPageStoreFactory(int count) => 
            new PlaceholderOnlyPageStorage<TItem>(
                PageSize, 
                count, 
                PlaceholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                BackgroundScheduler);

        internal IPageStorage<TItem> PageStoreFactory(
            int count, 
            Func<int, int, int, IPage<TItem>> nonPreloadingPageFetcherFactory, 
            Func<int, int, int, IPage<TItem>> preloadingPageFetcherFactory) =>
            PageLoadingBehavior == PageLoadingBehavior.Preloading
                ? new PreloadingPageStorage<TItem>(
                    PageSize,
                    count,
                    nonPreloadingPageFetcherFactory,
                    preloadingPageFetcherFactory,
                    PageHoldingBehavior)
                : new PageStorage<TItem>(
                    PageSize,
                    count,
                    nonPreloadingPageFetcherFactory,
                    PageHoldingBehavior);
    }
}