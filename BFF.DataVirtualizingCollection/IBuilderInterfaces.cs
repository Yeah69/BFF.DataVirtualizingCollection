using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection
{
    /// <summary>
    /// Lets you configure the page loading behavior.
    /// Here you can turn the preloading on or off. Preloading means that neighboring pages from requested pages are loaded as well, assuming that they'll be requested soon. 
    /// </summary>
    /// <typeparam name="TItem">Type of the collection items.</typeparam>
    public interface IPageLoadingBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// Pages are loaded only as soon as an item of the page is requested.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonPreloading();

        /// <summary>
        /// Pages are loaded as soon as an item of the page is requested or as soon as a neighboring page is loaded.
        /// Per default the TaskPoolScheduler is taken for the preloads.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> Preloading(Func<int, int, TItem> preloadingPlaceholderFactory);

        /// <summary>
        /// Pages are loaded as soon as an item of the page is requested or as soon as a neighboring page is loaded.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> Preloading(Func<int, int, TItem> preloadingPlaceholderFactory, IScheduler scheduler);
    }
    
    /// <summary>
    /// Lets you configure the page holding behavior.
    /// At the moment only one strategy (hoarding) is available.
    /// As further strategy get implement they will appear here as a choice.
    /// </summary>
    /// <typeparam name="TItem">Type of the collection items.</typeparam>
    public interface IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// In this mode pages are loaded on demand. However, once loaded the pages are hold in memory until the data virtualizing collection is disposed or garbage collected.
        /// </summary>
        /// <returns>The builder itself.</returns>
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> Hoarding();

        /// <summary>
        /// If the page limit is reached then the page which is least recently used will be chosen for removal.
        /// </summary>
        /// <param name="pageLimit">Has to be greater than zero (with preloading greater than two) in order to maintain at least one page in the page store (when preloading is active, then the neighbors of the most recently requested page are maintained as well).</param>
        /// <returns>The builder itself.</returns>
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> LeastRecentlyUsed(int pageLimit);

        /// <summary>
        /// If the page limit is reached then the pages (amount: removal buffer plus one) which are least recently used will be chosen for removal.
        /// </summary>
        /// <param name="pageLimit">Has to be greater than zero (with preloading greater than two) in order to maintain at least one page in the page store (when preloading is active, then the neighbors of the most recently requested page are maintained as well).</param>
        /// <param name="removalCount">Has to be in between one and the page limit minus one (so at least one page remains).
        /// With active preloading the removal count cannot be greater than the page limit minus three.</param>
        /// <returns>The builder itself.</returns>
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> LeastRecentlyUsed(int pageLimit, int removalCount);

        /// <summary>
        /// With this function you can provide an own page-removal strategy.
        /// You'll get an observable which emits all element requests in form of a key to the page and the element's index inside of the page.
        /// You'll have to return an observable which emits page-removal requests. You can request to remove several pages at once.
        /// </summary>
        /// <param name="pageReplacementStrategyFactory"></param>
        /// <returns>The builder itself.</returns>
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> CustomPageRemovalStrategy(
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>>
                pageReplacementStrategyFactory);
    }
    
    /// <summary>
    /// Lets you configure the fetcher (page and count) kind and lets you also provide appropriate fetchers as well.
    /// The page fetcher gets a page based on the provided offset and size. The count fetcher gets the count of the data virtualizing collection.
    /// </summary>
    /// <typeparam name="TItem">Type of the collection items.</typeparam>
    public interface IFetchersKindCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// You have to provide non-task-based (synchronous) fetchers.
        /// The page fetcher gets a page based on the provided offset and size. The count fetcher gets the count of the data virtualizing collection.
        /// </summary>
        /// <param name="pageFetcher">First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page.</param>
        /// <param name="countFetcher">You have to provide a lambda function which gets the count of all elements in the data virtualized collection.</param>
        /// <returns>The builder itself.</returns>
        IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonTaskBasedFetchers(Func<int, int, TItem[]> pageFetcher, Func<int> countFetcher);

        /// <summary>
        /// You have to provide task-based (asynchronous) fetchers.
        /// The page fetcher gets a page based on the provided offset and size. The count fetcher gets the count of the data virtualizing collection.
        /// </summary>
        /// <param name="pageFetcher">First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page.</param>
        /// <param name="countFetcher">You have to provide a lambda function which gets the count of all elements in the data virtualized collection.</param>
        /// <returns>The builder itself.</returns>
        IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> TaskBasedFetchers(Func<int, int, Task<TItem[]>> pageFetcher, Func<Task<int>> countFetcher);
    }
    
    /// <summary>
    /// Lets you configure whether the index access should be synchronous or asynchronous.
    /// Synchronous means that if the index access will wait actively until the entry is provided even if the page still has to be loaded.
    /// Asynchronous meas the if the page still needs to be loaded a placeholder for the indexed access is provided, as soon as the page is loaded a notification is emitted which states that the entry of the index arrived.  
    /// </summary>
    /// <typeparam name="TItem">Type of the collection items.</typeparam>
    public interface IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// If item of requested index isn't loaded yet the collections will return a placeholder instead and emit a notification as soon as it arrives.
        /// </summary>
        /// <param name="placeholderFactory">You have to provide a factory lambda function which returns a placeholder.
        /// The first parameter is the page key (index of pages) and the second is the page index (index of items inside the page).</param>
        /// <param name="backgroundScheduler">Scheduler for all background operations.</param>
        /// <param name="notificationScheduler">Scheduler on which the notifications are emitted.</param>
        /// <returns></returns>
        TVirtualizationKind AsyncIndexAccess(Func<int, int, TItem> placeholderFactory);
    }
    
    /// <summary>
    /// Lets you configure whether the index access should be synchronous or asynchronous.
    /// Synchronous means that if the index access will wait actively until the entry is provided even if the page still has to be loaded.
    /// Asynchronous meas the if the page still needs to be loaded a placeholder for the indexed access is provided, as soon as the page is loaded a notification is emitted which states that the entry of the index arrived.  
    /// </summary>
    /// <typeparam name="TItem">Type of the collection items.</typeparam>
    public interface IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> : IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// If item of requested index isn't loaded yet the collections will wait actively and return as soon as it arrives.
        /// </summary>
        /// <returns>The builder itself.</returns>
        TVirtualizationKind SyncIndexAccess();
    }
}