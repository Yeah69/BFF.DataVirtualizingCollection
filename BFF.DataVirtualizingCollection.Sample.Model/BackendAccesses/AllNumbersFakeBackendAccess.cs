using System.Linq;

namespace BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses
{
    public interface IAllNumbersFakeBackendAccess : IBackendAccess<int>
    {
    }

    internal class AllNumbersFakeFakeBackendAccess : IAllNumbersFakeBackendAccess
    {
        public string Name => "All Positive Numbers";

        public int[] PageFetch(int pageOffset, int pageSize)
        {
            return Enumerable.Range(pageOffset, pageSize).ToArray();
        }

        public int PlaceholderFetch(int _, int __)
        {
            return -1;
        }

        public int CountFetch()
        {
            return int.MaxValue;
        }
    }
}