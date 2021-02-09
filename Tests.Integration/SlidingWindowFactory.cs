using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.SlidingWindow;

namespace BFF.DataVirtualizingCollection.Tests.Integration
{
    internal static class SlidingWindowFactory
    {
        internal static ISlidingWindow<int> CreateCollectionWithIncrementalInteger(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior,
            int count,
            int pageSize,
            int initialWindowSize,
            int initialWindowOffset,
            IScheduler scheduler)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder.Build<int>(
                initialWindowSize, 
                initialWindowOffset,
                pageSize, 
                new EventLoopScheduler(),
                scheduler);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(
                    pageLoadingBehaviorCollectionBuilder, 
                    pageLoadingBehavior,
                    (_, __) => -1);
            var fetchersKindCollectionBuilder =
                StandardFetcherKindCollectionBuilder(
                    pageHoldingBehaviorCollectionBuilder,
                    pageRemovalBehavior,
                    10,
                    1);
            var indexAccessBehaviorCollectionBuilder =
                StandardIndexAccessBehaviorCollectionBuilder(
                    fetchersKindCollectionBuilder,
                    fetchersKind,
                    (offset, pSize) => 
                        Enumerable
                            .Range(offset, pSize)
                            .ToArray(),
                    () => count);

            var dataVirtualizingCollection =
                StandardDataVirtualizingCollection(
                    indexAccessBehaviorCollectionBuilder,
                    indexAccessBehavior,
                    () => -1);
            return dataVirtualizingCollection;
        }
        internal static ISlidingWindow<int> CreateCollectionWithIncrementalIntegerWhereFetchersIgnorePageSize(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior,
            int count,
            int pageSize,
            int initialWindowSize,
            int initialWindowOffset,
            IScheduler scheduler)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder.Build<int>(
                initialWindowSize, 
                initialWindowOffset, 
                pageSize,
                new EventLoopScheduler(),
                scheduler);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(
                    pageLoadingBehaviorCollectionBuilder, 
                    pageLoadingBehavior,
                    (_, __) => -1);
            var fetchersKindCollectionBuilder =
                StandardFetcherKindCollectionBuilder(
                    pageHoldingBehaviorCollectionBuilder,
                    pageRemovalBehavior,
                    10,
                    1);
            var indexAccessBehaviorCollectionBuilder =
                StandardIndexAccessBehaviorCollectionBuilder(
                    fetchersKindCollectionBuilder,
                    fetchersKind,
                    (offset, pSize) => 
                        Enumerable
                            .Range(offset, pageSize) // <--- This is different! pageSize instead of pSize!
                            .ToArray(),
                    () => count);

            var dataVirtualizingCollection =
                StandardDataVirtualizingCollection(
                    indexAccessBehaviorCollectionBuilder,
                    indexAccessBehavior,
                    () => -1);
            return dataVirtualizingCollection;
        }

        internal static ISlidingWindow<T> CreateCollectionWithCustomPageFetchingLogic<T>(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior,
            int count,
            int pageSize,
            int initialWindowSize,
            int initialWindowOffset,
            Func<int, int, T[]> pageFetchingLogic,
            T placeholder,
            IScheduler scheduler)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder.Build<T>(
                initialWindowSize, 
                initialWindowOffset, 
                pageSize, 
                new EventLoopScheduler(),
                scheduler);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(
                    pageLoadingBehaviorCollectionBuilder, 
                    pageLoadingBehavior,
                    (_, __) => placeholder);
            var fetchersKindCollectionBuilder =
                StandardFetcherKindCollectionBuilder(
                    pageHoldingBehaviorCollectionBuilder,
                    pageRemovalBehavior,
                    10,
                    1);
            var indexAccessBehaviorCollectionBuilder =
                StandardIndexAccessBehaviorCollectionBuilder(
                    fetchersKindCollectionBuilder,
                    fetchersKind,
                    pageFetchingLogic,
                    () => count);

            var dataVirtualizingCollection =
                StandardDataVirtualizingCollection(
                    indexAccessBehaviorCollectionBuilder,
                    indexAccessBehavior,
                    () => placeholder);
            return dataVirtualizingCollection;
        }

        internal static ISlidingWindow<T> CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed<T>(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior,
            int count,
            int pageSize,
            int initialWindowSize,
            int initialWindowOffset,
            Func<int, int, T[]> pageFetchingLogic,
            T placeholder,
            int pageLimit,
            int removalCount,
            IScheduler scheduler)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder.Build<T>(
                initialWindowSize,
                initialWindowOffset,
                pageSize,
                new EventLoopScheduler(),
                scheduler);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(
                    pageLoadingBehaviorCollectionBuilder,
                    pageLoadingBehavior,
                    (_, __) => placeholder);
            var fetchersKindCollectionBuilder =
                StandardFetcherKindCollectionBuilder(
                    pageHoldingBehaviorCollectionBuilder, 
                    pageRemovalBehavior,
                    pageLimit,
                    removalCount);
            var indexAccessBehaviorCollectionBuilder =
                StandardIndexAccessBehaviorCollectionBuilder(
                    fetchersKindCollectionBuilder,
                    fetchersKind,
                    pageFetchingLogic,
                    () => count);

            var dataVirtualizingCollection =
                StandardDataVirtualizingCollection(
                    indexAccessBehaviorCollectionBuilder,
                    indexAccessBehavior,
                    () => placeholder);
            return dataVirtualizingCollection;
        }
        
        internal static ISlidingWindow<int> CreateCollectionWithCustomCountFetcher(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior,
            Func<int> countFetcher,
            int pageSize,
            int initialWindowSize,
            int initialWindowOffset,
            IScheduler scheduler)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder.Build<int>(
                initialWindowSize, 
                initialWindowOffset, pageSize, 
                new EventLoopScheduler(),
                scheduler);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(
                    pageLoadingBehaviorCollectionBuilder, 
                    pageLoadingBehavior,
                    (_, __) => -1);
            var fetchersKindCollectionBuilder =
                StandardFetcherKindCollectionBuilder(
                    pageHoldingBehaviorCollectionBuilder,
                    pageRemovalBehavior,
                    10,
                    1);
            var indexAccessBehaviorCollectionBuilder =
                StandardIndexAccessBehaviorCollectionBuilder(
                    fetchersKindCollectionBuilder,
                    fetchersKind,
                    (offset, pSize) => 
                        Enumerable
                            .Range(offset, pSize)
                            .ToArray(),
                    countFetcher);

            var dataVirtualizingCollection =
                StandardDataVirtualizingCollection(
                    indexAccessBehaviorCollectionBuilder,
                    indexAccessBehavior,
                    () => -1);
            return dataVirtualizingCollection;
        }

        private static IPageHoldingBehaviorCollectionBuilder<T, ISlidingWindow<T>> StandardPageHoldingBehaviorCollectionBuilder<T>(
            IPageLoadingBehaviorCollectionBuilder<T, ISlidingWindow<T>> pageLoadingBehaviorCollectionBuilder,
            PageLoadingBehavior pageLoadingBehavior,
            Func<int, int, T> preloadingPlaceholderFactory) =>
            pageLoadingBehavior switch
                {
                PageLoadingBehavior.NonPreloading => pageLoadingBehaviorCollectionBuilder.NonPreloading(),
                PageLoadingBehavior.Preloading => pageLoadingBehaviorCollectionBuilder.Preloading(preloadingPlaceholderFactory),
                _ => throw new Exception("Test configuration failed!")
                };

        private static IFetchersKindCollectionBuilder<T, ISlidingWindow<T>> StandardFetcherKindCollectionBuilder<T>(IPageHoldingBehaviorCollectionBuilder<T, ISlidingWindow<T>> pageHoldingBehaviorCollectionBuilder,
            PageRemovalBehavior pageRemovalBehavior,
            int pageLimit,
            int removalCount) =>
            pageRemovalBehavior switch
                {
                PageRemovalBehavior.Hoarding => pageHoldingBehaviorCollectionBuilder.Hoarding(),
                PageRemovalBehavior.LeastRecentlyUsed => pageHoldingBehaviorCollectionBuilder.LeastRecentlyUsed(pageLimit, removalCount),
                _ => throw new Exception("Test configuration failed!")
                };

        private static IAsyncOnlyIndexAccessBehaviorCollectionBuilder<T, ISlidingWindow<T>> StandardIndexAccessBehaviorCollectionBuilder<T>(IFetchersKindCollectionBuilder<T, ISlidingWindow<T>> fetchersKindCollectionBuilder,
            FetchersKind fetchersKind,
            Func<int, int, T[]> pageFetcher,
            Func<int> countFetcher) =>
            fetchersKind switch
                {
                FetchersKind.NonTaskBased => fetchersKindCollectionBuilder.NonTaskBasedFetchers(
                    (offset, pSize, _) =>
                    {
                        Thread.Sleep(25);
                        return pageFetcher(offset, pSize);
                    },
                    _ =>
                    {
                        Thread.Sleep(25);
                        return countFetcher();
                    }),
                FetchersKind.TaskBased => fetchersKindCollectionBuilder.TaskBasedFetchers(
                    async (offset, pSize, _) =>
                    {
                        await Task.Delay(TimeSpan.FromTicks(1)).ConfigureAwait(false);
                        return pageFetcher(offset, pSize);
                    },
                    async _ =>
                    {
                        await Task.Delay(TimeSpan.FromTicks(1)).ConfigureAwait(false);
                        return countFetcher();
                    }),
                _ => throw new Exception("Test configuration failed!")
                };

        private static ISlidingWindow<T> StandardDataVirtualizingCollection<T>(IAsyncOnlyIndexAccessBehaviorCollectionBuilder<T, ISlidingWindow<T>> indexAccessBehaviorCollectionBuilder,
            IndexAccessBehavior indexAccessBehavior,
            Func<T> placeholderFactory) =>
            indexAccessBehavior switch
                {
                IndexAccessBehavior.Synchronous => 
                (indexAccessBehaviorCollectionBuilder as IIndexAccessBehaviorCollectionBuilder<T, ISlidingWindow<T>>)
                    ?.SyncIndexAccess() ?? throw new Exception("Task-based fetchers and synchronous access is not allowed."),
                IndexAccessBehavior.Asynchronous => indexAccessBehaviorCollectionBuilder.AsyncIndexAccess(
                    (_, __) => placeholderFactory()),
                _ => throw new Exception("Test configuration failed!")
                };
    }
}
