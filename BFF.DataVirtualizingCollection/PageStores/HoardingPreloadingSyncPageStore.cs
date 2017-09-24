using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.PageStores
{
    /// <summary>
    /// Operates in sync way, which means that it does block the current thread if the element isn't available yet.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    public interface IHoardingPreloadingSyncPageStore<T> : ISyncPageStore<T>
    {
    }

    /// <inheritdoc />
    internal class HoardingPreloadingSyncPageStore<T> : IHoardingPreloadingSyncPageStore<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderOptional<TItem>
        {
            IBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingPreloadingSyncPageStore<TItem> Build();
        }

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> With(IBasicSyncDataAccess<TItem> dataAccess);
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IBasicSyncDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;

            public IBuilderOptional<TItem> With(IBasicSyncDataAccess<TItem> dataAccess)
            {
                _dataAccess = dataAccess;
                return this;
            }

            public IBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = pageSize;
                return this;
            }

            public IHoardingPreloadingSyncPageStore<TItem> Build()
            {
                return new HoardingPreloadingSyncPageStore<TItem>(_dataAccess)
                {
                    _pageSize = _pageSize
                };
            }
        }

        private readonly IPageFetcher<T> _pageFetcher;
        private int _pageSize = 100;

        private readonly IDictionary<int, Task<T[]>> _preloadingTasks = new Dictionary<int, Task<T[]>>();
        
        
        private readonly IDictionary<int, T[]> _pageStore = new Dictionary<int, T[]>();

        private HoardingPreloadingSyncPageStore(IPageFetcher<T> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public T Fetch(int index)
        {
            int pageKey = index / _pageSize;
            int pageIndex = index % _pageSize;

            if (!_pageStore.ContainsKey(pageKey))
            {
                if (_preloadingTasks.ContainsKey(pageKey))
                {
                    _preloadingTasks[pageKey].Wait();
                    if(_preloadingTasks[pageKey].IsFaulted || _preloadingTasks[pageKey].IsCanceled)
                        _pageStore[pageKey] = _pageFetcher.PageFetch(pageKey * _pageSize, _pageSize);
                    _preloadingTasks.Remove(pageKey);
                }
                else
                    _pageStore[pageKey] = _pageFetcher.PageFetch(pageKey * _pageSize, _pageSize);
            }

            int nextPageKey = pageKey + 1;
            if (!_pageStore.ContainsKey(nextPageKey) && !_preloadingTasks.ContainsKey(nextPageKey))
                _preloadingTasks[nextPageKey] = Task.Run(() => 
                    _pageStore[nextPageKey] = _pageFetcher.PageFetch(nextPageKey * _pageSize, _pageSize));

            int previousPageKey = pageKey - 1;
            if (previousPageKey >= 0 && !_pageStore.ContainsKey(previousPageKey) && !_preloadingTasks.ContainsKey(previousPageKey))
                _preloadingTasks[previousPageKey] = Task.Run(() =>
                    _pageStore[previousPageKey] = _pageFetcher.PageFetch(previousPageKey * _pageSize, _pageSize));

            return _pageStore[pageKey][pageIndex];
        }

        public void Dispose()
        {
            foreach (var disposable in _pageStore.SelectMany(ps => ps.Value).OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}