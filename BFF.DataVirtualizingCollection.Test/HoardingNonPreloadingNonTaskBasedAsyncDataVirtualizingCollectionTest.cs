using System.Linq;
using System.Reactive.Concurrency;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
// ReSharper disable UnusedVariable

namespace BFF.DataVirtualizingCollection.Test
{
    public class HoardingNonPreloadingNonTaskBasedAsyncDataVirtualizingCollectionTest : AsyncDataVirtualizingCollectionTestBase
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
                .AsyncIndexAccess(() => -1, TaskPoolScheduler.Default, NewThreadScheduler.Default);
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
                .AsyncIndexAccess(() => -1, TaskPoolScheduler.Default, NewThreadScheduler.Default);
        }
    }
}
