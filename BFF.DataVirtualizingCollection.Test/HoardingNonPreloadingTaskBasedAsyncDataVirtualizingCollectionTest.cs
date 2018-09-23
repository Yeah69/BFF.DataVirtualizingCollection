using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using Xunit;
// ReSharper disable UnusedVariable

namespace BFF.DataVirtualizingCollection.Test
{
    public class HoardingNonPreloadingTaskBasedAsyncDataVirtualizingCollectionTest
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

        private IDataVirtualizingCollection<int> GenerateCollectionToBeTested(int count = 6969)
        {
            return DataVirtualizingCollectionBuilder<int>
                // ReSharper disable once RedundantArgumentDefaultValue
                .Build(pageSize: 100)
                .Hoarding()
                .NonPreloading()
                .TaskBasedFetchers(
                    (offset, pageSize) => 
                        Task.FromResult(Enumerable
                            .Range(offset, count)
                            .ToArray()),
                    () => Task.FromResult(count))
                .AsyncIndexAccess(() => -1, TaskPoolScheduler.Default, NewThreadScheduler.Default);
        }
    }
}
