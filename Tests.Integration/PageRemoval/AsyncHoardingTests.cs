using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using MoreLinq.Extensions;
using Xunit;

namespace BFF.DataVirtualizingCollection.Tests.Integration.PageRemoval
{
    public class AsyncHoardingTests
    {
        public static IEnumerable<object[]> Combinations =>
            Enum.GetValues(typeof(PageLoadingBehavior)).OfType<PageLoadingBehavior>()
                .Cartesian(
                    Enum.GetValues(typeof(FetchersKind)).OfType<FetchersKind>(),
                    (first, second) =>
                        new object[] {first, PageRemovalBehavior.Hoarding, second, IndexAccessBehavior.Asynchronous});

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task With6969ElementsAndPageSize1000_GetAnElementFromEachPage_NoneDisposed(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            var set = new ConcurrentBag<int>();
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithCustomPageFetchingLogic(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                10,
                (offset, pSize) =>
                    Enumerable
                        .Range(offset, pSize)
                        .Select(i => Disposable.Create(() => set.Add(i)))
                        .ToArray(),
                Disposable.Empty,
                scheduler);

            scheduler.AdvanceBy(20);
            Assert.True(collection.InitializationCompleted.IsCompletedSuccessfully);

            // Act
            for (var i = 0; i <= 6900; i += 100)
            {
                var _ = collection[i];
            }
            await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
            scheduler.AdvanceBy(20);

            // Assert
            Assert.Empty(set);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task With69ElementsAndPageSize10_GetAnElementFromEachPageDisposeCollection_AllDisposed(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var scheduler = new TestScheduler();
            const int expected = 69;
            var set = new ConcurrentBag<int>();
            var collection = DataVirtualizingCollectionFactory.CreateCollectionWithCustomPageFetchingLogic(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                expected,
                10,
                (offset, pSize) =>
                    Enumerable
                        .Range(offset, pSize)
                        .Select(i => Disposable.Create(() => set.Add(i)))
                        .ToArray(),
                Disposable.Empty,
                scheduler);

            scheduler.AdvanceBy(20);
            Assert.True(collection.InitializationCompleted.IsCompletedSuccessfully);

            // Act
            for (var i = 0; i <= expected; i += 10)
            {
                var _ = collection[i];
                await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
                scheduler.AdvanceBy(20);
            }
            await collection.DisposeAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
            scheduler.AdvanceBy(20);

            // Assert
            Assert.Equal(expected, set.Count);
        }
    }
}
