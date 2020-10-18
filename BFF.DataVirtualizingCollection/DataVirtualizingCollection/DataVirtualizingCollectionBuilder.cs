using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    /// <summary>
    /// Initial entry point for creating a data virtualizing collection.
    /// </summary>
    public static class DataVirtualizingCollectionBuilder
    {
        /// <summary>
        /// Use to configure general virtualization settings.
        /// Further settings are applied via method chaining.
        /// Page size is set to the default value 100.
        /// The background scheduler is per default the <see cref="TaskPoolScheduler"/>.
        /// </summary>
        /// <param name="notificationScheduler">A scheduler for sending the notifications (<see cref="INotifyCollectionChanged"/>, <see cref="INotifyPropertyChanged"/>).</param>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, IDataVirtualizingCollection<TItem>> Build<TItem>(
            IScheduler notificationScheduler) =>
            Build<TItem>(DataVirtualizingCollectionBuilderBase.DefaultPageSize, notificationScheduler);

        /// <summary>
        /// Use to configure general virtualization settings.
        /// Further settings are applied via method chaining.
        /// The background scheduler is per default the <see cref="TaskPoolScheduler"/>.
        /// </summary>
        /// <param name="pageSize">Maximum size of a single page.</param>
        /// <param name="notificationScheduler">A scheduler for sending the notifications (<see cref="INotifyCollectionChanged"/>, <see cref="INotifyPropertyChanged"/>).</param>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, IDataVirtualizingCollection<TItem>> Build<TItem>(
            int pageSize, 
            IScheduler notificationScheduler) => 
            new DataVirtualizingCollectionBuilder<TItem>(pageSize, notificationScheduler);
        
        /// <summary>
        /// Use to configure general virtualization settings.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <param name="pageSize">Maximum size of a single page.</param>
        /// <param name="notificationScheduler">A scheduler for sending the notifications (<see cref="INotifyCollectionChanged"/>, <see cref="INotifyPropertyChanged"/>).</param>
        /// <param name="backgroundScheduler">Per default this scheduler is used for all background operations (page and count fetches, preloading). In further settings you'll have the option to override this scheduler with another for specific background operations. </param>
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