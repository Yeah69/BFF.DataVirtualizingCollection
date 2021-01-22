using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection
{
    /// <summary>
    /// Lets you configure the page loading behavior.
    /// Here you can turn the preloading on or off. Preloading means that neighboring pages from requested pages are loaded as well, assuming that they'll be requested soon. 
    /// </summary>
    /// <typeparam name="TItem">Item type.</typeparam>
    /// <typeparam name="TVirtualizationKind">IDataVirtualizingCollection or ISlidingWindow.</typeparam>
    public interface IPageLoadingBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// No preloading. Pages are loaded only as soon as an item of the page is requested.
        /// </summary>
        IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonPreloading();

        /// <summary>
        /// Pages are loaded as soon as an item of the page is requested or as soon as a neighboring page is loaded.
        /// Per default the initially configured background scheduler is taken for the preloads.
        /// </summary>
        /// <param name="preloadingPlaceholderFactory">Initially preloaded pages are filled with placeholders.</param>
        IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> Preloading(
            Func<int, int, TItem> preloadingPlaceholderFactory);

        /// <summary>
        /// Pages are loaded as soon as an item of the page is requested or as soon as a neighboring page is loaded.
        /// </summary>
        /// <param name="preloadingPlaceholderFactory">Initially preloaded pages are filled with placeholders.</param>
        /// <param name="preloadingBackgroundScheduler">A scheduler exclusively for preloading pages.</param>
        IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind> Preloading(
            Func<int, int, TItem> preloadingPlaceholderFactory, 
            IScheduler preloadingBackgroundScheduler);
    }
    
    /// <summary>
    /// Lets you configure the page holding behavior.
    /// </summary>
    /// <typeparam name="TItem">Item type.</typeparam>
    /// <typeparam name="TVirtualizationKind">IDataVirtualizingCollection or ISlidingWindow.</typeparam>
    public interface IPageHoldingBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// In this mode pages are loaded on demand. However, once loaded the pages are hold in memory until the data virtualizing collection is reset or disposed.
        /// </summary>
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> Hoarding();

        /// <summary>
        /// If the page limit is reached then the page which is least recently used will be chosen for removal.
        /// </summary>
        /// <param name="pageLimit">Has to be greater than zero (with preloading greater than two) in order to maintain at least one page in the page store (when preloading is active, then the neighbors of the most recently requested page are maintained as well).</param>
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> LeastRecentlyUsed(
            int pageLimit);

        /// <summary>
        /// If the page limit is reached then the pages (amount: removal buffer plus one) which are least recently used will be chosen for removal.
        /// </summary>
        /// <param name="pageLimit">Has to be greater than zero (with preloading greater than two) in order to maintain at least one page in the page store (when preloading is active, then the neighbors of the most recently requested page are maintained as well).</param>
        /// <param name="removalCount">Has to be in between one and the page limit minus one (so at least one page remains).
        /// With active preloading the removal count cannot be greater than the page limit minus three.</param>
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> LeastRecentlyUsed(
            int pageLimit, 
            int removalCount);

        /// <summary>
        /// With this function you can provide a custom page-removal strategy.
        /// You'll get an observable which emits all element requests in form of a key to the page and the element's index inside of the page.
        /// You'll have to return an observable which emits page-removal requests. You can request to remove several pages at once.
        /// </summary>
        IFetchersKindCollectionBuilder<TItem, TVirtualizationKind> CustomPageRemovalStrategy(
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory);
    }
    
    /// <summary>
    /// Lets you configure the fetcher (page and count) kind and lets you also provide appropriate fetchers as well.
    /// </summary>
    /// <typeparam name="TItem">Item type.</typeparam>
    /// <typeparam name="TVirtualizationKind">IDataVirtualizingCollection or ISlidingWindow.</typeparam>
    public interface IFetchersKindCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// You have to provide non-task-based (synchronous) fetchers.
        /// The page fetcher has to get a page from the backend based on the provided offset and size. The count fetcher has to get the count of the items in the backend.
        /// </summary>
        /// <param name="pageFetcher">First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page from the backend.</param>
        /// <param name="countFetcher">You have to provide a lambda function which gets the count of all elements in the backend.</param>
        IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonTaskBasedFetchers(
            Func<int, int, TItem[]> pageFetcher, 
            Func<int> countFetcher);

        /// <summary>
        /// You have to provide task-based (asynchronous) fetchers.
        /// The page fetcher has to get a page from the backend based on the provided offset and size. The count fetcher has to get the count of the items in the backend.
        /// </summary>
        /// <param name="pageFetcher">First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page from the backend.</param>
        /// <param name="countFetcher">You have to provide a lambda function which gets the count of all elements in the backend.</param>
        IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> TaskBasedFetchers(
            Func<int, int, Task<TItem[]>> pageFetcher, 
            Func<Task<int>> countFetcher);
        
        /// <summary>
        /// You have to provide non-task-based (synchronous) fetchers.
        /// The page fetcher has to get a page from the backend based on the provided offset and size. The count fetcher has to get the count of the items in the backend.
        /// </summary>
        /// <param name="pageFetcher">First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page from the backend.</param>
        /// <param name="countFetcher">You have to provide a lambda function which gets the count of all elements in the backend.</param>
        IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> NonTaskBasedFetchers(
            Func<int, int, CancellationToken, TItem[]> pageFetcher, 
            Func<CancellationToken, int> countFetcher);

        /// <summary>
        /// You have to provide task-based (asynchronous) fetchers.
        /// The page fetcher has to get a page from the backend based on the provided offset and size. The count fetcher has to get the count of the items in the backend.
        /// </summary>
        /// <param name="pageFetcher">First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page from the backend.</param>
        /// <param name="countFetcher">You have to provide a lambda function which gets the count of all elements in the backend.</param>
        IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> TaskBasedFetchers(
            Func<int, int, CancellationToken, Task<TItem[]>> pageFetcher, 
            Func<CancellationToken, Task<int>> countFetcher);
    }
    
    /// <summary>
    /// Lets you configure the asynchronous index access options.
    /// Asynchronous meas the if the page still needs to be loaded a placeholder will be provided. As soon as the page is loaded a notification is emitted which states that the entry of the index arrived.  
    /// </summary>
    /// <typeparam name="TItem">Item type.</typeparam>
    /// <typeparam name="TVirtualizationKind">IDataVirtualizingCollection or ISlidingWindow.</typeparam>
    public interface IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// If item of requested index isn't loaded yet the collections will return a placeholder instead and emit a notification as soon as it arrives.
        /// Per default the initially configured background scheduler is taken for page and count fetches.
        /// </summary>
        /// <param name="placeholderFactory">You have to provide a factory lambda function which returns a placeholder.
        /// The first parameter is the page key (index of pages) and the second is the page index (index of items inside the page).</param>
        TVirtualizationKind AsyncIndexAccess(
            Func<int, int, TItem> placeholderFactory);
        
        /// <summary>
        /// If item of requested index isn't loaded yet the collections will return a placeholder instead and emit a notification as soon as it arrives.
        /// Per default the initially configured background scheduler is taken for count fetches.
        /// </summary>
        /// <param name="placeholderFactory">You have to provide a factory lambda function which returns a placeholder.
        /// The first parameter is the page key (index of pages) and the second is the page index (index of items inside the page).</param>
        /// <param name="pageBackgroundScheduler">A scheduler exclusively for page fetches.</param>
        TVirtualizationKind AsyncIndexAccess(
            Func<int, int, TItem> placeholderFactory,
            IScheduler pageBackgroundScheduler);
        
        /// <summary>
        /// If item of requested index isn't loaded yet the collections will return a placeholder instead and emit a notification as soon as it arrives.
        /// </summary>
        /// <param name="placeholderFactory">You have to provide a factory lambda function which returns a placeholder.
        /// The first parameter is the page key (index of pages) and the second is the page index (index of items inside the page).</param>
        /// <param name="pageBackgroundScheduler">A scheduler exclusively for page fetches.</param>
        /// <param name="countBackgroundScheduler">A scheduler exclusively for count fetches.</param>
        TVirtualizationKind AsyncIndexAccess(
            Func<int, int, TItem> placeholderFactory,
            IScheduler pageBackgroundScheduler,
            IScheduler countBackgroundScheduler);
    }
    
    /// <summary>
    /// Lets you configure whether the index access should be synchronous or asynchronous.
    /// Synchronous means that if the index access will wait actively until the entry is provided even if the page still has to be loaded.
    /// Asynchronous meas the if the page still needs to be loaded a placeholder will be provided. As soon as the page is loaded a notification is emitted which states that the entry of the index arrived.  
    /// </summary>
    /// <typeparam name="TItem">Item type.</typeparam>
    /// <typeparam name="TVirtualizationKind">IDataVirtualizingCollection or ISlidingWindow.</typeparam>
    public interface IIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind> : IAsyncOnlyIndexAccessBehaviorCollectionBuilder<TItem, TVirtualizationKind>
    {
        /// <summary>
        /// If item of requested index isn't loaded yet the collections will wait actively and return as soon as it arrives.
        /// Depending on the latency of the page fetcher it may result in freezes of the application.
        /// </summary>
        TVirtualizationKind SyncIndexAccess();
    }
}