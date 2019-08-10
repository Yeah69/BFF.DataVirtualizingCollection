using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.Integration.DataAccess
{
    public class AsyncDataAccessTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> Combinations =>
            new List<object[]>
            {
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.Hoarding, FetchersKind.NonTaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.Hoarding, FetchersKind.TaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.NonTaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.TaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.Hoarding, FetchersKind.NonTaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.Hoarding, FetchersKind.TaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.NonTaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.TaskBased, IndexAccessBehavior.Asynchronous }
            };

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_FirstEntry_0(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = Factory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act
            var placeholder = ((IList<int>)collection)[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(0, ((IList<int>)collection)[0]);
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
            using var collection = Factory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act
            var placeholder = ((IList<int>)collection)[69];
            await Task.Delay(500);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(69, ((IList<int>)collection)[69]);
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
            using var collection = Factory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act
            var placeholder = ((IList<int>)collection)[123];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(123, ((IList<int>)collection)[123]);
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
            using var collection = Factory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act
            var placeholder = ((IList<int>)collection)[6000];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(6000, ((IList<int>)collection)[6000]);
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
            using var collection = Factory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act
            var placeholder = ((IList<int>)collection)[6968];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(6968, ((IList<int>)collection)[6968]);
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
            using var collection = Factory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => ((IList<int>)collection)[6969]);
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
            using var collection = Factory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100);

            await collection.InitializationCompleted;

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => ((IList<int>)collection)[-1]);
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
            using var collection = Factory.CreateCollectionWithIncrementalIntegerWhereFetchersIgnorePageSize(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                23);

            await collection.InitializationCompleted;

            // Act
            var placeholder = ((IList<int>)collection)[69];
            await Task.Delay(50);

            // Assert
            Assert.Equal(-1, placeholder);
            Assert.Equal(69, ((IList<int>)collection)[69]);
        }
    }
}
