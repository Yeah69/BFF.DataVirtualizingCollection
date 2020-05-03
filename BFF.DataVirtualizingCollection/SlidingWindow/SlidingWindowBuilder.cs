using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    /// <summary>
    /// This class offers the function "Build" in order to build data virtualizing collections.
    /// The construction of such collections is encapsulated and externally only access-able via this class. 
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public class SlidingWindowBuilder<TItem> 
        : DataVirtualizingCollectionBuilderBase<TItem, ISlidingWindow<TItem>>
    {
        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// Page size is set to the default value 100.
        /// </summary>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, ISlidingWindow<TItem>> Build(
            int windowSize, 
            int initialOffset,
            IScheduler notificationScheduler) => 
            Build(windowSize, initialOffset, DefaultPageSize, notificationScheduler);

        /// <summary>
        /// Initial entry point for creating a data virtualizing collection.
        /// This call can be used to configure the maximum size of a single page. Hence, this configures how much data will be loaded at once.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <returns>The builder itself.</returns>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, ISlidingWindow<TItem>> Build(
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
        public static IPageLoadingBehaviorCollectionBuilder<TItem, ISlidingWindow<TItem>> Build(
            int windowSize,
            int initialOffset, 
            int pageSize,
            IScheduler notificationScheduler,
            IScheduler backgroundScheduler) => 
            new SlidingWindowBuilder<TItem>(windowSize, initialOffset, pageSize, notificationScheduler, backgroundScheduler);

        private readonly int _windowSize;
        private readonly int _initialOffset;

        private SlidingWindowBuilder(int windowSize, int initialOffset, int pageSize, IScheduler notificationScheduler)
            : base(pageSize, notificationScheduler)
        {
            _windowSize = windowSize;
            _initialOffset = initialOffset;
        }

        private SlidingWindowBuilder(int windowSize, int initialOffset, int pageSize, IScheduler notificationScheduler, IScheduler backgroundScheduler)
            : base(pageSize, notificationScheduler, backgroundScheduler)
        {
            _windowSize = windowSize;
            _initialOffset = initialOffset;
        }

        protected override ISlidingWindow<TItem> GenerateTaskBasedAsynchronousCollection(Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return new AsyncSlidingWindow<TItem>(
                _windowSize,
                _initialOffset,
                GenerateTaskBasedAsynchronousPageStorage(pageFetchEvents),
                PlaceholderOnlyPageStoreFactory,
                TaskBasedCountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler);
        }

        protected override ISlidingWindow<TItem> GenerateNonTaskBasedAsynchronousCollection(Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return new AsyncSlidingWindow<TItem>(
                _windowSize,
                _initialOffset,
                GenerateNonTaskBasedAsynchronousPageStorage(pageFetchEvents), 
                PlaceholderOnlyPageStoreFactory,
                () => Observable.Start(CountFetcher, BackgroundScheduler).ToTask(), 
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler);
        }

        protected override ISlidingWindow<TItem> GenerateNonTaskBasedSynchronousCollection(Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            return new SyncSlidingWindow<TItem>(
                _windowSize,
                _initialOffset,
                GenerateNonTaskBasedSynchronousPageStorage(pageFetchEvents), 
                CountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage),
                NotificationScheduler);
        }
    }
}