using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal abstract class SyncPreloadingPageBase<T> : IPage<T>
    {
        private readonly int _pageSize;
        private readonly IScheduler _scheduler;
        protected AsyncSubject<T[]> PageContent;

        internal SyncPreloadingPageBase(
            int pageSize,
            [NotNull] IScheduler scheduler)
        {
            scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            _pageSize = pageSize;
            _scheduler = scheduler;

        }

        public T this[int index] =>
            index >= _pageSize || index < 0
                ? throw new IndexOutOfRangeException(
                    "Index was out of range. Must be non-negative and less than the size of the collection.")
                // The blocking call to GetResult is accepted because this is the sync access mode
                : PageContent.GetResult()[index];

        public void Dispose()
        {
            Observable
                .StartAsync(async () =>
                {
                    foreach (var disposable in (await PageContent).OfType<IDisposable>())
                    {
                        disposable.Dispose();
                    }
                    PageContent.Dispose();
                }, _scheduler);
        }
    }

    internal sealed class SyncPreloadingNonTaskBasedPage<T> : SyncPreloadingPageBase<T>
    {
        internal SyncPreloadingNonTaskBasedPage(
            int offset,
            int pageSize,
            [NotNull] Func<int, int, T[]> pageFetcher,
            [NotNull] IScheduler scheduler) : base(pageSize, scheduler)
        {
            pageFetcher = pageFetcher ?? throw new ArgumentNullException(nameof(pageFetcher));
            scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            PageContent = Observable
                .Start(() => pageFetcher(offset, pageSize), scheduler)
                .RunAsync(CancellationToken.None);
        }
    }

    internal sealed class SyncPreloadingTaskBasedPage<T> : SyncPreloadingPageBase<T>
    {
        internal SyncPreloadingTaskBasedPage(
            int offset,
            int pageSize,
            [NotNull] Func<int, int, Task<T[]>> pageFetcher,
            [NotNull] IScheduler scheduler) : base(pageSize, scheduler)
        {
            pageFetcher = pageFetcher ?? throw new ArgumentNullException(nameof(pageFetcher));
            scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            PageContent = Observable
                .StartAsync(() => pageFetcher(offset, pageSize), scheduler)
                .RunAsync(CancellationToken.None);
        }
    }
}
