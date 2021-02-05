using System.Collections.Generic;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses
{
    public interface IBackendAccess<T>
    {
        string Name { get; }
        
        T[] PageFetch(int pageOffset, int pageSize);

        async IAsyncEnumerable<T> AsyncEnumerablePageFetch(int pageOffset, int pageSize)
        {
            foreach (var item in PageFetch(pageOffset, pageSize))
            {
                await Task.Delay(1);
                yield return item;
            }
        }

        T PlaceholderFetch(int pageOffset, int indexInsidePage);

        T PreloadingPlaceholderFetch(int pageOffset, int indexInsidePage);

        int CountFetch();
    }
}