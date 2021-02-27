using System;
using System.Linq;

namespace BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses
{
    public interface IAllNumbersFakeBackendAccess : IBackendAccess<int>
    {
    }

    internal class AllNumbersFakeBackendAccess : IAllNumbersFakeBackendAccess
    {
        public string Name => "All Positive Numbers";

        public int[] PageFetch(int pageOffset, int pageSize) => Enumerable.Range(pageOffset, pageSize).ToArray();

        public int PlaceholderFetch(int _, int __)
        {
            return -11;
        }

        public int PreloadingPlaceholderFetch(int _, int __)
        {
            return -21;
        }

        public int CountFetch()
        {
            return int.MaxValue;
        }
    }
}