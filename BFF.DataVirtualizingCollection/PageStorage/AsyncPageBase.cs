using System;
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
        private readonly IScheduler _scheduler;
        protected T[] Page;
        protected bool IsDisposed;

        internal AsyncPageBase(
            int pageKey,
            int pageSize,
            Func<int, int, T> placeholderFactory,
            IScheduler scheduler)
        {
            _pageSize = pageSize;
            _scheduler = scheduler;
            Page = Enumerable
                .Range(0, pageSize)
                .Select(pageIndex => placeholderFactory(pageKey, pageIndex))
                .ToArray();
        }

        protected abstract Task PageFetchCompletion { get; }

        public T this[int index] =>
            index >= _pageSize || index < 0
                ? throw new IndexOutOfRangeException(
                    "Index was out of range. Must be non-negative and less than the size of the collection.")
                : Page[index];

        public void Dispose()
        {
            IsDisposed = true;
            Observable
                .StartAsync(async () =>
                {
                    await PageFetchCompletion;
                    DisposePageItems(Page);
                    PageFetchCompletion.Dispose();
                }, _scheduler);
        }

        protected static void DisposePageItems(T[] page)
        {
            foreach (var disposable in page.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }

    internal sealed class AsyncNonTaskBasedPage<T> : AsyncPageBase<T>
    {
        internal AsyncNonTaskBasedPage(
            int pageKey,
            int offset,
            int pageSize,
            Func<int, int, T[]> pageFetcher,
            Func<int, int, T> placeholderFactory,
            IScheduler scheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations) 
            : base(pageKey, pageSize, placeholderFactory, scheduler)
        {
            PageFetchCompletion = Observable
                .Start(() =>
                {
                    var previousPage = Page;
                    Page = pageFetcher(offset, pageSize);
                    DisposePageItems(previousPage);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                }, scheduler)
                .ToTask();
        }

        protected override Task PageFetchCompletion { get; }
    }


    internal sealed class AsyncTaskBasedPage<T> : AsyncPageBase<T>
    {
        internal AsyncTaskBasedPage(
            int pageKey,
            int offset,
            int pageSize,
            Func<int, int, Task<T[]>> pageFetcher,
            Func<int, int, T> placeholderFactory,
            IScheduler scheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations)
            : base(pageKey,pageSize, placeholderFactory, scheduler)
        {
            PageFetchCompletion = Observable
                .StartAsync(async () =>
                {
                    var previousPage = Page;
                    Page = await pageFetcher(offset, pageSize);
                    DisposePageItems(previousPage);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                }, scheduler)
                .ToTask();
        }

        protected override Task PageFetchCompletion { get; }
    }
}
