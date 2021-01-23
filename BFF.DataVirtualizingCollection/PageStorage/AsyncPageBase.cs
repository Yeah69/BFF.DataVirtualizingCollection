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
                CancellationTokenSource.Cancel();
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
                .StartAsync(async ct =>
                {
                    var previousPage = Page;
                    await Task.Delay(1).ConfigureAwait(false);
                    Page = pageFetcher(offset, pageSize, ct);
                    await DisposePageItems(previousPage).ConfigureAwait(false);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                    else
                        await DisposePageItems(Page).ConfigureAwait(false);
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
                    await Task.Delay(1).ConfigureAwait(false);
                    Page = await pageFetcher(offset, pageSize, ct).ConfigureAwait(false);
                    await DisposePageItems(previousPage).ConfigureAwait(false);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                    else
                        await DisposePageItems(Page).ConfigureAwait(false);
                }, pageBackgroundScheduler)
                .ToTask(CancellationTokenSource.Token);
        }

        public override Task PageFetchCompletion { get; }
    }
    
    internal sealed class AsyncEnumerableBasedPage<T> : AsyncPageBase<T>
    {
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
                pageSize,
                onDisposalAfterFetchCompleted, 
                placeholderFactory)
        {
            PageFetchCompletion = Observable
                .StartAsync(async ct =>
                {
                    var i = 0;
                    await foreach (var item in pageFetcher(offset, pageSize, ct))
                    {
                        ct.ThrowIfCancellationRequested();
                        var temp = Page[i];
                        Page[i] = item;
                        (temp as IDisposable)?.Dispose();
                        if (temp is IAsyncDisposable asyncDisposable)
                            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        if(IsDisposed.Not())
                            pageArrivalObservations.OnNext((offset + i, 1, temp.ToEnumerable().ToArray(), item.ToEnumerable().ToArray()));
                        i++;
                    }
                }, pageBackgroundScheduler)
                .ToTask(CancellationTokenSource.Token);
        }

        public override Task PageFetchCompletion { get; }
    }
}
