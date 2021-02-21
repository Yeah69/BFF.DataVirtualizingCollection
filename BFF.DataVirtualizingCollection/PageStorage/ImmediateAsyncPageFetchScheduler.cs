using System.Threading;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal class ImmediateAsyncPageFetchScheduler : IAsyncPageFetchScheduler
    {
        public Task Schedule(int offset, CancellationToken ct) => Task.CompletedTask;
    }
}