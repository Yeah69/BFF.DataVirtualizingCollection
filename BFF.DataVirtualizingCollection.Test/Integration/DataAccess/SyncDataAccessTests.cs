using System;
using System.Collections.Generic;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.Integration.DataAccess
{
    public class SyncDataAccessTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> Combinations =>
            new List<object[]>
            {
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.Hoarding, FetchersKind.NonTaskBased, IndexAccessBehavior.Synchronous },
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.Hoarding, FetchersKind.TaskBased, IndexAccessBehavior.Synchronous },
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.NonTaskBased, IndexAccessBehavior.Synchronous },
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.TaskBased, IndexAccessBehavior.Synchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.Hoarding, FetchersKind.NonTaskBased, IndexAccessBehavior.Synchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.Hoarding, FetchersKind.TaskBased, IndexAccessBehavior.Synchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.NonTaskBased, IndexAccessBehavior.Synchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.TaskBased, IndexAccessBehavior.Synchronous }
            };

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_FirstEntry_0(
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

            // Act + Assert
            Assert.Equal(0, ((IList<int>)collection)[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_70thEntry_69(
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

            // Act + Assert
            Assert.Equal(69, ((IList<int>)collection)[69]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_124thEntry_123(
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

            // Act + Assert
            Assert.Equal(123, ((IList<int>)collection)[123]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_6001thEntry_6000(
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

            // Act + Assert
            Assert.Equal(6000, ((IList<int>)collection)[6000]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_6969thEntry_6968(
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

            // Act + Assert
            Assert.Equal(6968, ((IList<int>)collection)[6968]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_6970thEntry_ThrowsIndexOutOfRangeException(
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

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => ((IList<int>)collection)[6969]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_MinusFirstEntry_ThrowsIndexOutOfRangeException(
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

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => ((IList<int>)collection)[6969]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWherePageFetcherIgnoresGivenPageSize23_70thEntry_69(
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

            // Act + Assert
            Assert.Equal(69, ((IList<int>)collection)[69]);
        }
    }
}
