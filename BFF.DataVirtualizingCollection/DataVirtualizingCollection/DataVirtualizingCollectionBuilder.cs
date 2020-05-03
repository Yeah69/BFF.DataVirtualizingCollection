using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    /// <summary>
    /// This class offers the function "Build" in order to build data virtualizing collections.
    /// The construction of such collections is encapsulated and externally only access-able via this class. 
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public sealed class DataVirtualizingCollectionBuilder<TItem> 
        : DataVirtualizingCollectionBuilderBase<TItem, IDataVirtualizingCollection<TItem>>
    {
        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// Page size is set to the default value 100.
        /// </summary>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, IDataVirtualizingCollection<TItem>> Build(
            IScheduler notificationScheduler) =>
            Build(DefaultPageSize, notificationScheduler);

        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <param name="pageSize">Maximum size of a single page.</param>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, IDataVirtualizingCollection<TItem>> Build(
            int pageSize, 
            IScheduler notificationScheduler) => 
            new DataVirtualizingCollectionBuilder<TItem>(pageSize, notificationScheduler);
        
        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <param name="pageSize">Maximum size of a single page.</param>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, IDataVirtualizingCollection<TItem>> Build(
            int pageSize, 
            IScheduler notificationScheduler,
            IScheduler backgroundScheduler) => 
            new DataVirtualizingCollectionBuilder<TItem>(pageSize, notificationScheduler, backgroundScheduler);

        private DataVirtualizingCollectionBuilder(int pageSize, IScheduler notificationScheduler)
            : base(pageSize, notificationScheduler)
        {
        }

        private DataVirtualizingCollectionBuilder(int pageSize, IScheduler notificationScheduler, IScheduler backgroundScheduler)
            : base(pageSize, notificationScheduler, backgroundScheduler)
        {
        }

        protected override IDataVirtualizingCollection<TItem> GenerateTaskBasedAsynchronousCollection(Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return new AsyncDataVirtualizingCollection<TItem>(
                GenerateTaskBasedAsynchronousPageStorage(pageFetchEvents),
                PlaceholderOnlyPageStoreFactory,
                TaskBasedCountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler);
        }

        protected override IDataVirtualizingCollection<TItem> GenerateNonTaskBasedAsynchronousCollection(Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return new AsyncDataVirtualizingCollection<TItem>(
                GenerateNonTaskBasedAsynchronousPageStorage(pageFetchEvents),
                PlaceholderOnlyPageStoreFactory,
                () => Observable.Start(CountFetcher, BackgroundScheduler).ToTask(),
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler);
        }

        protected override IDataVirtualizingCollection<TItem> GenerateNonTaskBasedSynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return new SyncDataVirtualizingCollection<TItem>(
                GenerateNonTaskBasedSynchronousPageStorage(pageFetchEvents),
                CountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler);
        }
    }
}