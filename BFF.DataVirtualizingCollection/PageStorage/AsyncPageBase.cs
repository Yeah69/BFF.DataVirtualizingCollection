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
        protected Task PageFetchCompletion;
        protected T[] Page;
        protected bool IsDisposed;

        internal AsyncPageBase(
            int pageSize,
            Func<T> placeholderFactory,
            IScheduler scheduler)
        {
            _pageSize = pageSize;
            _scheduler = scheduler;
            Page = Enumerable
                .Range(0, pageSize)
                .Select(_ => placeholderFactory())
                .ToArray();
        }

        public T this[int index]
        {
            get
            {
                if (index >= _pageSize || index < 0)
                    throw new IndexOutOfRangeException(
                        "Index was out of range. Must be non-negative and less than the size of the collection.");

                return Page[index];
                            
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
            Observable
                .StartAsync(async () =>
                {
                    await PageFetchCompletion;
                    foreach (var disposable in Page.OfType<IDisposable>())
                    {
                        disposable.Dispose();
                    }
                    PageFetchCompletion.Dispose();
                }, _scheduler);
        }
    }

    internal sealed class AsyncNonTaskBasedPage<T> : AsyncPageBase<T>
    {
        internal AsyncNonTaskBasedPage(
            int offset,
            int pageSize,
            Func<int, int, T[]> pageFetcher,
            Func<T> placeholderFactory,
            IScheduler scheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations) 
            : base(pageSize, placeholderFactory, scheduler)
        {
            PageFetchCompletion = Observable
                .Start(() =>
                {
                    var previousPage = Page;
                    Page = pageFetcher(offset, pageSize);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                }, scheduler)
                .ToTask();
        }
    }


    internal sealed class AsyncTaskBasedPage<T> : AsyncPageBase<T>
    {
        internal AsyncTaskBasedPage(
            int offset,
            int pageSize,
            Func<int, int, Task<T[]>> pageFetcher,
            Func<T> placeholderFactory,
            IScheduler scheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations)
            : base(pageSize, placeholderFactory, scheduler)
        {
            PageFetchCompletion = Observable
                .StartAsync(async () =>
                {
                    var previousPage = Page;
                    Page = await pageFetcher(offset, pageSize);
                    if (!IsDisposed)
                        pageArrivalObservations.OnNext((offset, pageSize, previousPage, Page));
                }, scheduler)
                .ToTask();
        }
    }
}
