using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using MrMeeseeks.Extensions;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal abstract class AsyncPageBase<T> : IPage<T>
    {
        protected readonly int Offset;
        protected readonly int PageSize;
        private readonly IDisposable _onDisposalAfterFetchCompleted;
        protected readonly IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> PageArrivalObservations;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        protected T[] Page;
        protected bool IsDisposed;

        internal AsyncPageBase(
            // parameter
            int pageKey,
            int offset,
            int pageSize,
            IDisposable onDisposalAfterFetchCompleted,
            
            // dependencies
            Func<int, int, T> placeholderFactory,
            IScheduler pageBackgroundScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations)
        {
            Offset = offset;
            PageSize = pageSize;
            _onDisposalAfterFetchCompleted = onDisposalAfterFetchCompleted;
            PageArrivalObservations = pageArrivalObservations;
            Page = Enumerable
                .Range(0, pageSize)
                .Select(pageIndex => placeholderFactory(pageKey, pageIndex))
                .ToArray();
            PageFetchCompletion = Observable
                .StartAsync(OnPageFetch, pageBackgroundScheduler)
                .ToTask(_cancellationTokenSource.Token);
        }

        protected abstract Task OnPageFetch(CancellationToken ct);

        public Task PageFetchCompletion { get; }

        public T this[int index] =>
            index >= PageSize || index < 0
                ? throw new IndexOutOfRangeException(
                    "Index was out of range. Must be non-negative and less than the size of the collection.")
                : Page[index];

        protected static async Task DisposePageItems(T[] page)
        {
            foreach (var disposable in page.OfType<IDisposable>())
                disposable.Dispose();
            foreach (var disposable in page.OfType<IAsyncDisposable>())
                await disposable.DisposeAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            try
            {
                _cancellationTokenSource.Cancel();
                await PageFetchCompletion.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            finally
            {
                await DisposePageItems(Page).ConfigureAwait(false);
                PageFetchCompletion.Dispose();
                _onDisposalAfterFetchCompleted.Dispose();
            }
        }
    }

    internal sealed class AsyncNonTaskBasedPage<T> : AsyncPageBase<T>
    {
        private readonly Func<int, int, CancellationToken, T[]> _pageFetcher;

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
                offset,
                pageSize,
                onDisposalAfterFetchCompleted, 
                placeholderFactory,
                pageBackgroundScheduler,
                pageArrivalObservations) =>
            _pageFetcher = pageFetcher;

        protected override async Task OnPageFetch(CancellationToken ct)
        {
            var previousPage = Page;
            await Task.Delay(1, ct).ConfigureAwait(false);
            Page = _pageFetcher(Offset, PageSize, ct);
            await DisposePageItems(previousPage).ConfigureAwait(false);
            if (!IsDisposed)
                PageArrivalObservations.OnNext((Offset, PageSize, previousPage, Page));
            else
                await DisposePageItems(Page).ConfigureAwait(false);
        }
    }
    
    internal sealed class AsyncTaskBasedPage<T> : AsyncPageBase<T>
    {
        private readonly Func<int, int, CancellationToken, Task<T[]>> _pageFetcher;

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
                offset,
                pageSize,
                onDisposalAfterFetchCompleted, 
                placeholderFactory,
                pageBackgroundScheduler,
                pageArrivalObservations) =>
            _pageFetcher = pageFetcher;

        protected override async Task OnPageFetch(CancellationToken ct)
        {
            var previousPage = Page;
            await Task.Delay(1, ct).ConfigureAwait(false);
            Page = await _pageFetcher(Offset, PageSize, ct).ConfigureAwait(false);
            await DisposePageItems(previousPage).ConfigureAwait(false);
            if (!IsDisposed)
                PageArrivalObservations.OnNext((Offset, PageSize, previousPage, Page));
            else
                await DisposePageItems(Page).ConfigureAwait(false);
        }
    }
    
    internal sealed class AsyncEnumerableBasedPage<T> : AsyncPageBase<T>
    {
        private readonly Func<int, int, CancellationToken, IAsyncEnumerable<T>> _pageFetcher;

        internal AsyncEnumerableBasedPage(
            // parameter
            int pageKey,
            int offset,
            int pageSize,
            IDisposable onDisposalAfterFetchCompleted,
            
            // dependencies
            Func<int, int, CancellationToken, IAsyncEnumerable<T>> pageFetcher,
            Func<int, int, T> placeholderFactory,
            IScheduler pageBackgroundScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations)
            : base(
                pageKey, 
                offset,
                pageSize,
                onDisposalAfterFetchCompleted, 
                placeholderFactory,
                pageBackgroundScheduler,
                pageArrivalObservations) =>
            _pageFetcher = pageFetcher;

        protected override async Task OnPageFetch(CancellationToken ct)
        {
            var i = 0;
            await foreach (var item in _pageFetcher(Offset, PageSize, ct))
            {
                ct.ThrowIfCancellationRequested();
                var temp = Page[i];
                Page[i] = item;
                (temp as IDisposable)?.Dispose();
                if (temp is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                if(IsDisposed.Not())
                    PageArrivalObservations.OnNext((Offset + i, 1, temp.ToEnumerable().ToArray(), item.ToEnumerable().ToArray()));
                i++;
            }
        }
    }
}
