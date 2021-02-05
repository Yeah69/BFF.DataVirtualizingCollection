using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Xunit;

namespace BFF.DataVirtualizingCollection.IntegrationTests.SlidingWindowDataAccess
{
    public class AsyncDataAccessTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> Combinations =>
            Enum.GetValues(typeof(PageLoadingBehavior)).OfType<PageLoadingBehavior>()
                .Cartesian(
                    Enum.GetValues(typeof(PageRemovalBehavior)).OfType<PageRemovalBehavior>(), 
                    Enum.GetValues(typeof(FetchersKind)).OfType<FetchersKind>(),
                    (first, second, third) =>
                        new object[] {first, second, third, IndexAccessBehavior.Asynchronous});

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_FirstEntry_0(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100,
                10,
                0);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(0, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_70thEntry_69(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100,
                10,
                60);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[9];
            await Task.Delay(500);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(69, collection[9]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_124thEntry_123(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100,
                10,
                120);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[3];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(123, collection[3]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_6001thEntry_6000(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100,
                10,
                6000);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(6000, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_6969thEntry_6968(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100,
                10,
                6959);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[9];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(6968, collection[9]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_11thWindowEntry_ThrowsIndexOutOfRangeException(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100,
                10,
                6959);

            await collection.InitializationCompleted;

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => collection[10]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_MinusFirstEntry_ThrowsIndexOutOfRangeException(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100,
                10,
                6959);

            await collection.InitializationCompleted;

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => collection[-1]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWherePageFetcherIgnoresGivenPageSize23_70thEntry_69(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalIntegerWhereFetchersIgnorePageSize(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                23,
                10,
                60);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[9];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(69, collection[9]);
        }
    }
}
