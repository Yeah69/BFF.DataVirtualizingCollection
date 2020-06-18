using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MoreLinq.Extensions;
using Xunit;

namespace BFF.DataVirtualizingCollection.IntegrationTests.SlidingWindowSpecific
{
    public class SyncTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> Combinations =>
            Enum.GetValues(typeof(PageLoadingBehavior)).OfType<PageLoadingBehavior>()
                .Cartesian(
                    Enum.GetValues(typeof(PageRemovalBehavior)).OfType<PageRemovalBehavior>(), 
                    Enum.GetValues(typeof(FetchersKind)).OfType<FetchersKind>().Except(new [] { FetchersKind.TaskBased }),
                    (first, second, third) =>
                        new object[] {first, second, third, IndexAccessBehavior.Synchronous});

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset69SlidingLeft_Offset68(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69);

            // Act
            collection.SlideLeft();

            // Assert
            Assert.Equal(68, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset0SlidingLeft_Offset0(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                0);

            // Act
            collection.SlideLeft();

            // Assert
            Assert.Equal(0, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset69SlidingRight_Offset70(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                20,
                10,
                69);

            // Act
            collection.SlideRight();

            // Assert
            Assert.Equal(70, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset6959SlidingRight_Offset6959(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                6959);

            // Act
            collection.SlideRight();

            // Assert
            Assert.Equal(6959, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset69JumpTo169_Offset169(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69);

            // Act
            collection.JumpTo(169);

            // Assert
            Assert.Equal(169, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset69JumpToMinus1_Offset0(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69);

            // Act
            collection.JumpTo(-1);

            // Assert
            Assert.Equal(0, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset69JumpTo6970_Offset6959(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69);

            // Act
            collection.JumpTo(6970);

            // Assert
            Assert.Equal(6959, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset69WindowSize10Increase_CountIncreasedLastElement79(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69);

            // Act
            collection.IncreaseWindowSize();
            
            Thread.Sleep(500); // wait for preloading

            // Assert
            Assert.Equal(11, collection.Count);
            Assert.Equal(79, collection[10]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset69WindowSize10IncreaseBy2_CountIncreasedLastElement80(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69);

            // Act
            collection.IncreaseWindowSizeBy(2);
            
            Thread.Sleep(500); // wait for preloading

            // Assert
            Assert.Equal(12, collection.Count);
            Assert.Equal(80, collection[11]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset6959WindowSize10Increase_CountIncreasedLastElement6968(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                6959);

            // Act
            collection.IncreaseWindowSize();
            
            Thread.Sleep(500); // wait for preloading

            // Assert
            Assert.Equal(11, collection.Count);
            Assert.Equal(6968, collection[10]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset69WindowSize10Decrease_CountDecreasedLastElement77(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69);

            // Act
            collection.DecreaseWindowSize();
            
            Thread.Sleep(500); // wait for preloading

            // Assert
            Assert.Equal(9, collection.Count);
            Assert.Equal(77, collection[8]);
            Assert.Throws<IndexOutOfRangeException>(() => collection[9]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Offset69WindowSize10DecreaseBy2_CountDecreasedLastElement76(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69);

            // Act
            collection.DecreaseWindowSizeBy(2);
            
            Thread.Sleep(500); // wait for preloading

            // Assert
            Assert.Equal(8, collection.Count);
            Assert.Equal(76, collection[7]);
            Assert.Throws<IndexOutOfRangeException>(() => collection[8]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Reset_NothingChanged(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69);

            // Act
            collection.Reset();

            // Assert
            Assert.Equal(69, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Reset_SwitchedPageFetching(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            bool switched = false;

            int[] PageFetcher(int offset, int size) =>
                switched 
                    ? Enumerable.Range(offset + 1, size).ToArray() 
                    : Enumerable.Range(offset, size).ToArray();
            
            using var collection = SlidingWindowFactory.CreateCollectionWithCustomPageFetchingLogic(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                PageFetcher,
                -1);
            
            var previous = collection[0];
            switched = true;

            // Act
            collection.Reset();

            // Assert
            Assert.Equal(69, previous);
            Assert.Equal(70, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Reset_SwitchedCountFetchingOffsetAdjusted(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            bool switched = false;

            int CountFetcher() =>
                switched 
                    ? 70 
                    : 6969;
            
            using var collection = SlidingWindowFactory.CreateCollectionWithCustomCountFetcher(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                CountFetcher,
                10,
                10,
                69);
            
            var previous = collection[0];
            switched = true;

            // Act
            collection.Reset();

            // Assert
            Assert.Equal(69, previous);
            Assert.Equal(60, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public void BuildingCollectionWith6969Elements_Reset_SwitchedCountFetchingCountAdjusted(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            bool switched = false;

            int CountFetcher() =>
                switched 
                    ? 9 
                    : 6969;
            
            using var collection = SlidingWindowFactory.CreateCollectionWithCustomCountFetcher(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                CountFetcher,
                10,
                10,
                69);
            
            var previous = collection[0];
            switched = true;

            // Act
            collection.Reset();

            // Assert
            Assert.Equal(69, previous);
            Assert.Equal(0, collection[0]);
            Assert.Equal(9, collection.Count);
        }
    }
}
