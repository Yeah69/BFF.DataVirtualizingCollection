using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal class SyncNonPreloadingNonTaskBasedPage<T> : IPage<T>
    {
        private readonly int _pageSize;
        private readonly IDisposable _onDisposalAfterFetchCompleted;

        internal SyncNonPreloadingNonTaskBasedPage(
            // parameter
            int offset,
            int pageSize,
            IDisposable onDisposalAfterFetchCompleted,
            
            // dependencies
            Func<int, int, CancellationToken, T[]> pageFetcher)
        {
            _pageSize = pageSize;
            _onDisposalAfterFetchCompleted = onDisposalAfterFetchCompleted;
            PageContent = pageFetcher(offset, pageSize, CancellationToken.None);
        }

        private T[] PageContent { get; }

        public T this[int index] =>
            index >= _pageSize || index < 0
                ? throw new IndexOutOfRangeException(
                    "Index was out of range. Must be non-negative and less than the size of the collection.")
                : PageContent[index];

        public Task PageFetchCompletion => Task.CompletedTask;
        
        public async ValueTask DisposeAsync()
        {
            await PageFetchCompletion.ConfigureAwait(false);
            _onDisposalAfterFetchCompleted.Dispose();
            foreach (var disposable in PageContent.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}
