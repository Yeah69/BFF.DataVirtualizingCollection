using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal abstract class AsyncPageBase<T> : IPage<T>
    {
        private readonly int _pageSize;
        private readonly IDisposable _onDisposalAfterFetchCompleted;
        protected T[] Page;
        protected bool IsDisposed;

        internal AsyncPageBase(
            // parameter
            int pageKey,
            int pageSize,
            IDisposable onDisposalAfterFetchCompleted,
            
            // dependencies
            Func<int, int, T> placeholderFactory)
        {
            _pageSize = pageSize;
            _onDisposalAfterFetchCompleted = onDisposalAfterFetchCompleted;
            Page = Enumerable
                .Range(0, pageSize)
                .Select(pageIndex => placeholderFactory(pageKey, pageIndex))
                .ToArray();
        }

        public abstract Task PageFetchCompletion { get; }

        public T this[int index] =>
            index >= _pageSize || index < 0
                ? throw new IndexOutOfRangeException(
                    "Index was out of range. Must be non-negative and less than the size of the collection.")
                : Page[index];

        protected static void DisposePageItems(IEnumerable<T> page)
        {
            foreach (var disposable in page.OfType<IDisposable>())
                disposable.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            await PageFetchCompletion.ConfigureAwait(false);
            _onDisposalAfterFetchCompleted.Dispose();
            DisposePageItems(Page);
            PageFetchCompletion.Dispose();
        }
    }

    internal sealed class AsyncNonTaskBasedPage<T> : AsyncPageBase<T>
    {
        internal AsyncNonTaskBasedPage(
            // parameter
            int pageKey,
            int offset,
            int pageSize,
            IDisposable onDisposalAfterFetchCompleted,
            
            // dependencies
            Func<int, int, T[]> pageFetcher,
            Func<int, int, T> placeholderFactory,
            IScheduler pageBackgroundScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations) 
            : base(
                pageKey, 
                pageSize,
                onDisposalAfterFetchCompleted, 
                placeholderFactory)
        {
            PageFetchCompletion = Observable
                .Start(() =>
                {
                    var previousPage = Page;
                    Page = pageFetcher(offset, pageSize);
                    DisposePageItems(previousPage);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                }, pageBackgroundScheduler)
                .ToTask();
        }

        public override Task PageFetchCompletion { get; }
    }
    
    internal sealed class AsyncTaskBasedPage<T> : AsyncPageBase<T>
    {
        internal AsyncTaskBasedPage(
            // parameter
            int pageKey,
            int offset,
            int pageSize,
            IDisposable onDisposalAfterFetchCompleted,
            
            // dependencies
            Func<int, int, Task<T[]>> pageFetcher,
            Func<int, int, T> placeholderFactory,
            IScheduler pageBackgroundScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations)
            : base(
                pageKey,
                pageSize,
                onDisposalAfterFetchCompleted, 
                placeholderFactory)
        {
            PageFetchCompletion = Observable
                .StartAsync(async () =>
                {
                    var previousPage = Page;
                    Page = await pageFetcher(offset, pageSize);
                    DisposePageItems(previousPage);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                }, pageBackgroundScheduler)
                .ToTask();
        }

        public override Task PageFetchCompletion { get; }
    }
}
