using System;
using System.Collections.Generic;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in sync way with task-based data access, which means that it does block the current thread if the element isn't available yet.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    internal interface IHoardingTaskBasedSyncPageStore<out T> : ISyncPageStore<T>
    {
    }
    
    internal class HoardingTaskBasedSyncPageStore<T> : SyncPageStoreBase<T>, IHoardingTaskBasedSyncPageStore<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderOptional<out TItem>
        {
            IBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingTaskBasedSyncPageStore<TItem> Build();
        }

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> With(
                IBasicTaskBasedSyncDataAccess<TItem> dataAccess,
                Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory);
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IBasicTaskBasedSyncDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;
            private Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>>
                _pageReplacementStrategyFactory;

            public IBuilderOptional<TItem> With(
                IBasicTaskBasedSyncDataAccess<TItem> dataAccess,
                Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
            {
                _dataAccess = dataAccess;
                _pageReplacementStrategyFactory = pageReplacementStrategyFactory;
                return this;
            }

            public IBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = pageSize;
                return this;
            }

            public IHoardingTaskBasedSyncPageStore<TItem> Build()
            {
                return new HoardingTaskBasedSyncPageStore<TItem>(_dataAccess, _pageReplacementStrategyFactory)
                {
                    PageSize = _pageSize
                };
            }
        }

        private readonly ITaskBasedPageFetcher<T> _pageFetcher;

        private HoardingTaskBasedSyncPageStore(
            ITaskBasedPageFetcher<T> pageFetcher,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
            : base(pageReplacementStrategyFactory)
        {
            _pageFetcher = pageFetcher;
        }

        protected override T OnPageNotContained(int pageKey, int pageIndex)
        {
            var offset = pageKey * PageSize;
            var actualPageSize = Math.Min(PageSize, Count - offset);
            var page = _pageFetcher.PageFetchAsync(offset, actualPageSize).Result;
            PageStore[pageKey] = page;
            return page[pageIndex];
        }
    }
}