using System.Linq;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;

namespace BFF.DataVirtualizingCollection.Test
{
    public class HoardingNonPreloadingTaskBasedSyncDataVirtualizingCollectionTest : SyncDataVirtualizingCollectionTestBase
    {
        protected override IDataVirtualizingCollection<int> GenerateCollectionToBeTested(int count = 6969, int pageSize = 100)
        {
            return DataVirtualizingCollectionBuilder<int>
                // ReSharper disable once RedundantArgumentDefaultValue
                .Build(pageSize: pageSize)
                .Hoarding()
                .NonPreloading()
                .TaskBasedFetchers(
                    (offset, pSize) => 
                        Task.FromResult(Enumerable
                            .Range(offset, pSize)
                            .ToArray()),
                    () => Task.FromResult(count))
                .SyncIndexAccess();
        }

        protected override IDataVirtualizingCollection<int> GenerateCollectionWherePageFetcherIgnoresGivenPageSize(int count = 6969, int pageSize = 100)
        {
            return DataVirtualizingCollectionBuilder<int>
                // ReSharper disable once RedundantArgumentDefaultValue
                .Build(pageSize: pageSize)
                .Hoarding()
                .NonPreloading()
                .TaskBasedFetchers(
                    (offset, pSize) =>
                        Task.FromResult(Enumerable
                            .Range(offset, pageSize) // <--- This is different
                            .ToArray()),
                    () => Task.FromResult(count))
                .SyncIndexAccess();
        }
    }
}
