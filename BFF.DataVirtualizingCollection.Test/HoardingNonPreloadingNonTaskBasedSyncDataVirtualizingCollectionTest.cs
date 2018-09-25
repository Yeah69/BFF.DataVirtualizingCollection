using System.Linq;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;

namespace BFF.DataVirtualizingCollection.Test
{
    public class HoardingNonPreloadingNonTaskBasedSyncDataVirtualizingCollectionTest : SyncDataVirtualizingCollectionTestBase
    {
        protected override IDataVirtualizingCollection<int> GenerateCollectionToBeTested(int count = 6969, int pageSize = 100)
        {
            return DataVirtualizingCollectionBuilder<int>
                // ReSharper disable once RedundantArgumentDefaultValue
                .Build(pageSize: pageSize)
                .Hoarding()
                .NonPreloading()
                .NonTaskBasedFetchers(
                    (offset, pSize) => Enumerable.Range(offset, pSize).ToArray(),
                    () => count)
                .SyncIndexAccess();
        }

        protected override IDataVirtualizingCollection<int> GenerateCollectionWherePageFetcherIgnoresGivenPageSize(int count = 6969, int pageSize = 100)
        {
            return DataVirtualizingCollectionBuilder<int>
                // ReSharper disable once RedundantArgumentDefaultValue
                .Build(pageSize: pageSize)
                .Hoarding()
                .NonPreloading()
                .NonTaskBasedFetchers(
                    (offset, pSize) => Enumerable
                        .Range(offset, pageSize) // <--- This is different
                        .ToArray(),
                    () => count)
                .SyncIndexAccess();
        }
    }
}
