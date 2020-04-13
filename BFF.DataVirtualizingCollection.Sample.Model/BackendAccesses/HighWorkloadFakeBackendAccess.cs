using System.Linq;
using BFF.DataVirtualizingCollection.Sample.Model.Models;

namespace BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses
{
    public interface IHighWorkloadFakeBackendAccess : IBackendAccess<ISomeWorkloadObject>
    {
    }

    internal class HighWorkloadFakeBackendAccess : IHighWorkloadFakeBackendAccess
    {
        private ISomeWorkloadObject Placeholder { get; } = (ISomeWorkloadObject) new LowWorkloadObject(-1);
        
        public string Name => "High Workload Simulation";

        public ISomeWorkloadObject[] PageFetch(int pageOffset, int pageSize)
        {
            return Enumerable
                .Range(pageOffset, pageSize)
                .Select(i => (ISomeWorkloadObject) new HighWorkloadObject(i))
                .ToArray();
        }
        
        public ISomeWorkloadObject PlaceholderFetch(int _, int __)
        {
            return Placeholder;
        }

        public int CountFetch()
        {
            return int.MaxValue;
        }
    }
}