using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.Extensions;

namespace BFF.DataVirtualizingCollection.PageStores
{
    internal abstract class PageStoreBase<T> : ISyncPageStore<T>
    {
        protected int PageSize = 100;
        protected readonly IDictionary<int, T[]> PageStore = new Dictionary<int, T[]>();

        public abstract T Fetch(int index);

        public abstract void Dispose();

        protected int GetPageIndex(int index) => index % PageSize;

        protected int GetPageKey(int index) => index / PageSize;
    }

    internal abstract class SyncPageStoreBase<T> : PageStoreBase<T>
    {
        public override T Fetch(int index)
        {
            int pageKey = GetPageKey(index);
            int pageIndex = GetPageIndex(index);

            if (!PageStore.ContainsKey(pageKey))
            {
                OnPageContained(pageKey, pageIndex);
            }

            return OnPageNotContained(pageKey, pageIndex);
        }

        protected abstract void OnPageContained(int pageKey, int pageIndex);

        protected abstract T OnPageNotContained(int pageKey, int pageIndex);


        public override void Dispose()
        {
            foreach (var disposable in PageStore.SelectMany(ps => ps.Value).OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }

    internal abstract class AsyncPageStoreBase<T> : PageStoreBase<T>, IAsyncPageStore<T>
    {
        protected readonly IScheduler SubscribeScheduler;
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        protected Subject<(T, T, int)> OnCollectionChangedReplaceSubject;

        protected readonly IDictionary<int, ReplaySubject<(int, T)>> DeferredRequests = new Dictionary<int, ReplaySubject<(int, T)>>();

        protected readonly T Placeholder;

        protected AsyncPageStoreBase(
            IPlaceholderFactory<T> placeholderFactory,
            IScheduler subscribeScheduler)
        {
            SubscribeScheduler = subscribeScheduler;
            Placeholder = placeholderFactory.CreatePlaceholder();

            OnCollectionChangedReplaceSubject = new Subject<(T, T, int)>().AddTo(CompositeDisposable);
            OnCollectionChangedReplace = OnCollectionChangedReplaceSubject.AsObservable();
        }

        public override T Fetch(int index)
        {
            int pageKey = GetPageKey(index);
            int pageIndex = GetPageIndex(index);

            if (!PageStore.ContainsKey(pageKey))
            {
                return OnPageNotContained(pageKey, pageIndex);
            }

            return OnPageContained(pageKey, pageIndex);
        }

        protected abstract T OnPageContained(int pageKey, int pageIndex);

        protected abstract T OnPageNotContained(int pageKey, int pageIndex);


        public override void Dispose()
        {
            SubscribeScheduler.MinimalSchedule(() =>
            {
                CompositeDisposable.Dispose();
                foreach (var disposable in PageStore.SelectMany(ps => ps.Value).OfType<IDisposable>())
                {
                    disposable.Dispose();
                }
                foreach (var subject in DeferredRequests.Values)
                {
                    subject.Dispose();
                }
            });
        }

        public IObservable<(T, T, int)> OnCollectionChangedReplace { get; }
    }
}
