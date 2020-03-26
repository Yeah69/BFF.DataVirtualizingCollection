using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageRemoval;
using BFF.DataVirtualizingCollection.PageStorage;
using BFF.DataVirtualizingCollection.Utilities;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    /// <summary>
    /// Lets you configure the page loading behavior.
    /// Here you can turn the preloading on or off. Preloading means that neighboring pages from requested pages are loaded as well, assuming that they'll be requested soon. 
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public interface IPageLoadingBehaviorCollectionBuilder<T>
    {
        /// <summary>
        /// Pages are loaded only as soon as an item of the page is requested.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IPageHoldingBehaviorCollectionBuilder<T> NonPreloading();

        /// <summary>
        /// Pages are loaded as soon as an item of the page is requested or as soon as a neighboring page is loaded.
        /// Per default the TaskPoolScheduler is taken for the preloads.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IPageHoldingBehaviorCollectionBuilder<T> Preloading();

        /// <summary>
        /// Pages are loaded as soon as an item of the page is requested or as soon as a neighboring page is loaded.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IPageHoldingBehaviorCollectionBuilder<T> Preloading(IScheduler scheduler);
    }

    /// <summary>
    /// Lets you configure the page holding behavior.
    /// At the moment only one strategy (hoarding) is available.
    /// As further strategy get implement they will appear here as a choice.
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public interface IPageHoldingBehaviorCollectionBuilder<T>
    {
        /// <summary>
        /// In this mode pages are loaded on demand. However, once loaded the pages are hold in memory until the data virtualizing collection is disposed or garbage collected.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IFetchersKindCollectionBuilder<T> Hoarding();

        /// <summary>
        /// If the page limit is reached then the page which is least recently used will be chosen for removal.
        /// </summary>
        /// <param name="pageLimit">Has to be greater than zero (with preloading greater than two) in order to maintain at least one page in the page store (when preloading is active, then the neighbors of the most recently requested page are maintained as well).</param>
        /// <returns>The builder itself.</returns>
        IFetchersKindCollectionBuilder<T> LeastRecentlyUsed(int pageLimit);

        /// <summary>
        /// If the page limit is reached then the pages (amount: removal buffer plus one) which are least recently used will be chosen for removal.
        /// </summary>
        /// <param name="pageLimit">Has to be greater than zero (with preloading greater than two) in order to maintain at least one page in the page store (when preloading is active, then the neighbors of the most recently requested page are maintained as well).</param>
        /// <param name="removalCount">Has to be in between one and the page limit minus one (so at least one page remains).
        /// With active preloading the removal count cannot be greater than the page limit minus three.</param>
        /// <returns>The builder itself.</returns>
        IFetchersKindCollectionBuilder<T> LeastRecentlyUsed(int pageLimit, int removalCount);

        /// <summary>
        /// With this function you can provide an own page-removal strategy.
        /// You'll get an observable which emits all element requests in form of a key to the page and the element's index inside of the page.
        /// You'll have to return an observable which emits page-removal requests. You can request to remove several pages at once.
        /// </summary>
        /// <param name="pageReplacementStrategyFactory"></param>
        /// <returns>The builder itself.</returns>
        IFetchersKindCollectionBuilder<T> CustomPageRemovalStrategy(
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>>
                pageReplacementStrategyFactory);
    }

    /// <summary>
    /// Lets you configure the fetcher (page and count) kind and lets you also provide appropriate fetchers as well.
    /// The page fetcher gets a page based on the provided offset and size. The count fetcher gets the count of the data virtualizing collection.
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public interface IFetchersKindCollectionBuilder<T>
    {
        /// <summary>
        /// You have to provide non-task-based (synchronous) fetchers.
        /// The page fetcher gets a page based on the provided offset and size. The count fetcher gets the count of the data virtualizing collection.
        /// </summary>
        /// <param name="pageFetcher">First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page.</param>
        /// <param name="countFetcher">You have to provide a lambda function which gets the count of all elements in the data virtualized collection.</param>
        /// <returns>The builder itself.</returns>
        IIndexAccessBehaviorCollectionBuilder<T> NonTaskBasedFetchers(Func<int, int, T[]> pageFetcher, Func<int> countFetcher);

        /// <summary>
        /// You have to provide task-based (asynchronous) fetchers.
        /// The page fetcher gets a page based on the provided offset and size. The count fetcher gets the count of the data virtualizing collection.
        /// </summary>
        /// <param name="pageFetcher">First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page.</param>
        /// <param name="countFetcher">You have to provide a lambda function which gets the count of all elements in the data virtualized collection.</param>
        /// <returns>The builder itself.</returns>
        IIndexAccessBehaviorCollectionBuilder<T> TaskBasedFetchers(Func<int, int, Task<T[]>> pageFetcher, Func<Task<int>> countFetcher);
    }


    /// <summary>
    /// Lets you configure whether the index access should be synchronous or asynchronous.
    /// Synchronous means that if the index access will wait actively until the entry is provided even if the page still has to be loaded.
    /// Asynchronous meas the if the page still needs to be loaded a placeholder for the indexed access is provided, as soon as the page is loaded a notification is emitted which states that the entry of the index arrived.  
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public interface IIndexAccessBehaviorCollectionBuilder<T>
    {
        /// <summary>
        /// If item of requested index isn't loaded yet the collections will wait actively and return as soon as it arrives.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IDataVirtualizingCollection<T> SyncIndexAccess();

        /// <summary>
        /// If item of requested index isn't loaded yet the collections will return a placeholder instead and emit a notification as soon as it arrives.
        /// </summary>
        /// <param name="placeholderFactory">You have to provide a factory lambda function which returns a placeholder.</param>
        /// <param name="backgroundScheduler">Scheduler for all background operations.</param>
        /// <param name="notificationScheduler">Scheduler on which the notifications are emitted.</param>
        /// <returns></returns>
        [Obsolete("Please use the overload whose placeholderFactory gets the page key and index as parameters. Ignore these parameters if they are not necessary. This very overload will be removed in future releases.")]
        IDataVirtualizingCollection<T> AsyncIndexAccess(Func<T> placeholderFactory, IScheduler backgroundScheduler, IScheduler notificationScheduler);

        /// <summary>
        /// If item of requested index isn't loaded yet the collections will return a placeholder instead and emit a notification as soon as it arrives.
        /// </summary>
        /// <param name="placeholderFactory">You have to provide a factory lambda function which returns a placeholder.
        /// The first parameter is the page key (index of pages) and the second is the page index (index of items inside the page).</param>
        /// <param name="backgroundScheduler">Scheduler for all background operations.</param>
        /// <param name="notificationScheduler">Scheduler on which the notifications are emitted.</param>
        /// <returns></returns>
        IDataVirtualizingCollection<T> AsyncIndexAccess(Func<int, int, T> placeholderFactory, IScheduler backgroundScheduler, IScheduler notificationScheduler);
    }

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

    /// <summary>
    /// This class offers the function "Build" in order to build data virtualizing collections.
    /// The construction of such collections is encapsulated and externally only access-able via this class. 
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public class DataVirtualizingCollectionBuilder<T> 
        : IPageLoadingBehaviorCollectionBuilder<T>,
            IPageHoldingBehaviorCollectionBuilder<T>,
            IFetchersKindCollectionBuilder<T>,
            IIndexAccessBehaviorCollectionBuilder<T>
    {
        private static string UninitializedElementsExceptionMessage =
            "The builder used an uninitialized element. This should be impossible. Please open an issue on https://github.com/Yeah69/BFF.DataVirtualizingCollection.";

        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// Page size is set to the default value 100.
        /// </summary>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<T> Build() => Build(100);

        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <param name="pageSize">Maximum size of a single page.</param>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<T> Build(int pageSize) => 
            new DataVirtualizingCollectionBuilder<T>(pageSize);

        private readonly int _pageSize;

        private Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> _pageHoldingBehavior =
            HoardingPageNonRemoval.Create();

        private PageLoadingBehavior _pageLoadingBehavior =
            PageLoadingBehavior.NonPreloading;

        private FetchersKind _fetchersKind =
            FetchersKind.NonTaskBased;

        private IndexAccessBehavior _indexAccessBehavior =
            IndexAccessBehavior.Synchronous;

        private Func<int, int, T[]>? _pageFetcher;

        private Func<int>? _countFetcher;

        private Func<int, int, Task<T[]>>? _taskBasedPageFetcher;

        private Func<Task<int>>? _taskBasedCountFetcher;

        private Func<int, int, T>? _placeholderFactory;

        private IScheduler _backgroundScheduler = DefaultScheduler.Instance;

        private IScheduler _preloadingScheduler = DefaultScheduler.Instance;

        private IScheduler _notificationScheduler = DefaultScheduler.Instance;

        private DataVirtualizingCollectionBuilder(int pageSize = 100)
        {
            _pageSize = pageSize;
        }

        /// <inheritdoc />
        public IPageHoldingBehaviorCollectionBuilder<T> NonPreloading()
        {
            _pageLoadingBehavior = PageLoadingBehavior.NonPreloading;
            return this;
        }

        /// <inheritdoc />
        public IPageHoldingBehaviorCollectionBuilder<T> Preloading()
        {
            _pageLoadingBehavior = PageLoadingBehavior.Preloading;
            _preloadingScheduler = TaskPoolScheduler.Default;
            return this;
        }

        /// <inheritdoc />
        public IPageHoldingBehaviorCollectionBuilder<T> Preloading(IScheduler scheduler)
        {
            _pageLoadingBehavior = PageLoadingBehavior.Preloading;
            _preloadingScheduler = scheduler;
            return this;
        }

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<T> Hoarding() => CustomPageRemovalStrategy(HoardingPageNonRemoval.Create());

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<T> LeastRecentlyUsed(int pageLimit) => LeastRecentlyUsed(pageLimit, 1);

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<T> LeastRecentlyUsed(int pageLimit, int removalCount) =>
            CustomPageRemovalStrategy(LeastRecentlyUsedPageRemoval.Create(
                pageLimit,
                removalCount, 
                _pageLoadingBehavior == PageLoadingBehavior.Preloading, 
                new DateTimeTimestampProvider()));

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<T> CustomPageRemovalStrategy(Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
        {
            _pageHoldingBehavior = pageReplacementStrategyFactory;
            return this;
        }

        /// <inheritdoc />
        public IIndexAccessBehaviorCollectionBuilder<T> NonTaskBasedFetchers(Func<int, int, T[]> pageFetcher, Func<int> countFetcher)
        {
            _fetchersKind = FetchersKind.NonTaskBased;
            _pageFetcher = pageFetcher;
            _countFetcher = countFetcher;
            return this;
        }

        /// <inheritdoc />
        public IIndexAccessBehaviorCollectionBuilder<T> TaskBasedFetchers(Func<int, int, Task<T[]>> pageFetcher, Func<Task<int>> countFetcher)
        {
            _fetchersKind = FetchersKind.TaskBased;
            _taskBasedPageFetcher = pageFetcher;
            _taskBasedCountFetcher = countFetcher;
            return this;
        }

        /// <inheritdoc />
        public IDataVirtualizingCollection<T> SyncIndexAccess()
        {
            _indexAccessBehavior = IndexAccessBehavior.Synchronous;
            return GenerateCollection();
        }

        /// <inheritdoc />
        public IDataVirtualizingCollection<T> AsyncIndexAccess(Func<T> placeholderFactory, IScheduler backgroundScheduler, IScheduler notificationScheduler)
        {
            _indexAccessBehavior = IndexAccessBehavior.Asynchronous;
            _placeholderFactory = (_, __) => placeholderFactory();
            _backgroundScheduler = backgroundScheduler;
            _notificationScheduler = notificationScheduler;
            return GenerateCollection();
        }

        /// <inheritdoc />
        public IDataVirtualizingCollection<T> AsyncIndexAccess(Func<int, int, T> placeholderFactory, IScheduler backgroundScheduler, IScheduler notificationScheduler)
        {
            _indexAccessBehavior = IndexAccessBehavior.Asynchronous;
            _placeholderFactory = placeholderFactory;
            _backgroundScheduler = backgroundScheduler;
            _notificationScheduler = notificationScheduler;
            return GenerateCollection();
        }

        private IDataVirtualizingCollection<T> GenerateCollection()
        {
            switch (_indexAccessBehavior, _fetchersKind)
            {
                case (IndexAccessBehavior.Synchronous, FetchersKind.NonTaskBased):
                {
                    return new SyncDataVirtualizingCollection<T>(
                        PageStoreFactory, 
                        _countFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage));

                    IPage<T> NonPreloadingPageFetcherFactory(int pageKey, int offset, int pageSize) =>
                        new SyncNonPreloadingNonTaskBasedPage<T>(
                            offset,
                            pageSize,
                            _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage));
                    IPage<T> PreloadingPageFetcherFactory(int pageKey, int offset, int pageSize) =>
                        new SyncPreloadingNonTaskBasedPage<T>(
                            offset, 
                            pageSize, 
                            _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                            _preloadingScheduler);

                    IPageStorage<T> PageStoreFactory(int count) =>
                        _pageLoadingBehavior == PageLoadingBehavior.Preloading
                        ? new PreloadingPageStorage<T>(
                            _pageSize,
                            count,
                            NonPreloadingPageFetcherFactory,
                            PreloadingPageFetcherFactory,
                            _pageHoldingBehavior)
                        : new PageStorage<T>(
                            _pageSize,
                            count,
                            NonPreloadingPageFetcherFactory,
                            _pageHoldingBehavior);
                }
                case (IndexAccessBehavior.Synchronous, FetchersKind.TaskBased):
                {
                    return new SyncDataVirtualizingCollection<T>(
                        PageStoreFactory, 
                        () => _taskBasedCountFetcher?.Invoke().Result ?? throw new NullReferenceException(UninitializedElementsExceptionMessage));

                    IPage<T> NonPreloadingPageFetcherFactory(int pageKey, int offset, int pageSize) =>
                        new SyncNonPreloadingTaskBasedPage<T>(
                            offset,
                            pageSize, 
                            _taskBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage));
                    IPage<T> PreloadingPageFetcherFactory(int pageKey, int offset, int pageSize) =>
                        new SyncPreloadingTaskBasedPage<T>(
                            offset, 
                            pageSize, 
                            _taskBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage), 
                            _preloadingScheduler);

                    IPageStorage<T> PageStoreFactory(int count) =>
                        _pageLoadingBehavior == PageLoadingBehavior.Preloading
                            ? new PreloadingPageStorage<T>(
                                _pageSize,
                                count,
                                NonPreloadingPageFetcherFactory,
                                PreloadingPageFetcherFactory,
                                _pageHoldingBehavior)
                            : new PageStorage<T>(
                                _pageSize,
                                count,
                                NonPreloadingPageFetcherFactory,
                                _pageHoldingBehavior);
                    }
                case (IndexAccessBehavior.Asynchronous, FetchersKind.NonTaskBased):
                {
                    var pageFetchEvents = new Subject<(int Offset, int PageSize, T[] PreviousPage, T[] Page)>();

                    return new AsyncDataVirtualizingCollection<T>(
                        PageStoreFactory, 
                        () => Observable.Start(_countFetcher, _backgroundScheduler).ToTask(), 
                        pageFetchEvents.AsObservable(),
                        pageFetchEvents,
                        _notificationScheduler);

                    IPage<T> NonPreloadingPageFetcherFactory(
                        int pageKey,
                        int offset,
                        int pageSize) =>
                        new AsyncNonTaskBasedPage<T>(
                            pageKey,
                            offset,
                            pageSize,
                            _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                            _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                            _backgroundScheduler,
                            pageFetchEvents.AsObserver());

                    IPage<T> PreloadingPageFetcherFactory(
                        int pageKey,
                        int offset,
                        int pageSize) =>
                        new AsyncNonTaskBasedPage<T>(
                            pageKey,
                            offset,
                            pageSize,
                            _pageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                            _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                            _preloadingScheduler,
                            pageFetchEvents.AsObserver());

                        IPageStorage<T> PageStoreFactory(int count) =>
                            _pageLoadingBehavior == PageLoadingBehavior.Preloading
                                ? new PreloadingPageStorage<T>(
                                    _pageSize,
                                    count,
                                    NonPreloadingPageFetcherFactory,
                                    PreloadingPageFetcherFactory,
                                    _pageHoldingBehavior)
                                : new PageStorage<T>(
                                    _pageSize,
                                    count,
                                    NonPreloadingPageFetcherFactory,
                                    _pageHoldingBehavior);
                    }
                case (IndexAccessBehavior.Asynchronous, FetchersKind.TaskBased):
                {
                    var pageFetchEvents = new Subject<(int Offset, int PageSize, T[] PreviousPage, T[] Page)>();

                    return new AsyncDataVirtualizingCollection<T>(
                        PageStoreFactory,
                        _taskBasedCountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                        pageFetchEvents.AsObservable(),
                        pageFetchEvents,
                        _notificationScheduler);

                    IPage<T> NonPreloadingPageFetcherFactory(
                        int pageKey,
                        int offset,
                        int pageSize) =>
                        new AsyncTaskBasedPage<T>(
                            pageKey,
                            offset,
                            pageSize,
                            _taskBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                            _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                            _backgroundScheduler,
                            pageFetchEvents.AsObserver());

                    IPage<T> PreloadingPageFetcherFactory(
                        int pageKey,
                        int offset,
                        int pageSize) =>
                        new AsyncTaskBasedPage<T>(
                            pageKey,
                            offset,
                            pageSize,
                            _taskBasedPageFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                            _placeholderFactory ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                            _backgroundScheduler,
                            pageFetchEvents.AsObserver());

                        IPageStorage<T> PageStoreFactory(int count) =>
                            _pageLoadingBehavior == PageLoadingBehavior.Preloading
                                ? new PreloadingPageStorage<T>(
                                    _pageSize,
                                    count,
                                    NonPreloadingPageFetcherFactory,
                                    PreloadingPageFetcherFactory,
                                    _pageHoldingBehavior)
                                : new PageStorage<T>(
                                    _pageSize,
                                    count,
                                    NonPreloadingPageFetcherFactory,
                                    _pageHoldingBehavior);
                    }
                default: throw new ArgumentException("Can't build data-virtualizing collection with given input.");
            }
        }
    }
}