﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using MoreLinq.Extensions;
using Xunit;

namespace BFF.DataVirtualizingCollection.Tests.Integration.SlidingWindowSpecific
{
    public class AsyncTests
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
        public async Task BuildingCollectionWith6969Elements_Offset69SlidingLeft_Offset68(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);
            
            await collection.InitializationCompleted;

            // Act
            collection.SlideLeft();
            var _ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(68, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset0SlidingLeft_Offset0(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                0,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.SlideLeft();
            var _ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(0, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset69SlidingRight_Offset70(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.SlideRight();
            var _ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(70, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset6959SlidingRight_Offset6959(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                6959,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.SlideRight();
            var _ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(6959, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset69JumpTo169_Offset169(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.JumpTo(169);
            var _ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(169, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset69JumpToMinus1_Offset0(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.JumpTo(-1);
            var _ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(0, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset69JumpTo6970_Offset6959(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.JumpTo(6970);
            var _ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(6959, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset69WindowSize10Increase_CountIncreasedLastElement79(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.IncreaseWindowSize();
            var _ = collection[10];
            await Task.Delay(50);

            // Assert
            Assert.Equal(11, collection.Count);
            Assert.Equal(79, collection[10]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset69WindowSize10IncreaseBy2_CountIncreasedLastElement80(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.IncreaseWindowSizeBy(2);
            var _ = collection[11];
            await Task.Delay(50);

            // Assert
            Assert.Equal(12, collection.Count);
            Assert.Equal(80, collection[11]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset6959WindowSize10Increase_CountIncreasedLastElement6968(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                6959,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.IncreaseWindowSize();
            var _ = collection[10];
            await Task.Delay(50);

            // Assert
            Assert.Equal(11, collection.Count);
            Assert.Equal(6968, collection[10]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset69WindowSize10Decrease_CountDecreasedLastElement77(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.DecreaseWindowSize();
            var _ = collection[8];
            await Task.Delay(50);

            // Assert
            Assert.Equal(9, collection.Count);
            Assert.Equal(77, collection[8]);
            Assert.Throws<IndexOutOfRangeException>(() => collection[9]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Offset69WindowSize10DecreaseBy2_CountDecreasedLastElement76(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;

            // Act
            collection.DecreaseWindowSizeBy(2);
            var _ = collection[7];
            await Task.Delay(50);

            // Assert
            Assert.Equal(8, collection.Count);
            Assert.Equal(76, collection[7]);
            Assert.Throws<IndexOutOfRangeException>(() => collection[8]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Reset_NothingChanged(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            await using var collection = SlidingWindowFactory.CreateCollectionWithIncrementalInteger(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;
            
            var _ = collection[0];
            await Task.Delay(50);

            // Act
            collection.Reset();
            await Task.Delay(50);
            var __ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(69, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Reset_SwitchedPageFetching(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            bool switched = false;

            int[] PageFetcher(int offset, int size) =>
                switched 
                    ? Enumerable.Range(offset + 1, size).ToArray() 
                    : Enumerable.Range(offset, size).ToArray();
            
            await using var collection = SlidingWindowFactory.CreateCollectionWithCustomPageFetchingLogic(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                10,
                69,
                PageFetcher,
                -1,
                scheduler);

            await collection.InitializationCompleted;
            
            var _ = collection[0];
            await Task.Delay(50);
            switched = true;

            // Act
            collection.Reset();
            await Task.Delay(50);
            var __ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(70, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Reset_SwitchedCountFetchingOffsetAdjusted(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            bool switched = false;

            int CountFetcher() =>
                switched 
                    ? 70 
                    : 6969;
            
            await using var collection = SlidingWindowFactory.CreateCollectionWithCustomCountFetcher(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                CountFetcher,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;
            
            var _ = collection[0];
            await Task.Delay(50);
            switched = true;

            // Act
            collection.Reset();
            await Task.Delay(50);
            var __ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(60, collection[0]);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task BuildingCollectionWith6969Elements_Reset_SwitchedCountFetchingCountAdjusted(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            bool switched = false;

            int CountFetcher() =>
                switched 
                    ? 9 
                    : 6969;
            
            await using var collection = SlidingWindowFactory.CreateCollectionWithCustomCountFetcher(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                CountFetcher,
                10,
                10,
                69,
                scheduler);

            await collection.InitializationCompleted;
            
            var _ = collection[0];
            await Task.Delay(50);
            switched = true;

            // Act
            collection.Reset();
            await Task.Delay(50);
            var __ = collection[0];
            await Task.Delay(50);

            // Assert
            Assert.Equal(0, collection[0]);
            Assert.Equal(9, collection.Count);
        }
    }
}
