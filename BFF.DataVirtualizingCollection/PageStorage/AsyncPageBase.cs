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
        private readonly IScheduler _pageBackgroundScheduler;
        protected T[] Page;
        protected bool IsDisposed;

        internal AsyncPageBase(
            int pageKey,
            int pageSize,
            Func<int, int, T> placeholderFactory,
            IScheduler pageBackgroundScheduler)
        {
            _pageSize = pageSize;
            _pageBackgroundScheduler = pageBackgroundScheduler;
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
            _pageBackgroundScheduler.Schedule(async () =>
                {
                    await PageFetchCompletion;
                    DisposePageItems(Page);
                    PageFetchCompletion.Dispose();
                });
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
            IScheduler pageBackgroundScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations) 
            : base(pageKey, pageSize, placeholderFactory, pageBackgroundScheduler)
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
            IScheduler pageBackgroundScheduler,
            IObserver<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> pageArrivalObservations)
            : base(pageKey,pageSize, placeholderFactory, pageBackgroundScheduler)
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

        protected override Task PageFetchCompletion { get; }
    }

    internal class PlaceholderOnlyPage<T> : AsyncPageBase<T>
    {
        public PlaceholderOnlyPage(
            int pageKey, 
            int pageSize, 
            Func<int, int, T> placeholderFactory, 
            IScheduler pageBackgroundScheduler) 
            : base(pageKey, pageSize, placeholderFactory, pageBackgroundScheduler)
        {
        }

        protected override Task PageFetchCompletion => Task.CompletedTask;
    }
}
