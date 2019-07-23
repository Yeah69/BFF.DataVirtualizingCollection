using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.PageRemoval
{
    public class AsyncLeastRecentlyUsedTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> Combinations =>
            new List<object[]>
            {
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.NonTaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.TaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.NonTaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.LeastRecentlyUsed, FetchersKind.TaskBased, IndexAccessBehavior.Asynchronous }
            };

        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> CombinationsWherePreloading =>
            Combinations.Where(objects =>
                objects[0] is PageLoadingBehavior pageLoadingBehavior &&
                pageLoadingBehavior == PageLoadingBehavior.Preloading);

        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> CombinationsWhereNonPreloading =>
            Combinations.Where(objects =>
                objects[0] is PageLoadingBehavior pageLoadingBehavior &&
                pageLoadingBehavior == PageLoadingBehavior.NonPreloading);

        [Theory]
        [MemberData(nameof(CombinationsWhereNonPreloading))]
        public async Task With6969ElementsPageSize100_NotMoreThanPageLimit_NoRemovals(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed(
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
                Disposable.Empty,
                10,
                1);

            await collection.InitializationCompleted;

            // Act
            for (var i = 0; i <= 900; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }

            // Assert
            Assert.Empty(set);
        }

        [Theory]
        [MemberData(nameof(CombinationsWherePreloading))]
        public async Task With6969ElementsPageSize100Preloading_NotMoreThanPageLimit_NoRemovals(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed(
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
                Disposable.Empty,
                10,
                1);

            await collection.InitializationCompleted;

            // Act
            for (var i = 0; i <= 800; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }

            // Assert
            Assert.Empty(set);
        }

        [Theory]
        [MemberData(nameof(CombinationsWhereNonPreloading))]
        public async Task With6969ElementsPageSize100_NotMoreThanPageLimitAndIterateSameElementsAgain_NoRemovals(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed(
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
                Disposable.Empty,
                10,
                1);

            await collection.InitializationCompleted;

            // Act
            for (var i = 0; i <= 900; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }
            for (var i = 0; i <= 900; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }

            // Assert
            Assert.Empty(set);
        }

        [Theory]
        [MemberData(nameof(CombinationsWherePreloading))]
        public async Task With6969ElementsPageSize100Preloading_NotMoreThanPageLimitAndIterateSameElementsAgain_NoRemovals(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed(
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
                Disposable.Empty,
                10,
                1);

            await collection.InitializationCompleted;

            // Act
            for (var i = 0; i <= 800; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }
            for (var i = 0; i <= 800; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }

            // Assert
            Assert.Empty(set);
        }

        [Theory]
        [MemberData(nameof(CombinationsWhereNonPreloading))]
        public async Task With6969ElementsPageSize100_OneMoreThanPageLimit_FirstPageRemoved(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            int[] expected = Enumerable.Range(0, 100).ToArray();
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed(
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
                Disposable.Empty,
                10,
                1);

            await collection.InitializationCompleted;

            // Act
            for (var i = 0; i <= 1000; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }

            // Assert
            Assert.Equal(set.Count, expected.Length);
            Assert.True(set.IsSubsetOf(expected));

        }

        [Theory]
        [MemberData(nameof(CombinationsWherePreloading))]
        public async Task With6969ElementsPageSize100Preloading_OneMoreThanPageLimit_FirstPageRemoved(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            int[] expected = Enumerable.Range(0, 100).ToArray();
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed(
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
                Disposable.Empty,
                10,
                1);

            await collection.InitializationCompleted;

            // Act
            for (var i = 0; i <= 900; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }

            // Assert
            Assert.Equal(set.Count, expected.Length);
            Assert.True(set.IsSubsetOf(expected));

        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task With6969ElementsPageSize100RemovalCount3_OneMoreThanPageLimit_FirstThreePagesRemoved(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            int[] expected = Enumerable.Range(0, 300).ToArray();
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed(
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
                Disposable.Empty,
                10,
                3);

            await collection.InitializationCompleted;

            // Act
            for (var i = 0; i <= 1000; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }

            // Assert
            Assert.Equal(set.Count, expected.Length);
            Assert.True(set.IsSubsetOf(expected));

        }

        [Theory]
        [MemberData(nameof(CombinationsWherePreloading))]
        public async Task With6969ElementsPageSize100PageLimit1Preloading_FourPagesLoaded_FirstPageRemoved(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            int[] expected = Enumerable.Range(0, 100).ToArray();
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed(
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
                Disposable.Empty,
                1,
                1);

            await collection.InitializationCompleted;

            // Act
            for (var i = 0; i <= 300; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }

            // Assert
            Assert.Equal(set.Count, expected.Length);
            Assert.True(set.IsSubsetOf(expected));
        }

        [Theory]
        [MemberData(nameof(CombinationsWherePreloading))]
        public async Task With6969ElementsPageSize100PageLimit4RemovalCount3Preloading_FourPagesLoaded_FirstPageRemoved(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            int[] expected = Enumerable.Range(0, 100).ToArray();
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed(
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
                Disposable.Empty,
                4,
                3);

            await collection.InitializationCompleted;

            // Act
            for (var i = 0; i <= 300; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }

            // Assert
            Assert.Equal(set.Count, expected.Length);
            Assert.True(set.IsSubsetOf(expected));
        }
    }
}
