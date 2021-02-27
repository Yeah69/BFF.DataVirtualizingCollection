using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal class ImmediateAsyncPageFetchScheduler : IAsyncPageFetchScheduler
    {
        public Task Schedule() => Task.CompletedTask;
    }
}