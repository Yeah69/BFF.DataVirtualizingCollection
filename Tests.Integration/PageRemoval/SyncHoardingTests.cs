using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Xunit;

namespace BFF.DataVirtualizingCollection.IntegrationTests.PageRemoval
{
    public class SyncHoardingTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> Combinations =>
            Enum.GetValues(typeof(PageLoadingBehavior)).OfType<PageLoadingBehavior>()
                .Cartesian(
                    Enum.GetValues(typeof(FetchersKind)).OfType<FetchersKind>().Except(new [] { FetchersKind.TaskBased }),
                    (first, second) =>
                        new object[] {first, PageRemovalBehavior.Hoarding, second, IndexAccessBehavior.Synchronous});

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task With6969ElementsAndPageSize100_GetAnElementFromEachPage_NoneDisposed(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var set = new ConcurrentBag<int>();
            await using var collection = DataVirtualizingCollectionFactory.CreateCollectionWithCustomPageFetchingLogic(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100,
                (offset, pSize) => 
                    Enumerable
                        .Range(offset, pSize)
                        .Select(i => Disposable.Create(() => set.Add(i)))
                        .ToArray(),
                Disposable.Empty);

            // Act
            for (var i = 0; i <= 6900; i += 100)
            {
                var _ = collection[i];
            }

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
                Disposable.Empty);

            // Act
            for (var i = 0; i <= expected; i += 10)
            {
                var _ = collection[i];
            }
            await collection.DisposeAsync();
            
            Thread.Sleep(500);

            // Assert
            Assert.Equal(expected, set.Count);
        }
    }
}
