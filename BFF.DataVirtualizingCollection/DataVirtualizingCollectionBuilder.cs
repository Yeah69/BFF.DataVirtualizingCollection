using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection
{
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
        IPageLoadingBehaviorCollectionBuilder<T> Hoarding();
    }

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
        IFetchersKindCollectionBuilder<T> NonPreloading();

        /// <summary>
        /// Pages are loaded as soon as an item of the page is requested or as soon as a neighboring page is loaded.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IFetchersKindCollectionBuilder<T> Preloading();
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

    internal enum PageHoldingBehavior
    {
        Hoarding
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
    /// This class offers the function <see cref="Build"/> in order to build data virtualizing collections.
    /// The construction of such collections is encapsulated and externally only access-able via this class. 
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public class DataVirtualizingCollectionBuilder<T> 
        : IPageHoldingBehaviorCollectionBuilder<T>, 
            IPageLoadingBehaviorCollectionBuilder<T>,
            IFetchersKindCollectionBuilder<T>,
            IIndexAccessBehaviorCollectionBuilder<T>
    { 
        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <param name="pageSize">Maximum size of a single page.</param>
        /// <returns>The builder itself.</returns>
        public static IPageHoldingBehaviorCollectionBuilder<T> Build(int pageSize = 100) => new DataVirtualizingCollectionBuilder<T>();

        private readonly int _pageSize;

        private PageHoldingBehavior _pageHoldingBehavior;

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
        public IPageLoadingBehaviorCollectionBuilder<T> Hoarding()
        {
            _pageHoldingBehavior = PageHoldingBehavior.Hoarding;
            return this;
        }

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<T> NonPreloading()
        {
            _pageLoadingBehavior = PageLoadingBehavior.NonPreloading;
            return this;
        }

        /// <inheritdoc />
        public IFetchersKindCollectionBuilder<T> Preloading()
        {
            _pageLoadingBehavior = PageLoadingBehavior.Preloading;
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
            switch (_pageHoldingBehavior)
            {
                case PageHoldingBehavior.Hoarding when
                _pageLoadingBehavior == PageLoadingBehavior.NonPreloading
                && _fetchersKind == FetchersKind.NonTaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Synchronous:
                {
                    var dataAccess = new RelayBasicSyncDataAccess<T>(_pageFetcher, _countFetcher);
                    var pageStore = HoardingSyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess)
                        .WithPageSize(_pageSize)
                        .Build();
                    return SyncDataVirtualizingCollection<T>
                        .CreateBuilder()
                        .WithPageStore(
                            pageStore,
                            dataAccess)
                        .Build();
                }
                case PageHoldingBehavior.Hoarding when
                _pageLoadingBehavior == PageLoadingBehavior.NonPreloading
                && _fetchersKind == FetchersKind.NonTaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Asynchronous:
                {
                    var dataAccess = new RelayBasicAsyncDataAccess<T>(_pageFetcher, _countFetcher, _placeholderFactory);
                    var pageStore = HoardingAsyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess, _backgroundScheduler)
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
                
                case PageHoldingBehavior.Hoarding when
                _pageLoadingBehavior == PageLoadingBehavior.NonPreloading
                && _fetchersKind == FetchersKind.TaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Synchronous:
                {
                    var dataAccess =
                        new RelayBasicTaskBasedSyncDataAccess<T>(_taskBasedPageFetcher, _taskBasedCountFetcher);
                    var pageStore = HoardingTaskBasedSyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess)
                        .WithPageSize(_pageSize)
                        .Build();
                    return TaskBasedSyncDataVirtualizingCollection<T>
                        .CreateBuilder()
                        .WithPageStore(
                            pageStore,
                            dataAccess)
                        .Build();
                }
                case PageHoldingBehavior.Hoarding when
                _pageLoadingBehavior == PageLoadingBehavior.NonPreloading
                && _fetchersKind == FetchersKind.TaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Asynchronous:
                {
                    var dataAccess =
                        new RelayBasicTaskBasedAsyncDataAccess<T>(_taskBasedPageFetcher, _taskBasedCountFetcher, _placeholderFactory);
                        var pageStore = HoardingTaskBasedAsyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess, _backgroundScheduler)
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




                case PageHoldingBehavior.Hoarding when
                _pageLoadingBehavior == PageLoadingBehavior.Preloading
                && _fetchersKind == FetchersKind.NonTaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Synchronous:
                    {
                        var dataAccess = new RelayBasicSyncDataAccess<T>(_pageFetcher, _countFetcher);
                        var pageStore = HoardingPreloadingSyncPageStore<T>
                            .CreateBuilder()
                            .With(dataAccess)
                            .WithPageSize(_pageSize)
                            .Build();
                        return SyncDataVirtualizingCollection<T>
                            .CreateBuilder()
                            .WithPageStore(
                                pageStore,
                                dataAccess)
                            .Build();
                    }
                case PageHoldingBehavior.Hoarding when
                _pageLoadingBehavior == PageLoadingBehavior.Preloading
                && _fetchersKind == FetchersKind.NonTaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Asynchronous:
                    {
                        var dataAccess = new RelayBasicAsyncDataAccess<T>(_pageFetcher, _countFetcher, _placeholderFactory);
                        var pageStore = HoardingPreloadingAsyncPageStore<T>
                            .CreateBuilder()
                            .With(dataAccess, _backgroundScheduler)
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

                case PageHoldingBehavior.Hoarding when
                _pageLoadingBehavior == PageLoadingBehavior.Preloading
                && _fetchersKind == FetchersKind.TaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Synchronous:
                    {
                        var dataAccess =
                            new RelayBasicTaskBasedSyncDataAccess<T>(_taskBasedPageFetcher, _taskBasedCountFetcher);
                        var pageStore = HoardingPreloadingTaskBasedSyncPageStore<T>
                            .CreateBuilder()
                            .With(dataAccess)
                            .WithPageSize(_pageSize)
                            .Build();
                        return TaskBasedSyncDataVirtualizingCollection<T>
                            .CreateBuilder()
                            .WithPageStore(
                                pageStore,
                                dataAccess)
                            .Build();
                    }
                case PageHoldingBehavior.Hoarding when
                _pageLoadingBehavior == PageLoadingBehavior.Preloading
                && _fetchersKind == FetchersKind.TaskBased
                && _indexAccessBehavior == IndexAccessBehavior.Asynchronous:
                    {
                        var dataAccess =
                            new RelayBasicTaskBasedAsyncDataAccess<T>(_taskBasedPageFetcher, _taskBasedCountFetcher, _placeholderFactory);
                        var pageStore = HoardingPreloadingTaskBasedAsyncPageStore<T>
                        .CreateBuilder()
                        .With(dataAccess, _backgroundScheduler)
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