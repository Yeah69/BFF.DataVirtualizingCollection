using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
// ReSharper disable UnusedVariable

namespace BFF.DataVirtualizingCollection.Test
{
    public class HoardingNonPreloadingTaskBasedAsyncDataVirtualizingCollectionTest : AsyncDataVirtualizingCollectionTestBase
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
                .AsyncIndexAccess(() => -1, TaskPoolScheduler.Default, NewThreadScheduler.Default);
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
                .AsyncIndexAccess(() => -1, TaskPoolScheduler.Default, NewThreadScheduler.Default);
        }
    }
}
