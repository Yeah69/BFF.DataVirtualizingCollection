using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;

namespace BFF.DataVirtualizingCollection.Test.Integration
{
    internal static class DataVirtualizingCollectionFactory
    {
        internal static IDataVirtualizingCollection<int> CreateCollectionWithIncrementalInteger(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior,
            int count,
            int pageSize)
        {
            var pageLoadingBehaviorCollectionBuilder = DataVirtualizingCollectionBuilder.Build<int>(pageSize, new EventLoopScheduler());
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
        internal static IDataVirtualizingCollection<int> CreateCollectionWithIncrementalIntegerWhereFetchersIgnorePageSize(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior,
            int count,
            int pageSize)
        {
            var pageLoadingBehaviorCollectionBuilder = DataVirtualizingCollectionBuilder.Build<int>(pageSize, new EventLoopScheduler());
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

        internal static IDataVirtualizingCollection<T> CreateCollectionWithCustomPageFetchingLogic<T>(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior,
            int count,
            int pageSize,
            Func<int, int, T[]> pageFetchingLogic,
            T placeholder)
        {
            var pageLoadingBehaviorCollectionBuilder = DataVirtualizingCollectionBuilder.Build<T>(pageSize, new EventLoopScheduler());
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

        internal static IDataVirtualizingCollection<T> CreateCollectionWithCustomPageFetchingLogicAndCustomLeastRecentlyUsed<T>(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior,
            int count,
            int pageSize,
            Func<int, int, T[]> pageFetchingLogic,
            T placeholder,
            int pageLimit,
            int removalCount)
        {
            var pageLoadingBehaviorCollectionBuilder = DataVirtualizingCollectionBuilder.Build<T>(pageSize, new EventLoopScheduler());
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

        private static IPageHoldingBehaviorCollectionBuilder<T, IDataVirtualizingCollection<T>> StandardPageHoldingBehaviorCollectionBuilder<T>(
            IPageLoadingBehaviorCollectionBuilder<T, IDataVirtualizingCollection<T>> pageLoadingBehaviorCollectionBuilder,
            PageLoadingBehavior pageLoadingBehavior,
            Func<int, int, T> preloadingPlaceholderFactory) =>
            pageLoadingBehavior switch
                {
                PageLoadingBehavior.NonPreloading => pageLoadingBehaviorCollectionBuilder.NonPreloading(),
                PageLoadingBehavior.Preloading => pageLoadingBehaviorCollectionBuilder.Preloading(preloadingPlaceholderFactory),
                _ => throw new Exception("Test configuration failed!")
                };

        private static IFetchersKindCollectionBuilder<T, IDataVirtualizingCollection<T>> StandardFetcherKindCollectionBuilder<T>(IPageHoldingBehaviorCollectionBuilder<T, IDataVirtualizingCollection<T>> pageHoldingBehaviorCollectionBuilder,
            PageRemovalBehavior pageRemovalBehavior,
            int pageLimit,
            int removalCount) =>
            pageRemovalBehavior switch
                {
                PageRemovalBehavior.Hoarding => pageHoldingBehaviorCollectionBuilder.Hoarding(),
                PageRemovalBehavior.LeastRecentlyUsed => pageHoldingBehaviorCollectionBuilder.LeastRecentlyUsed(pageLimit, removalCount),
                _ => throw new Exception("Test configuration failed!")
                };

        private static IAsyncOnlyIndexAccessBehaviorCollectionBuilder<T, IDataVirtualizingCollection<T>> StandardIndexAccessBehaviorCollectionBuilder<T>(IFetchersKindCollectionBuilder<T, IDataVirtualizingCollection<T>> fetchersKindCollectionBuilder,
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

        private static IDataVirtualizingCollection<T> StandardDataVirtualizingCollection<T>(IAsyncOnlyIndexAccessBehaviorCollectionBuilder<T, IDataVirtualizingCollection<T>> indexAccessBehaviorCollectionBuilder,
            IndexAccessBehavior indexAccessBehavior,
            Func<T> placeholderFactory) =>
            indexAccessBehavior switch
                {
                IndexAccessBehavior.Synchronous => (indexAccessBehaviorCollectionBuilder as IIndexAccessBehaviorCollectionBuilder<T, IDataVirtualizingCollection<T>>)?.SyncIndexAccess() ?? throw new Exception("Task-based fetchers and synchronous access is not allowed."),
                IndexAccessBehavior.Asynchronous => indexAccessBehaviorCollectionBuilder.AsyncIndexAccess(
                    (_, __) => placeholderFactory()),
                _ => throw new Exception("Test configuration failed!")
                };
    }
}
