using System.Reactive.Concurrency;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection
{
    /// <summary>
    /// Is intended to provide a convenient way to build data virtualizing collections for consumers.
    /// Each function has the required components as normal parameters and the optionally configurable components as optional parameters.
    /// The names of the functions describe which kind of collection is build.
    /// </summary>
    /// <typeparam name="T">The type of the elements provided by the build collection.</typeparam>
    public interface ICollectionBuilder<T>
    {
        /// <summary>
        /// Builds a data virtualizing collection which operates async (i.e. placeholders are returned if element is not available yet)
        /// and hoards the fetched pages (i.e. the once fetched pages of data are kept in memory for the life time of the collection).
        /// </summary>
        /// <param name="dataAccess">Provides access to the data, see <see cref="IBasicAsyncDataAccess{T}"/></param>
        /// <param name="subscribeScheduler">Is used to schedule task, which should be done in the background. 
        /// Basically, it is everything besides the notifications, like fetching the pages.
        /// It is advised to used the ThreadPoolScheduler in order to do the heavy lifting in the background.</param>
        /// <param name="observeScheduler">Is used to schedule the notification. For GUI applications it is mandatory to use the DispatcherScheduler. </param>
        /// <param name="pageSize">Optionally, the page size can be configured, whereas a page size of 100 elements is considered as universal enough to be the default.
        /// A lesser page size will consequently mean more frequent page fetches and a greater page size mean a bigger chunk of data is fetched at once.
        /// This parameter allows adjusting to specific requirements.</param>
        /// <returns>The requested data virtualizing collection.</returns>
        IDataVirtualizingCollection<T> BuildAHoardingAsyncCollection(
            IBasicAsyncDataAccess<T> dataAccess,
            IScheduler subscribeScheduler,
            IScheduler observeScheduler,
            int pageSize = 100);

        /// <summary>
        /// Build a similar data virtualizing collection to the <see cref="BuildAHoardingAsyncCollection"/> function. 
        /// However, this collection preloads consequtive pages in the background.
        /// </summary>
        /// <param name="dataAccess">Provides access to the data, see <see cref="IBasicAsyncDataAccess{T}"/></param>
        /// <param name="subscribeScheduler">Is used to schedule task, which should be done in the background. 
        /// Basically, it is everything besides the notifications, like fetching the pages.
        /// It is advised to used the ThreadPoolScheduler in order to do the heavy lifting in the background.</param>
        /// <param name="observeScheduler">Is used to schedule the notification. For GUI applications it is mandatory to use the DispatcherScheduler. </param>
        /// <param name="pageSize">Optionally, the page size can be configured, whereas a page size of 100 elements is considered as universal enough to be the default.
        /// A lesser page size will consequently mean more frequent page fetches and a greater page size mean a bigger chunk of data is fetched at once.
        /// This parameter allows adjusting to specific requirements.</param>
        /// <returns>The requested data virtualizing collection.</returns>
        IDataVirtualizingCollection<T> BuildAHoardingPreloadingAsyncCollection(
            IBasicAsyncDataAccess<T> dataAccess,
            IScheduler subscribeScheduler,
            IScheduler observeScheduler,
            int pageSize = 100);

        /// <summary>
        /// Builds a data virtualizing collection which operates synchronous (i.e. it waits until the page fetch is completed if page is not available yet)
        /// and hoards the fetched pages (i.e. the once fetched pages of data are kept in memory for the life time of the collection).
        /// </summary>
        /// <param name="dataAccess">Provides access to the data, see <see cref="IBasicSyncDataAccess{T}"/></param>
        /// <param name="pageSize">Optionally, the page size can be configured, whereas a page size of 100 elements is considered as universal enough to be the default.
        /// A lesser page size will consequently mean more frequent page fetches and a greater page size mean a bigger chunk of data is fetched at once.
        /// This parameter allows adjusting to specific requirements.</param>
        /// <returns>The requested data virtualizing collection.</returns>
        IDataVirtualizingCollection<T> BuildAHoardingSyncCollection(
            IBasicAsyncDataAccess<T> dataAccess,
            int pageSize = 100);

        /// <summary>
        /// Builds a similar data virtualizing collection to the <see cref="BuildAHoardingSyncCollection"/> function. 
        /// However, this collection preloads consequtive pages in the background.
        /// </summary>
        /// <param name="dataAccess">Provides access to the data, see <see cref="IBasicSyncDataAccess{T}"/></param>
        /// <param name="pageSize">Optionally, the page size can be configured, whereas a page size of 100 elements is considered as universal enough to be the default.
        /// A lesser page size will consequently mean more frequent page fetches and a greater page size mean a bigger chunk of data is fetched at once.
        /// This parameter allows adjusting to specific requirements.</param>
        /// <returns>The requested data virtualizing collection.</returns>
        IDataVirtualizingCollection<T> BuildAHoardingPreloadingSyncCollection(
            IBasicAsyncDataAccess<T> dataAccess,
            int pageSize = 100);
    }

    /// <inheritdoc />
    public class CollectionBuilder<T> : ICollectionBuilder<T>
    {
        /// <summary>
        /// This factory method creates an instance of a <see cref="ICollectionBuilder{T}"/>.
        /// </summary>
        /// <returns>An instance of a <see cref="ICollectionBuilder{T}"/>.</returns>
        public static ICollectionBuilder<T> CreateBuilder() => new CollectionBuilder<T>();

        /// <inheritdoc />
        public IDataVirtualizingCollection<T> BuildAHoardingAsyncCollection(
            IBasicAsyncDataAccess<T> dataAccess, 
            IScheduler subscribeScheduler,
            IScheduler observeScheduler,
            int pageSize = 100)
        {
            var hoardingPageStore = HoardingAsyncPageStore<T>
                .CreateBuilder()
                .With(
                    dataAccess, 
                    subscribeScheduler)
                .WithPageSize(pageSize)
                .Build();
            return AsyncDataVirtualizingCollection<T>
                .CreateBuilder()
                .WithPageStore(
                    hoardingPageStore, 
                    dataAccess, 
                    subscribeScheduler, 
                    observeScheduler)
                .Build();
        }

        /// <inheritdoc />
        public IDataVirtualizingCollection<T> BuildAHoardingPreloadingAsyncCollection(
            IBasicAsyncDataAccess<T> dataAccess,
            IScheduler subscribeScheduler, 
            IScheduler observeScheduler,
            int pageSize = 100)
        {
            var hoardingPageStore = HoardingPreloadingAsyncPageStore<T>
                .CreateBuilder()
                .With(
                    dataAccess,
                    subscribeScheduler)
                .WithPageSize(pageSize)
                .Build();
            return AsyncDataVirtualizingCollection<T>
                .CreateBuilder()
                .WithPageStore(
                    hoardingPageStore,
                    dataAccess,
                    subscribeScheduler,
                    observeScheduler)
                .Build();
        }

        /// <inheritdoc />
        public IDataVirtualizingCollection<T> BuildAHoardingSyncCollection(IBasicAsyncDataAccess<T> dataAccess, int pageSize = 100)
        {
            var hoardingPageStore = HoardingSyncPageStore<T>
                .CreateBuilder()
                .With(dataAccess)
                .WithPageSize(pageSize)
                .Build();
            return SyncDataVirtualizingCollection<T>
                .CreateBuilder()
                .WithPageStore(
                    hoardingPageStore,
                    dataAccess)
                .Build();
        }

        /// <inheritdoc />
        public IDataVirtualizingCollection<T> BuildAHoardingPreloadingSyncCollection(IBasicAsyncDataAccess<T> dataAccess, int pageSize = 100)
        {
            var hoardingPageStore = HoardingPreloadingSyncPageStore<T>
                .CreateBuilder()
                .With(dataAccess)
                .WithPageSize(pageSize)
                .Build();
            return SyncDataVirtualizingCollection<T>
                .CreateBuilder()
                .WithPageStore(
                    hoardingPageStore,
                    dataAccess)
                .Build();
        }

        private CollectionBuilder() 
        {
        }
    }
}