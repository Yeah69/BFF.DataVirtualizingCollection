using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DataVirtualizingCollection.PageRemoval;
using BFF.DataVirtualizingCollection.PageStores;
using BFF.DataVirtualizingCollection.Utilities;

namespace BFF.DataVirtualizingCollection
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
        /// </summary>
        /// <returns>The builder itself.</returns>
        IPageHoldingBehaviorCollectionBuilder<T> Preloading();
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
        IDataVirtualizingCollection<T> AsyncIndexAccess(Func<T> placeholderFactory, IScheduler backgroundScheduler, IScheduler notificationScheduler);
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

        private Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> _pageHoldingBehavior;

        private PageLoadingBehavior _pageLoadingBehavior;

        private FetchersKind _fetchersKind;

        private IndexAccessBehavior _indexAccessBehavior;

        private Func<int, int, T[]> _pageFetcher;

        private Func<int> _countFetcher;

        private Func<int, int, Task<T[]>> _taskBasedPageFetcher;

        private Func<Task<int>> _taskBasedCountFetcher;

        private Func<T> _placeholderFactory;

        private IScheduler _backgroundScheduler;

        private IScheduler _notificationScheduler;

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
            _placeholderFactory = placeholderFactory;
            _backgroundScheduler = backgroundScheduler;
            _notificationScheduler = notificationScheduler;
            return GenerateCollection();
        }

        private IDataVirtualizingCollection<T> GenerateCollection()
        {
            switch (_pageLoadingBehavior)
            {
                case PageLoadingBehavior.NonPreloading when
                _fetchersKind == FetchersKind.NonTaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Synchronous:
                {
                    var dataAccess = new RelayBasicSyncDataAccess<T>(_pageFetcher, _countFetcher);
                    var pageStore = HoardingSyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess, _pageHoldingBehavior)
                        .WithPageSize(_pageSize)
                        .Build();
                    return SyncDataVirtualizingCollection<T>
                        .CreateBuilder()
                        .WithPageStore(
                            pageStore,
                            dataAccess)
                        .Build();
                }
                case PageLoadingBehavior.NonPreloading when
                _fetchersKind == FetchersKind.NonTaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Asynchronous:
                {
                    var dataAccess = new RelayBasicAsyncDataAccess<T>(_pageFetcher, _countFetcher, _placeholderFactory);
                    var pageStore = HoardingAsyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess, _backgroundScheduler, _pageHoldingBehavior)
                        .WithPageSize(_pageSize)
                        .Build();
                    return AsyncDataVirtualizingCollection<T>
                        .CreateBuilder()
                        .WithPageStore(
                            pageStore,
                            dataAccess, 
                            _backgroundScheduler,
                            _notificationScheduler)
                        .Build();
                }
                
                case PageLoadingBehavior.NonPreloading when
                    _fetchersKind == FetchersKind.TaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Synchronous:
                {
                    var dataAccess =
                        new RelayBasicTaskBasedSyncDataAccess<T>(_taskBasedPageFetcher, _taskBasedCountFetcher);
                    var pageStore = HoardingTaskBasedSyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess, _pageHoldingBehavior)
                        .WithPageSize(_pageSize)
                        .Build();
                    return TaskBasedSyncDataVirtualizingCollection<T>
                        .CreateBuilder()
                        .WithPageStore(
                            pageStore,
                            dataAccess)
                        .Build();
                }
                case PageLoadingBehavior.NonPreloading when
                _fetchersKind == FetchersKind.TaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Asynchronous:
                {
                    var dataAccess =
                        new RelayBasicTaskBasedAsyncDataAccess<T>(_taskBasedPageFetcher, _taskBasedCountFetcher, _placeholderFactory);
                        var pageStore = HoardingTaskBasedAsyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess, _backgroundScheduler, _pageHoldingBehavior)
                        .WithPageSize(_pageSize)
                        .Build();
                    return TaskBasedAsyncDataVirtualizingCollection<T>
                        .CreateBuilder()
                        .WithPageStore(
                            pageStore,
                            dataAccess,
                            _backgroundScheduler,
                            _notificationScheduler)
                        .Build();
                }




                case PageLoadingBehavior.Preloading when
                _fetchersKind == FetchersKind.NonTaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Synchronous:
                    {
                        var dataAccess = new RelayBasicSyncDataAccess<T>(_pageFetcher, _countFetcher);
                        var pageStore = HoardingPreloadingSyncPageStore<T>
                            .CreateBuilder()
                            .With(dataAccess, _pageHoldingBehavior)
                            .WithPageSize(_pageSize)
                            .Build();
                        return SyncDataVirtualizingCollection<T>
                            .CreateBuilder()
                            .WithPageStore(
                                pageStore,
                                dataAccess)
                            .Build();
                    }
                case PageLoadingBehavior.Preloading when
                _fetchersKind == FetchersKind.NonTaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Asynchronous:
                    {
                        var dataAccess = new RelayBasicAsyncDataAccess<T>(_pageFetcher, _countFetcher, _placeholderFactory);
                        var pageStore = HoardingPreloadingAsyncPageStore<T>
                            .CreateBuilder()
                            .With(dataAccess, _backgroundScheduler, _pageHoldingBehavior)
                            .WithPageSize(_pageSize)
                            .Build();
                        return AsyncDataVirtualizingCollection<T>
                            .CreateBuilder()
                            .WithPageStore(
                                pageStore,
                                dataAccess,
                                _backgroundScheduler,
                                _notificationScheduler)
                            .Build();
                    }

                case PageLoadingBehavior.Preloading when
                _fetchersKind == FetchersKind.TaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Synchronous:
                    {
                        var dataAccess =
                            new RelayBasicTaskBasedSyncDataAccess<T>(_taskBasedPageFetcher, _taskBasedCountFetcher);
                        var pageStore = HoardingPreloadingTaskBasedSyncPageStore<T>
                            .CreateBuilder()
                            .With(dataAccess, _pageHoldingBehavior)
                            .WithPageSize(_pageSize)
                            .Build();
                        return TaskBasedSyncDataVirtualizingCollection<T>
                            .CreateBuilder()
                            .WithPageStore(
                                pageStore,
                                dataAccess)
                            .Build();
                    }
                case PageLoadingBehavior.Preloading when
                _fetchersKind == FetchersKind.TaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Asynchronous:
                    {
                        var dataAccess =
                            new RelayBasicTaskBasedAsyncDataAccess<T>(_taskBasedPageFetcher, _taskBasedCountFetcher, _placeholderFactory);
                        var pageStore = HoardingPreloadingTaskBasedAsyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess, _backgroundScheduler, _pageHoldingBehavior)
                        .WithPageSize(_pageSize)
                        .Build();
                        return TaskBasedAsyncDataVirtualizingCollection<T>
                            .CreateBuilder()
                            .WithPageStore(
                                pageStore,
                                dataAccess,
                                _backgroundScheduler,
                                _notificationScheduler)
                            .Build();
                    }
            }

            throw new ArgumentException("Couldn't create a collection based on given settings.");
        }
    }
}