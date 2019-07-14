using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageRemoval;

namespace BFF.DataVirtualizingCollection.PageStores
{
    internal abstract class PageStoreBase<T> : ISyncPageStore<T>
    {
        protected readonly int PageSize;
        protected readonly IDictionary<int, T[]> PageStore = new Dictionary<int, T[]>();

        internal PageStoreBase(int pageSize)
        {
            PageSize = pageSize;
        }

        public int Count { get; set; }

        public abstract T Fetch(int index);

        public abstract void Dispose();

        protected int GetPageIndex(int index) => index % PageSize;

        protected int GetPageKey(int index) => index / PageSize;

        protected void RemovePage(int pageKey)
        {
            if (!PageStore.TryGetValue(pageKey, out var page)) return;

            PageDisposal(page);
            PageStore.Remove(pageKey);
        }

        protected void Clear()
        {
            foreach (var page in PageStore.Values.ToList())
            {
                PageDisposal(page);
            }
            PageStore.Clear();
        }

        protected static void PageDisposal(T[] page)
        {
            foreach (var disposable in page.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }

    internal abstract class SyncPageStoreBase<T> : PageStoreBase<T>
    {
        protected readonly ISubject<(int PageKey, int PageIndex)> Requests;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        protected SyncPageStoreBase(
            int pageSize,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
            : base (pageSize)
        {
            Requests = new Subject<(int PageKey, int PageIndex)>().AddTo(_compositeDisposable);
            pageReplacementStrategyFactory(Requests)
                .Subscribe(pageKeysToRemove =>
                {
                    foreach (var pageKey in pageKeysToRemove)
                    {
                        RemovePage(pageKey);
                    }
                },
                exception => throw new PageReplacementStrategyException(
                    "LeastRecentlyUsed strategy: Something unexpected happened during page removal! See inner exception.",
                    exception))
                .AddTo(_compositeDisposable);
        }

        public override T Fetch(int index)
        {
            int pageKey = GetPageKey(index);
            int pageIndex = GetPageIndex(index);

            Requests.OnNext((pageKey, pageIndex));

            if (PageStore.TryGetValue(pageKey, out var page))
            {
                OnPageContained(pageKey);
                return page[pageIndex];
            }
            var requestedElement = OnPageNotContained(pageKey, pageIndex);
            OnPageContained(pageKey);
            return requestedElement;
        }

        protected abstract T OnPageNotContained(int pageKey, int pageIndex);

        protected virtual void OnPageContained(int pageKey)
        {
        }


        public override void Dispose()
        {
            Clear();
            _compositeDisposable.Dispose();
        }
    }

    internal abstract class AsyncPageStoreBase<T> : PageStoreBase<T>, IAsyncPageStore<T>
    {
        private readonly IScheduler _subscribeScheduler;
        protected readonly ISubject<(int PageKey, int PageIndex)> Requests;
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        protected readonly Subject<(T, T, int)> OnCollectionChangedReplaceSubject;

        protected readonly IDictionary<int, ReplaySubject<(int, T)>> DeferredRequests = new Dictionary<int, ReplaySubject<(int, T)>>();

        protected readonly T Placeholder;

        protected bool DisposeOnArrival;

        protected AsyncPageStoreBase(
            int pageSize,
            IPlaceholderFactory<T> placeholderFactory,
            IScheduler subscribeScheduler,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
            : base(pageSize)
        {
            _subscribeScheduler = subscribeScheduler;
            Placeholder = placeholderFactory.CreatePlaceholder();

            OnCollectionChangedReplaceSubject = new Subject<(T, T, int)>().AddTo(CompositeDisposable);
            OnCollectionChangedReplace = OnCollectionChangedReplaceSubject.AsObservable();
            Requests = new Subject<(int PageKey, int PageIndex)>().AddTo(CompositeDisposable);
            pageReplacementStrategyFactory(Requests)
                .Subscribe(pageKeysToRemove =>
                    {
                        foreach (var pageKey in pageKeysToRemove)
                        {
                            RemovePage(pageKey);
                        }
                    },
                    exception => throw new PageReplacementStrategyException(
                        "LeastRecentlyUsed strategy: Something unexpected happened during page removal! See inner exception.",
                        exception))
                .AddTo(CompositeDisposable);
        }

        public override T Fetch(int index)
        {
            int pageKey = GetPageKey(index);
            int pageIndex = GetPageIndex(index);

            Requests.OnNext((pageKey, pageIndex));

            if (PageStore.TryGetValue(pageKey, out var page))
            {
                OnPageContained(pageKey);
                return page[pageIndex];
            }
            return OnPageNotContained(pageKey, pageIndex);
        }

        protected abstract void OnPageContained(int pageKey);

        protected abstract T OnPageNotContained(int pageKey, int pageIndex);


        public override void Dispose()
        {
            DisposeOnArrival = true;
            _subscribeScheduler.MinimalSchedule(() =>
            {
                CompositeDisposable.Dispose();
                Clear();
                foreach (var subject in DeferredRequests.Values)
                {
                    subject.Dispose();
                }
            });
        }

        public IObservable<(T, T, int)> OnCollectionChangedReplace { get; }
    }
}
