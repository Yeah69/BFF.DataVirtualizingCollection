using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal interface IAsyncPageFetchScheduler
    {
        Task Schedule();
    }
}