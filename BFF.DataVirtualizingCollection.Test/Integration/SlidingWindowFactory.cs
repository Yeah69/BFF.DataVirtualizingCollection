using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.SlidingWindow;

namespace BFF.DataVirtualizingCollection.Test.Integration
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
            int initialWindowOffset)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder<int>.Build(initialWindowSize, initialWindowOffset, new EventLoopScheduler(), pageSize);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(pageLoadingBehaviorCollectionBuilder, pageLoadingBehavior);
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
            int initialWindowOffset)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder<int>.Build(initialWindowSize, initialWindowOffset, new EventLoopScheduler(), pageSize);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(pageLoadingBehaviorCollectionBuilder, pageLoadingBehavior);
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
            T placeholder)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder<T>.Build(initialWindowSize, initialWindowOffset, new EventLoopScheduler(), pageSize);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(pageLoadingBehaviorCollectionBuilder, pageLoadingBehavior);
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
            int removalCount)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder<T>.Build(initialWindowSize, initialWindowOffset, new EventLoopScheduler(), pageSize);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(pageLoadingBehaviorCollectionBuilder, pageLoadingBehavior);
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
            int initialWindowOffset)
        {
            var pageLoadingBehaviorCollectionBuilder = SlidingWindowBuilder<int>.Build(initialWindowSize, initialWindowOffset, new EventLoopScheduler(), pageSize);
            var pageHoldingBehaviorCollectionBuilder =
                StandardPageHoldingBehaviorCollectionBuilder(pageLoadingBehaviorCollectionBuilder, pageLoadingBehavior);
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

        private static SlidingWindow.IPageHoldingBehaviorCollectionBuilder<T> StandardPageHoldingBehaviorCollectionBuilder<T>(IPageLoadingBehaviorCollectionBuilder<T> pageLoadingBehaviorCollectionBuilder,
            PageLoadingBehavior pageLoadingBehavior) =>
            pageLoadingBehavior switch
                {
                PageLoadingBehavior.NonPreloading => pageLoadingBehaviorCollectionBuilder.NonPreloading(),
                PageLoadingBehavior.Preloading => pageLoadingBehaviorCollectionBuilder.Preloading(),
                _ => throw new Exception("Test configuration failed!")
                };

        private static SlidingWindow.IFetchersKindCollectionBuilder<T> StandardFetcherKindCollectionBuilder<T>(IPageHoldingBehaviorCollectionBuilder<T> pageHoldingBehaviorCollectionBuilder,
            PageRemovalBehavior pageRemovalBehavior,
            int pageLimit,
            int removalCount) =>
            pageRemovalBehavior switch
                {
                PageRemovalBehavior.Hoarding => pageHoldingBehaviorCollectionBuilder.Hoarding(),
                PageRemovalBehavior.LeastRecentlyUsed => pageHoldingBehaviorCollectionBuilder.LeastRecentlyUsed(pageLimit, removalCount),
                _ => throw new Exception("Test configuration failed!")
                };

        private static SlidingWindow.IAsyncOnlyIndexAccessBehaviorCollectionBuilder<T> StandardIndexAccessBehaviorCollectionBuilder<T>(SlidingWindow.IFetchersKindCollectionBuilder<T> fetchersKindCollectionBuilder,
            FetchersKind fetchersKind,
            Func<int, int, T[]> pageFetcher,
            Func<int> countFetcher) =>
            fetchersKind switch
                {
                FetchersKind.NonTaskBased => fetchersKindCollectionBuilder.NonTaskBasedFetchers(
                    (offset, pSize) =>
                    {
                        Thread.Sleep(25);
                        return pageFetcher(offset, pSize);
                    },
                    () =>
                    {
                        Thread.Sleep(25);
                        return countFetcher();
                    }),
                FetchersKind.TaskBased => fetchersKindCollectionBuilder.TaskBasedFetchers(
                    async (offset, pSize) =>
                    {
                        await Task.Delay(25).ConfigureAwait(false);
                        return pageFetcher(offset, pSize);
                    },
                    async () =>
                    {
                        await Task.Delay(25).ConfigureAwait(false);
                        return countFetcher();
                    }),
                _ => throw new Exception("Test configuration failed!")
                };

        private static ISlidingWindow<T> StandardDataVirtualizingCollection<T>(IAsyncOnlyIndexAccessBehaviorCollectionBuilder<T> indexAccessBehaviorCollectionBuilder,
            IndexAccessBehavior indexAccessBehavior,
            Func<T> placeholderFactory) =>
            indexAccessBehavior switch
                {
                IndexAccessBehavior.Synchronous => (indexAccessBehaviorCollectionBuilder as IIndexAccessBehaviorCollectionBuilder<T>)?.SyncIndexAccess(new EventLoopScheduler()) ?? throw new Exception("Task-based fetchers and synchronous access is not allowed."),
                IndexAccessBehavior.Asynchronous => indexAccessBehaviorCollectionBuilder.AsyncIndexAccess(
                    (_, __) => placeholderFactory(),
                    new EventLoopScheduler()),
                _ => throw new Exception("Test configuration failed!")
                };
    }
}
