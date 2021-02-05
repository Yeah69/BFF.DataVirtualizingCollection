using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Xunit;

namespace BFF.DataVirtualizingCollection.IntegrationTests.DataVirtualizingCollectionDataAccess
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
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

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
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[69];
            await Task.Delay(500);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(69, collection[69]);
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
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[123];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(123, collection[123]);
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
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[6000];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(6000, collection[6000]);
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
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[6968];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(6968, collection[6968]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_6970thEntry_ThrowsIndexOutOfRangeException(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => collection[6969]);
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
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

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
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithIncrementalIntegerWhereFetchersIgnorePageSize(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                23);

            await collection.InitializationCompleted;

            // Act
            var placeholder = collection[69];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(69, collection[69]);
        }
    }
}
