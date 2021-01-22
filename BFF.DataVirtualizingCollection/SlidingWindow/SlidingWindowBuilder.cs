using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;

namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    /// <summary>
    /// Initial entry point for creating a sliding window.
    /// </summary>
    public static class SlidingWindowBuilder
    {
        /// <summary>
        /// Use to configure general virtualization and sliding-window-specific settings.
        /// Further settings are applied via method chaining.
        /// Page size is set to the default value 100.
        /// The background scheduler is per default the <see cref="TaskPoolScheduler"/>.
        /// </summary>
        /// <param name="windowSize">Initial count of items that the window should contain.</param>
        /// <param name="initialOffset">Initial starting item within the backend.</param>
        /// <param name="notificationScheduler">A scheduler for sending the notifications (<see cref="INotifyCollectionChanged"/>, <see cref="INotifyPropertyChanged"/>).</param>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, ISlidingWindow<TItem>> Build<TItem>(
            int windowSize, 
            int initialOffset,
            IScheduler notificationScheduler) => 
            Build<TItem>(windowSize, initialOffset, DataVirtualizingCollectionBuilderBase.DefaultPageSize, notificationScheduler);

        /// <summary>
        /// Use to configure general virtualization and sliding-window-specific settings.
        /// Further settings are applied via method chaining.
        /// The background scheduler is per default the <see cref="TaskPoolScheduler"/>.
        /// </summary>
        /// <param name="windowSize">Initial count of items that the window should contain.</param>
        /// <param name="initialOffset">Initial starting item within the backend.</param>
        /// <param name="pageSize">Maximum size of a single page.</param>
        /// <param name="notificationScheduler">A scheduler for sending the notifications (<see cref="INotifyCollectionChanged"/>, <see cref="INotifyPropertyChanged"/>).</param>
        public static IPageLoadingBehaviorCollectionBuilder<TItem, ISlidingWindow<TItem>> Build<TItem>(
            int windowSize,
            int initialOffset, 
            int pageSize,
            IScheduler notificationScheduler) => 
            new SlidingWindowBuilder<TItem>(windowSize, initialOffset, pageSize, notificationScheduler);

        /// <summary>
        /// Use to configure general virtualization and sliding-window-specific settings.
        /// Further settings are applied via method chaining.
        /// </summary>
        /// <param name="windowSize">Initial count of items that the window should contain.</param>
        /// <param name="initialOffset">Initial starting item within the backend.</param>
        /// <param name="pageSize">Maximum size of a single page.</param>
        /// <param name="notificationScheduler">A scheduler for sending the notifications (<see cref="INotifyCollectionChanged"/>, <see cref="INotifyPropertyChanged"/>).</param>
        /// <param name="backgroundScheduler">Per default this scheduler is used for all background operations (page and count fetches, preloading). In further settings you'll have the option to override this scheduler with another for specific background operations. </param>
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
            
            var dvc = new AsyncDataVirtualizingCollection<TItem>(
                GenerateTaskBasedAsynchronousPageStorage(pageFetchEvents),
                taskBasedCountFetcher,
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler,
                CountBackgroundScheduler);
            return new SlidingWindow<TItem>(
                _initialOffset,
                _windowSize,
                dvc,
                NotificationScheduler);
        }

        protected override ISlidingWindow<TItem> GenerateNonTaskBasedAsynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            var countFetcher = CountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
            
            var dvc = new AsyncDataVirtualizingCollection<TItem>(
                GenerateTaskBasedAsynchronousPageStorage(pageFetchEvents),
                ct => Task.FromResult(countFetcher(ct)), 
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler,
                CountBackgroundScheduler);
            return new SlidingWindow<TItem>(
                _initialOffset,
                _windowSize,
                dvc, 
                NotificationScheduler);
        }

        protected override ISlidingWindow<TItem> GenerateNonTaskBasedSynchronousCollection(
            Subject<(int Offset, int PageSize, TItem[] PreviousPage, TItem[] Page)> pageFetchEvents)
        {
            var countFetcher = CountFetcher ?? throw new NullReferenceException(UninitializedElementsExceptionMessage);
            
            var dvc = new SyncDataVirtualizingCollection<TItem>(
                GenerateNonTaskBasedSynchronousPageStorage(pageFetchEvents), 
                countFetcher,
                pageFetchEvents.AsObservable(),
                pageFetchEvents,
                NotificationScheduler);
            return new SlidingWindow<TItem>(
                _initialOffset,
                _windowSize,
                dvc, 
                NotificationScheduler);
        }
    }
}