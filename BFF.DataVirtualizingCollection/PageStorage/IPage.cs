using System;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal interface IPage : IAsyncDisposable
    {
        Task PageFetchCompletion { get; }
    }
    
    internal interface IPage<out T> : IPage
    {
        T this[int index] { get; }
    }
}
