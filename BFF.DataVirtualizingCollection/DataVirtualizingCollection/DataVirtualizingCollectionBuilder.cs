using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    public static class DataVirtualizingCollectionBuilder
    {
        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// Page size is set to the default value 100.
        /// </summary>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, IDataVirtualizingCollection<TItem>> Build<TItem>(
            IScheduler notificationScheduler) =>
            Build<TItem>(DataVirtualizingCollectionBuilderBase.DefaultPageSize, notificationScheduler);

        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <param name="pageSize">Maximum size of a single page.</param>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, IDataVirtualizingCollection<TItem>> Build<TItem>(
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
        public static IPageLoadingBehaviorCollectionBuilder<TItem, IDataVirtualizingCollection<TItem>> Build<TItem>(
            int pageSize, 
            IScheduler notificationScheduler,
            IScheduler backgroundScheduler) => 
            new DataVirtualizingCollectionBuilder<TItem>(pageSize, notificationScheduler, backgroundScheduler);
    }
    
    internal sealed class DataVirtualizingCollectionBuilder<TItem> 
        : DataVirtualizingCollectionBuilderBase<TItem, IDataVirtualizingCollection<TItem>>
    {

        internal DataVirtualizingCollectionBuilder(int pageSize, IScheduler notificationScheduler)
            : base(pageSize, notificationScheduler)
        {
        }

        internal DataVirtualizingCollectionBuilder(int pageSize, IScheduler notificationScheduler, IScheduler backgroundScheduler)
            : base(pageSize, notificationScheduler, backgroundScheduler)
        {
        }

        protected override IDataVirtualizingCollection<TItem> GenerateTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            var taskBasedCountFetcher = TaskBasedCountFetcher ??
                                        throw new NullReferenceException(UninitializedElementsExceptionMessage);
            return new AsyncDataVirtualizingCollection<TItem>(
                GenerateTaskBasedAsynchronousPageStorage(pageFetchEvents),
                PlaceholderOnlyPageStoreFactory,
                taskBasedCountFetcher,
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler,
                CountBackgroundScheduler);
        }

        protected override IDataVirtualizingCollection<TItem> GenerateNonTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            var countFetcher = CountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
            return new AsyncDataVirtualizingCollection<TItem>(
                GenerateNonTaskBasedAsynchronousPageStorage(pageFetchEvents),
                PlaceholderOnlyPageStoreFactory,
                () => Task.FromResult(countFetcher()),
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler,
                CountBackgroundScheduler);
        }

        protected override IDataVirtualizingCollection<TItem> GenerateNonTaskBasedSynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            var countFetcher = CountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
            return new SyncDataVirtualizingCollection<TItem>(
                GenerateNonTaskBasedSynchronousPageStorage(pageFetchEvents),
                countFetcher,
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler);
        }
    }
}