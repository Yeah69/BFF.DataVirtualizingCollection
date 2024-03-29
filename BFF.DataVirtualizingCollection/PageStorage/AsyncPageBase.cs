﻿using System;
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
        private readonly IAsyncPageFetchScheduler _asyncPageFetchScheduler;
        protected readonly IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> PageArrivalObservations;
        protected readonly CancellationTokenSource CancellationTokenSource = new();
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
            IAsyncPageFetchScheduler asyncPageFetchScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations)
        {
            Offset = offset;
            PageSize = pageSize;
            _onDisposalAfterFetchCompleted = onDisposalAfterFetchCompleted;
            _asyncPageFetchScheduler = asyncPageFetchScheduler;
            PageArrivalObservations = pageArrivalObservations;
            Page = Enumerable
                .Range(0, pageSize)
                .Select(pageIndex => placeholderFactory(pageKey, pageIndex))
                .ToArray();
        }
            
        protected async Task FetchPage(CancellationToken ct)
        {
            await _asyncPageFetchScheduler.Schedule().ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
            await FetchPageInner(ct).ConfigureAwait(false);
        }

        protected abstract Task FetchPageInner(CancellationToken ct);

        public abstract Task PageFetchCompletion { get; }

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
            IAsyncPageFetchScheduler asyncPageFetchScheduler,
            IScheduler pageBackgroundScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations) 
            : base(
                pageKey, 
                offset,
                pageSize,
                onDisposalAfterFetchCompleted, 
                placeholderFactory,
                asyncPageFetchScheduler,
                pageArrivalObservations)
        {
            _pageFetcher = pageFetcher;
            PageFetchCompletion = Observable
                .StartAsync(FetchPage, pageBackgroundScheduler)
                .ToTask(CancellationTokenSource.Token);
        }

        protected override async Task FetchPageInner(CancellationToken ct)
        {
            await Task.Delay(1, ct).ConfigureAwait(false);
            var previousPage = Page;
            Page = _pageFetcher(Offset, PageSize, ct);
            await DisposePageItems(previousPage).ConfigureAwait(false);
            if (!IsDisposed)
                PageArrivalObservations.OnNext((Offset, PageSize, previousPage, Page));
            else
                await DisposePageItems(Page).ConfigureAwait(false);
        }

        public override Task PageFetchCompletion { get; }
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
            IAsyncPageFetchScheduler asyncPageFetchScheduler,
            IScheduler pageBackgroundScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations)
            : base(
                pageKey, 
                offset,
                pageSize,
                onDisposalAfterFetchCompleted, 
                placeholderFactory,
                asyncPageFetchScheduler,
                pageArrivalObservations)
        {
            _pageFetcher = pageFetcher;
            PageFetchCompletion = Observable
                .StartAsync(FetchPage, pageBackgroundScheduler)
                .ToTask(CancellationTokenSource.Token);
        }

        protected override async Task FetchPageInner(CancellationToken ct)
        {
            var previousPage = Page;
            Page = await _pageFetcher(Offset, PageSize, ct).ConfigureAwait(false);
            await DisposePageItems(previousPage).ConfigureAwait(false);
            if (!IsDisposed)
                PageArrivalObservations.OnNext((Offset, PageSize, previousPage, Page));
            else
                await DisposePageItems(Page).ConfigureAwait(false);
        }

        public override Task PageFetchCompletion { get; }
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
            IAsyncPageFetchScheduler asyncPageFetchScheduler,
            IScheduler pageBackgroundScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations)
            : base(
                pageKey, 
                offset,
                pageSize,
                onDisposalAfterFetchCompleted, 
                placeholderFactory,
                asyncPageFetchScheduler,
                pageArrivalObservations)
        {
            _pageFetcher = pageFetcher;
            PageFetchCompletion = Observable
                .StartAsync(FetchPage, pageBackgroundScheduler)
                .ToTask(CancellationTokenSource.Token);
        }

        protected override async Task FetchPageInner(CancellationToken ct)
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

        public override Task PageFetchCompletion { get; }
    }
}
