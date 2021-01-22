using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal abstract class AsyncPageBase<T> : IPage<T>
    {
        private readonly int _pageSize;
        private readonly IDisposable _onDisposalAfterFetchCompleted;
        protected readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
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
            try
            {
                CancellationTokenSource.Cancel();
                await PageFetchCompletion.ConfigureAwait(false);
                DisposePageItems(Page);
                PageFetchCompletion.Dispose();
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            finally
            {
                _onDisposalAfterFetchCompleted.Dispose();
            }
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
            Func<int, int, CancellationToken, T[]> pageFetcher,
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
                .StartAsync(ct =>
                {
                    var previousPage = Page;
                    Page = pageFetcher(offset, pageSize, ct);
                    DisposePageItems(previousPage);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                    return Task.CompletedTask;
                }, pageBackgroundScheduler)
                .ToTask(CancellationTokenSource.Token);
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
            Func<int, int, CancellationToken, Task<T[]>> pageFetcher,
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
                .StartAsync(async ct =>
                {
                    var previousPage = Page;
                    Page = await pageFetcher(offset, pageSize, ct);
                    DisposePageItems(previousPage);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                }, pageBackgroundScheduler)
                .ToTask(CancellationTokenSource.Token);
        }

        public override Task PageFetchCompletion { get; }
    }
}
