using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using Xunit;
// ReSharper disable UnusedVariable

namespace BFF.DataVirtualizingCollection.Test
{
    public class HoardingPreloadingNonTaskBasedAsyncDataVirtualizingCollectionTest
    {
        [Fact]
        public async Task BuildingCollection0To6969_FirstEntry_0()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act
            var placeholder = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(0, collection[0]);
        }

        [Fact]
        public async Task BuildingCollection0To6969_70thEntry_69()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act
            var placeholder = collection[69];
            await Task.Delay(50);

            // Assert
            Assert.Equal(69, collection[69]);
        }

        [Fact]
        public async Task BuildingCollection0To6969_124thEntry_123()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act
            var placeholder = collection[123];
            await Task.Delay(50);

            // Assert
            Assert.Equal(123, collection[123]);
        }

        [Fact]
        public async Task BuildingCollection0To6969_6001thEntry_6000()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act
            var placeholder = collection[6000];
            await Task.Delay(50);

            // Assert
            Assert.Equal(6000, collection[6000]);
        }

        [Fact]
        public async Task BuildingCollection0To6969_6969thEntry_6968()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act
            var placeholder = collection[6968];
            await Task.Delay(50);

            // Assert
            Assert.Equal(6968, collection[6968]);
        }

        [Fact]
        public void BuildingCollection0To6969_6970thEntry_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => collection[6969]);
        }

        [Fact]
        public void BuildingCollection0To6969_MinusFirstEntry_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => collection[-1]);
        }

        [Fact]
        public async Task BuildingCollectionWherePageFetcherIgnoresGivenPageSize23_70thEntry_69()
        {
            // Arrange
            var collection = GenerateCollectionWherePageFetcherIgnoresGivenPageSize(pageSize: 23);

            // Act
            var placeholder = collection[69];
            await Task.Delay(50);

            // Assert
            Assert.Equal(69, collection[69]);
        }

        private IDataVirtualizingCollection<int> GenerateCollectionToBeTested(int count = 6969, int pageSize = 100)
        {
            return DataVirtualizingCollectionBuilder<int>
                // ReSharper disable once RedundantArgumentDefaultValue
                .Build(pageSize: pageSize)
                .Hoarding()
                .Preloading()
                .NonTaskBasedFetchers(
                    (offset, pSize) => Enumerable.Range(offset, pSize).ToArray(),
                    () => count)
                .AsyncIndexAccess(() => -1, TaskPoolScheduler.Default, NewThreadScheduler.Default);
        }

        private IDataVirtualizingCollection<int> GenerateCollectionWherePageFetcherIgnoresGivenPageSize(int count = 6969, int pageSize = 100)
        {
            return DataVirtualizingCollectionBuilder<int>
                // ReSharper disable once RedundantArgumentDefaultValue
                .Build(pageSize: pageSize)
                .Hoarding()
                .Preloading()
                .NonTaskBasedFetchers(
                    (offset, pSize) => Enumerable
                        .Range(offset, pageSize) // <--- This is different
                        .ToArray(),
                    () => count)
                .AsyncIndexAccess(() => -1, TaskPoolScheduler.Default, NewThreadScheduler.Default);
        }
    }
}
