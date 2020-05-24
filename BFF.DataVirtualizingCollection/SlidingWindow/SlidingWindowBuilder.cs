using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    public static class SlidingWindowBuilder
    {
        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// Page size is set to the default value 100.
        /// </summary>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, ISlidingWindow<TItem>> Build<TItem>(
            int windowSize, 
            int initialOffset,
            IScheduler notificationScheduler) => 
            Build<TItem>(windowSize, initialOffset, DataVirtualizingCollectionBuilderBase.DefaultPageSize, notificationScheduler);

        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, ISlidingWindow<TItem>> Build<TItem>(
            int windowSize,
            int initialOffset, 
            int pageSize,
            IScheduler notificationScheduler) => 
            new SlidingWindowBuilder<TItem>(windowSize, initialOffset, pageSize, notificationScheduler);

        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, ISlidingWindow<TItem>> Build<TItem>(
            int windowSize,
            int initialOffset, 
            int pageSize,
            IScheduler notificationScheduler,
            IScheduler backgroundScheduler) => 
            new SlidingWindowBuilder<TItem>(windowSize, initialOffset, pageSize, notificationScheduler, backgroundScheduler);
    }

    internal class SlidingWindowBuilder<TItem> 
        : DataVirtualizingCollectionBuilderBase<TItem, ISlidingWindow<TItem>>
    {
        private readonly int _windowSize;
        private readonly int _initialOffset;

        internal SlidingWindowBuilder(int windowSize, int initialOffset, int pageSize, IScheduler notificationScheduler)
            : base(pageSize, notificationScheduler)
        {
            _windowSize = windowSize;
            _initialOffset = initialOffset;
        }

        internal SlidingWindowBuilder(int windowSize, int initialOffset, int pageSize, IScheduler notificationScheduler, IScheduler backgroundScheduler)
            : base(pageSize, notificationScheduler, backgroundScheduler)
        {
            _windowSize = windowSize;
            _initialOffset = initialOffset;
        }

        protected override ISlidingWindow<TItem> GenerateTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            var taskBasedCountFetcher = TaskBasedCountFetcher ??
                                        throw new NullReferenceException(UninitializedElementsExceptionMessage);
            return new AsyncSlidingWindow<TItem>(
                _windowSize,
                _initialOffset,
                GenerateTaskBasedAsynchronousPageStorage(pageFetchEvents),
                PlaceholderOnlyPageStoreFactory,
                taskBasedCountFetcher,
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler,
                CountBackgroundScheduler);
        }

        protected override ISlidingWindow<TItem> GenerateNonTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            var countFetcher = CountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
            return new AsyncSlidingWindow<TItem>(
                _windowSize,
                _initialOffset,
                GenerateNonTaskBasedAsynchronousPageStorage(pageFetchEvents), 
                PlaceholderOnlyPageStoreFactory,
                () => Task.FromResult(countFetcher()), 
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler,
                CountBackgroundScheduler);
        }

        protected override ISlidingWindow<TItem> GenerateNonTaskBasedSynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            var countFetcher = CountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
            return new SyncSlidingWindow<TItem>(
                _windowSize,
                _initialOffset,
                GenerateNonTaskBasedSynchronousPageStorage(pageFetchEvents), 
                countFetcher,
                pageFetchEvents,
                NotificationScheduler);
        }
    }
}