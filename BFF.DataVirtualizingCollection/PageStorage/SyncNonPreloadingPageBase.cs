using System;
using System.Linq;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal abstract class SyncNonPreloadingPageBase<T> : IPage<T>
    {
        private readonly int _pageSize;
        protected T[] PageContent;

        internal SyncNonPreloadingPageBase(
            int pageSize)
        {
            _pageSize = pageSize;
        }

        public T this[int index] =>
            index >= _pageSize || index < 0
                ? throw new IndexOutOfRangeException(
                    "Index was out of range. Must be non-negative and less than the size of the collection.")
                : PageContent[index];

        public void Dispose()
        {
            foreach (var disposable in PageContent.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }

    internal sealed class SyncNonPreloadingNonTaskBasedPage<T> : SyncNonPreloadingPageBase<T>
    {
        internal SyncNonPreloadingNonTaskBasedPage(
            int offset,
            int pageSize,
            Func<int, int, T[]> pageFetcher) : base(pageSize)
        {
            PageContent = pageFetcher(offset, pageSize);
        }
    }

    internal sealed class SyncNonPreloadingTaskBasedPage<T> : SyncNonPreloadingPageBase<T>
    {
        internal SyncNonPreloadingTaskBasedPage(
            int offset,
            int pageSize,
            Func<int, int, Task<T[]>> pageFetcher) : base(pageSize)
        {
            // The blocking call to Result is accepted because this is the sync access mode
            PageContent = pageFetcher(offset, pageSize).Result;
        }
    }
}
