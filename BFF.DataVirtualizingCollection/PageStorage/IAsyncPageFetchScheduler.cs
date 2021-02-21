using System.Threading;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal interface IAsyncPageFetchScheduler
    {
        Task Schedule(int offset, CancellationToken ct);
    }
}