<a name='assembly'></a>
# BFF.DataVirtualizingCollection

## Contents

- [DataVirtualizingCollectionBuilder](#T-BFF-DataVirtualizingCollection-DataVirtualizingCollection-DataVirtualizingCollectionBuilder 'BFF.DataVirtualizingCollection.DataVirtualizingCollection.DataVirtualizingCollectionBuilder')
  - [Build\`\`1(notificationScheduler)](#M-BFF-DataVirtualizingCollection-DataVirtualizingCollection-DataVirtualizingCollectionBuilder-Build``1-System-Reactive-Concurrency-IScheduler- 'BFF.DataVirtualizingCollection.DataVirtualizingCollection.DataVirtualizingCollectionBuilder.Build``1(System.Reactive.Concurrency.IScheduler)')
  - [Build\`\`1(pageSize,notificationScheduler)](#M-BFF-DataVirtualizingCollection-DataVirtualizingCollection-DataVirtualizingCollectionBuilder-Build``1-System-Int32,System-Reactive-Concurrency-IScheduler- 'BFF.DataVirtualizingCollection.DataVirtualizingCollection.DataVirtualizingCollectionBuilder.Build``1(System.Int32,System.Reactive.Concurrency.IScheduler)')
  - [Build\`\`1(pageSize,notificationScheduler,backgroundScheduler)](#M-BFF-DataVirtualizingCollection-DataVirtualizingCollection-DataVirtualizingCollectionBuilder-Build``1-System-Int32,System-Reactive-Concurrency-IScheduler,System-Reactive-Concurrency-IScheduler- 'BFF.DataVirtualizingCollection.DataVirtualizingCollection.DataVirtualizingCollectionBuilder.Build``1(System.Int32,System.Reactive.Concurrency.IScheduler,System.Reactive.Concurrency.IScheduler)')
- [IAsyncOnlyIndexAccessBehaviorCollectionBuilder\`2](#T-BFF-DataVirtualizingCollection-IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2 'BFF.DataVirtualizingCollection.IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2')
  - [AsyncIndexAccess(placeholderFactory)](#M-BFF-DataVirtualizingCollection-IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2-AsyncIndexAccess-System-Func{System-Int32,System-Int32,`0}- 'BFF.DataVirtualizingCollection.IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2.AsyncIndexAccess(System.Func{System.Int32,System.Int32,`0})')
  - [AsyncIndexAccess(placeholderFactory,pageBackgroundScheduler)](#M-BFF-DataVirtualizingCollection-IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2-AsyncIndexAccess-System-Func{System-Int32,System-Int32,`0},System-Reactive-Concurrency-IScheduler- 'BFF.DataVirtualizingCollection.IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2.AsyncIndexAccess(System.Func{System.Int32,System.Int32,`0},System.Reactive.Concurrency.IScheduler)')
  - [AsyncIndexAccess(placeholderFactory,pageBackgroundScheduler,countBackgroundScheduler)](#M-BFF-DataVirtualizingCollection-IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2-AsyncIndexAccess-System-Func{System-Int32,System-Int32,`0},System-Reactive-Concurrency-IScheduler,System-Reactive-Concurrency-IScheduler- 'BFF.DataVirtualizingCollection.IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2.AsyncIndexAccess(System.Func{System.Int32,System.Int32,`0},System.Reactive.Concurrency.IScheduler,System.Reactive.Concurrency.IScheduler)')
- [IDataVirtualizingCollection](#T-BFF-DataVirtualizingCollection-DataVirtualizingCollection-IDataVirtualizingCollection 'BFF.DataVirtualizingCollection.DataVirtualizingCollection.IDataVirtualizingCollection')
- [IDataVirtualizingCollection\`1](#T-BFF-DataVirtualizingCollection-DataVirtualizingCollection-IDataVirtualizingCollection`1 'BFF.DataVirtualizingCollection.DataVirtualizingCollection.IDataVirtualizingCollection`1')
- [IFetchersKindCollectionBuilder\`2](#T-BFF-DataVirtualizingCollection-IFetchersKindCollectionBuilder`2 'BFF.DataVirtualizingCollection.IFetchersKindCollectionBuilder`2')
  - [NonTaskBasedFetchers(pageFetcher,countFetcher)](#M-BFF-DataVirtualizingCollection-IFetchersKindCollectionBuilder`2-NonTaskBasedFetchers-System-Func{System-Int32,System-Int32,`0[]},System-Func{System-Int32}- 'BFF.DataVirtualizingCollection.IFetchersKindCollectionBuilder`2.NonTaskBasedFetchers(System.Func{System.Int32,System.Int32,`0[]},System.Func{System.Int32})')
  - [TaskBasedFetchers(pageFetcher,countFetcher)](#M-BFF-DataVirtualizingCollection-IFetchersKindCollectionBuilder`2-TaskBasedFetchers-System-Func{System-Int32,System-Int32,System-Threading-Tasks-Task{`0[]}},System-Func{System-Threading-Tasks-Task{System-Int32}}- 'BFF.DataVirtualizingCollection.IFetchersKindCollectionBuilder`2.TaskBasedFetchers(System.Func{System.Int32,System.Int32,System.Threading.Tasks.Task{`0[]}},System.Func{System.Threading.Tasks.Task{System.Int32}})')
- [IIndexAccessBehaviorCollectionBuilder\`2](#T-BFF-DataVirtualizingCollection-IIndexAccessBehaviorCollectionBuilder`2 'BFF.DataVirtualizingCollection.IIndexAccessBehaviorCollectionBuilder`2')
  - [SyncIndexAccess()](#M-BFF-DataVirtualizingCollection-IIndexAccessBehaviorCollectionBuilder`2-SyncIndexAccess 'BFF.DataVirtualizingCollection.IIndexAccessBehaviorCollectionBuilder`2.SyncIndexAccess')
- [IPageHoldingBehaviorCollectionBuilder\`2](#T-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2 'BFF.DataVirtualizingCollection.IPageHoldingBehaviorCollectionBuilder`2')
  - [CustomPageRemovalStrategy()](#M-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2-CustomPageRemovalStrategy-System-Func{System-IObservable{System-ValueTuple{System-Int32,System-Int32}},System-IObservable{System-Collections-Generic-IReadOnlyList{System-Int32}}}- 'BFF.DataVirtualizingCollection.IPageHoldingBehaviorCollectionBuilder`2.CustomPageRemovalStrategy(System.Func{System.IObservable{System.ValueTuple{System.Int32,System.Int32}},System.IObservable{System.Collections.Generic.IReadOnlyList{System.Int32}}})')
  - [Hoarding()](#M-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2-Hoarding 'BFF.DataVirtualizingCollection.IPageHoldingBehaviorCollectionBuilder`2.Hoarding')
  - [LeastRecentlyUsed(pageLimit)](#M-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2-LeastRecentlyUsed-System-Int32- 'BFF.DataVirtualizingCollection.IPageHoldingBehaviorCollectionBuilder`2.LeastRecentlyUsed(System.Int32)')
  - [LeastRecentlyUsed(pageLimit,removalCount)](#M-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2-LeastRecentlyUsed-System-Int32,System-Int32- 'BFF.DataVirtualizingCollection.IPageHoldingBehaviorCollectionBuilder`2.LeastRecentlyUsed(System.Int32,System.Int32)')
- [IPageLoadingBehaviorCollectionBuilder\`2](#T-BFF-DataVirtualizingCollection-IPageLoadingBehaviorCollectionBuilder`2 'BFF.DataVirtualizingCollection.IPageLoadingBehaviorCollectionBuilder`2')
  - [NonPreloading()](#M-BFF-DataVirtualizingCollection-IPageLoadingBehaviorCollectionBuilder`2-NonPreloading 'BFF.DataVirtualizingCollection.IPageLoadingBehaviorCollectionBuilder`2.NonPreloading')
  - [Preloading(preloadingPlaceholderFactory)](#M-BFF-DataVirtualizingCollection-IPageLoadingBehaviorCollectionBuilder`2-Preloading-System-Func{System-Int32,System-Int32,`0}- 'BFF.DataVirtualizingCollection.IPageLoadingBehaviorCollectionBuilder`2.Preloading(System.Func{System.Int32,System.Int32,`0})')
  - [Preloading(preloadingPlaceholderFactory,preloadingBackgroundScheduler)](#M-BFF-DataVirtualizingCollection-IPageLoadingBehaviorCollectionBuilder`2-Preloading-System-Func{System-Int32,System-Int32,`0},System-Reactive-Concurrency-IScheduler- 'BFF.DataVirtualizingCollection.IPageLoadingBehaviorCollectionBuilder`2.Preloading(System.Func{System.Int32,System.Int32,`0},System.Reactive.Concurrency.IScheduler)')
- [ISlidingWindow](#T-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow')
  - [MaximumOffset](#P-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-MaximumOffset 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.MaximumOffset')
  - [Offset](#P-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-Offset 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.Offset')
  - [DecreaseWindowSize()](#M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-DecreaseWindowSize 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.DecreaseWindowSize')
  - [DecreaseWindowSizeBy()](#M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-DecreaseWindowSizeBy-System-Int32- 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.DecreaseWindowSizeBy(System.Int32)')
  - [IncreaseWindowSize()](#M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-IncreaseWindowSize 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.IncreaseWindowSize')
  - [IncreaseWindowSizeBy()](#M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-IncreaseWindowSizeBy-System-Int32- 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.IncreaseWindowSizeBy(System.Int32)')
  - [JumpTo()](#M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-JumpTo-System-Int32- 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.JumpTo(System.Int32)')
  - [SetWindowSizeTo()](#M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-SetWindowSizeTo-System-Int32- 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.SetWindowSizeTo(System.Int32)')
  - [SlideLeft()](#M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-SlideLeft 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.SlideLeft')
  - [SlideRight()](#M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-SlideRight 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.SlideRight')
- [ISlidingWindow\`1](#T-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow`1 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow`1')
- [IVirtualizationBase](#T-BFF-DataVirtualizingCollection-IVirtualizationBase 'BFF.DataVirtualizingCollection.IVirtualizationBase')
  - [InitializationCompleted](#P-BFF-DataVirtualizingCollection-IVirtualizationBase-InitializationCompleted 'BFF.DataVirtualizingCollection.IVirtualizationBase.InitializationCompleted')
  - [SelectedIndex](#P-BFF-DataVirtualizingCollection-IVirtualizationBase-SelectedIndex 'BFF.DataVirtualizingCollection.IVirtualizationBase.SelectedIndex')
  - [Reset()](#M-BFF-DataVirtualizingCollection-IVirtualizationBase-Reset 'BFF.DataVirtualizingCollection.IVirtualizationBase.Reset')
- [IVirtualizationBase\`1](#T-BFF-DataVirtualizingCollection-IVirtualizationBase`1 'BFF.DataVirtualizingCollection.IVirtualizationBase`1')
  - [Count](#P-BFF-DataVirtualizingCollection-IVirtualizationBase`1-Count 'BFF.DataVirtualizingCollection.IVirtualizationBase`1.Count')
  - [Item](#P-BFF-DataVirtualizingCollection-IVirtualizationBase`1-Item-System-Int32- 'BFF.DataVirtualizingCollection.IVirtualizationBase`1.Item(System.Int32)')
- [PageReplacementStrategyException](#T-BFF-DataVirtualizingCollection-PageRemoval-PageReplacementStrategyException 'BFF.DataVirtualizingCollection.PageRemoval.PageReplacementStrategyException')
- [SlidingWindowBuilder](#T-BFF-DataVirtualizingCollection-SlidingWindow-SlidingWindowBuilder 'BFF.DataVirtualizingCollection.SlidingWindow.SlidingWindowBuilder')
  - [Build\`\`1(windowSize,initialOffset,notificationScheduler)](#M-BFF-DataVirtualizingCollection-SlidingWindow-SlidingWindowBuilder-Build``1-System-Int32,System-Int32,System-Reactive-Concurrency-IScheduler- 'BFF.DataVirtualizingCollection.SlidingWindow.SlidingWindowBuilder.Build``1(System.Int32,System.Int32,System.Reactive.Concurrency.IScheduler)')
  - [Build\`\`1(windowSize,initialOffset,pageSize,notificationScheduler)](#M-BFF-DataVirtualizingCollection-SlidingWindow-SlidingWindowBuilder-Build``1-System-Int32,System-Int32,System-Int32,System-Reactive-Concurrency-IScheduler- 'BFF.DataVirtualizingCollection.SlidingWindow.SlidingWindowBuilder.Build``1(System.Int32,System.Int32,System.Int32,System.Reactive.Concurrency.IScheduler)')
  - [Build\`\`1(windowSize,initialOffset,pageSize,notificationScheduler,backgroundScheduler)](#M-BFF-DataVirtualizingCollection-SlidingWindow-SlidingWindowBuilder-Build``1-System-Int32,System-Int32,System-Int32,System-Reactive-Concurrency-IScheduler,System-Reactive-Concurrency-IScheduler- 'BFF.DataVirtualizingCollection.SlidingWindow.SlidingWindowBuilder.Build``1(System.Int32,System.Int32,System.Int32,System.Reactive.Concurrency.IScheduler,System.Reactive.Concurrency.IScheduler)')

<a name='T-BFF-DataVirtualizingCollection-DataVirtualizingCollection-DataVirtualizingCollectionBuilder'></a>
## DataVirtualizingCollectionBuilder `type`

##### Namespace

BFF.DataVirtualizingCollection.DataVirtualizingCollection

##### Summary

Initial entry point for creating a data virtualizing collection.

<a name='M-BFF-DataVirtualizingCollection-DataVirtualizingCollection-DataVirtualizingCollectionBuilder-Build``1-System-Reactive-Concurrency-IScheduler-'></a>
### Build\`\`1(notificationScheduler) `method`

##### Summary

Use to configure general virtualization settings.
Further settings are applied via method chaining.
Page size is set to the default value 100.
The background scheduler is per default the [TaskPoolScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.TaskPoolScheduler 'System.Reactive.Concurrency.TaskPoolScheduler').

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| notificationScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler for sending the notifications ([INotifyCollectionChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Specialized.INotifyCollectionChanged 'System.Collections.Specialized.INotifyCollectionChanged'), [INotifyPropertyChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ComponentModel.INotifyPropertyChanged 'System.ComponentModel.INotifyPropertyChanged')). |

<a name='M-BFF-DataVirtualizingCollection-DataVirtualizingCollection-DataVirtualizingCollectionBuilder-Build``1-System-Int32,System-Reactive-Concurrency-IScheduler-'></a>
### Build\`\`1(pageSize,notificationScheduler) `method`

##### Summary

Use to configure general virtualization settings.
Further settings are applied via method chaining.
The background scheduler is per default the [TaskPoolScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.TaskPoolScheduler 'System.Reactive.Concurrency.TaskPoolScheduler').

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pageSize | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Maximum size of a single page. |
| notificationScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler for sending the notifications ([INotifyCollectionChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Specialized.INotifyCollectionChanged 'System.Collections.Specialized.INotifyCollectionChanged'), [INotifyPropertyChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ComponentModel.INotifyPropertyChanged 'System.ComponentModel.INotifyPropertyChanged')). |

<a name='M-BFF-DataVirtualizingCollection-DataVirtualizingCollection-DataVirtualizingCollectionBuilder-Build``1-System-Int32,System-Reactive-Concurrency-IScheduler,System-Reactive-Concurrency-IScheduler-'></a>
### Build\`\`1(pageSize,notificationScheduler,backgroundScheduler) `method`

##### Summary

Use to configure general virtualization settings.
Further settings are applied via method chaining.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pageSize | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Maximum size of a single page. |
| notificationScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler for sending the notifications ([INotifyCollectionChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Specialized.INotifyCollectionChanged 'System.Collections.Specialized.INotifyCollectionChanged'), [INotifyPropertyChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ComponentModel.INotifyPropertyChanged 'System.ComponentModel.INotifyPropertyChanged')). |
| backgroundScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | Per default this scheduler is used for all background operations (page and count fetches, preloading). In further settings you'll have the option to override this scheduler with another for specific background operations. |

<a name='T-BFF-DataVirtualizingCollection-IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2'></a>
## IAsyncOnlyIndexAccessBehaviorCollectionBuilder\`2 `type`

##### Namespace

BFF.DataVirtualizingCollection

##### Summary

Lets you configure the asynchronous index access options.
Asynchronous meas the if the page still needs to be loaded a placeholder will be provided. As soon as the page is loaded a notification is emitted which states that the entry of the index arrived.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TItem | Item type. |
| TVirtualizationKind | IDataVirtualizingCollection or ISlidingWindow. |

<a name='M-BFF-DataVirtualizingCollection-IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2-AsyncIndexAccess-System-Func{System-Int32,System-Int32,`0}-'></a>
### AsyncIndexAccess(placeholderFactory) `method`

##### Summary

If item of requested index isn't loaded yet the collections will return a placeholder instead and emit a notification as soon as it arrives.
Per default the initially configured background scheduler is taken for page and count fetches.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| placeholderFactory | [System.Func{System.Int32,System.Int32,\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Int32,System.Int32,`0}') | You have to provide a factory lambda function which returns a placeholder.
The first parameter is the page key (index of pages) and the second is the page index (index of items inside the page). |

<a name='M-BFF-DataVirtualizingCollection-IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2-AsyncIndexAccess-System-Func{System-Int32,System-Int32,`0},System-Reactive-Concurrency-IScheduler-'></a>
### AsyncIndexAccess(placeholderFactory,pageBackgroundScheduler) `method`

##### Summary

If item of requested index isn't loaded yet the collections will return a placeholder instead and emit a notification as soon as it arrives.
Per default the initially configured background scheduler is taken for count fetches.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| placeholderFactory | [System.Func{System.Int32,System.Int32,\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Int32,System.Int32,`0}') | You have to provide a factory lambda function which returns a placeholder.
The first parameter is the page key (index of pages) and the second is the page index (index of items inside the page). |
| pageBackgroundScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler exclusively for page fetches. |

<a name='M-BFF-DataVirtualizingCollection-IAsyncOnlyIndexAccessBehaviorCollectionBuilder`2-AsyncIndexAccess-System-Func{System-Int32,System-Int32,`0},System-Reactive-Concurrency-IScheduler,System-Reactive-Concurrency-IScheduler-'></a>
### AsyncIndexAccess(placeholderFactory,pageBackgroundScheduler,countBackgroundScheduler) `method`

##### Summary

If item of requested index isn't loaded yet the collections will return a placeholder instead and emit a notification as soon as it arrives.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| placeholderFactory | [System.Func{System.Int32,System.Int32,\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Int32,System.Int32,`0}') | You have to provide a factory lambda function which returns a placeholder.
The first parameter is the page key (index of pages) and the second is the page index (index of items inside the page). |
| pageBackgroundScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler exclusively for page fetches. |
| countBackgroundScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler exclusively for count fetches. |

<a name='T-BFF-DataVirtualizingCollection-DataVirtualizingCollection-IDataVirtualizingCollection'></a>
## IDataVirtualizingCollection `type`

##### Namespace

BFF.DataVirtualizingCollection.DataVirtualizingCollection

##### Summary

Marks a nongeneric data virtualizing collection.
The data virtualizing collection represents the whole backend as a list. However, the items are not loaded all at once but page by page on demand.

<a name='T-BFF-DataVirtualizingCollection-DataVirtualizingCollection-IDataVirtualizingCollection`1'></a>
## IDataVirtualizingCollection\`1 `type`

##### Namespace

BFF.DataVirtualizingCollection.DataVirtualizingCollection

##### Summary

Marks a generic data virtualizing collection.
The data virtualizing collection represents the whole backend as a list. However, the items are not loaded all at once but page by page on demand.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | Item type. |

<a name='T-BFF-DataVirtualizingCollection-IFetchersKindCollectionBuilder`2'></a>
## IFetchersKindCollectionBuilder\`2 `type`

##### Namespace

BFF.DataVirtualizingCollection

##### Summary

Lets you configure the fetcher (page and count) kind and lets you also provide appropriate fetchers as well.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TItem | Item type. |
| TVirtualizationKind | IDataVirtualizingCollection or ISlidingWindow. |

<a name='M-BFF-DataVirtualizingCollection-IFetchersKindCollectionBuilder`2-NonTaskBasedFetchers-System-Func{System-Int32,System-Int32,`0[]},System-Func{System-Int32}-'></a>
### NonTaskBasedFetchers(pageFetcher,countFetcher) `method`

##### Summary

You have to provide non-task-based (synchronous) fetchers.
The page fetcher has to get a page from the backend based on the provided offset and size. The count fetcher has to get the count of the items in the backend.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pageFetcher | [System.Func{System.Int32,System.Int32,\`0[]}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Int32,System.Int32,`0[]}') | First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page from the backend. |
| countFetcher | [System.Func{System.Int32}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Int32}') | You have to provide a lambda function which gets the count of all elements in the backend. |

<a name='M-BFF-DataVirtualizingCollection-IFetchersKindCollectionBuilder`2-TaskBasedFetchers-System-Func{System-Int32,System-Int32,System-Threading-Tasks-Task{`0[]}},System-Func{System-Threading-Tasks-Task{System-Int32}}-'></a>
### TaskBasedFetchers(pageFetcher,countFetcher) `method`

##### Summary

You have to provide task-based (asynchronous) fetchers.
The page fetcher has to get a page from the backend based on the provided offset and size. The count fetcher has to get the count of the items in the backend.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pageFetcher | [System.Func{System.Int32,System.Int32,System.Threading.Tasks.Task{\`0[]}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Int32,System.Int32,System.Threading.Tasks.Task{`0[]}}') | First parameter is the offset, second parameter is the size. You have to provide a lambda function which given the parameters returns the expected page from the backend. |
| countFetcher | [System.Func{System.Threading.Tasks.Task{System.Int32}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Threading.Tasks.Task{System.Int32}}') | You have to provide a lambda function which gets the count of all elements in the backend. |

<a name='T-BFF-DataVirtualizingCollection-IIndexAccessBehaviorCollectionBuilder`2'></a>
## IIndexAccessBehaviorCollectionBuilder\`2 `type`

##### Namespace

BFF.DataVirtualizingCollection

##### Summary

Lets you configure whether the index access should be synchronous or asynchronous.
Synchronous means that if the index access will wait actively until the entry is provided even if the page still has to be loaded.
Asynchronous meas the if the page still needs to be loaded a placeholder will be provided. As soon as the page is loaded a notification is emitted which states that the entry of the index arrived.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TItem | Item type. |
| TVirtualizationKind | IDataVirtualizingCollection or ISlidingWindow. |

<a name='M-BFF-DataVirtualizingCollection-IIndexAccessBehaviorCollectionBuilder`2-SyncIndexAccess'></a>
### SyncIndexAccess() `method`

##### Summary

If item of requested index isn't loaded yet the collections will wait actively and return as soon as it arrives.
Depending on the latency of the page fetcher it may result in freezes of the application.

##### Parameters

This method has no parameters.

<a name='T-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2'></a>
## IPageHoldingBehaviorCollectionBuilder\`2 `type`

##### Namespace

BFF.DataVirtualizingCollection

##### Summary

Lets you configure the page holding behavior.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TItem | Item type. |
| TVirtualizationKind | IDataVirtualizingCollection or ISlidingWindow. |

<a name='M-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2-CustomPageRemovalStrategy-System-Func{System-IObservable{System-ValueTuple{System-Int32,System-Int32}},System-IObservable{System-Collections-Generic-IReadOnlyList{System-Int32}}}-'></a>
### CustomPageRemovalStrategy() `method`

##### Summary

With this function you can provide a custom page-removal strategy.
You'll get an observable which emits all element requests in form of a key to the page and the element's index inside of the page.
You'll have to return an observable which emits page-removal requests. You can request to remove several pages at once.

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2-Hoarding'></a>
### Hoarding() `method`

##### Summary

In this mode pages are loaded on demand. However, once loaded the pages are hold in memory until the data virtualizing collection is reset or disposed.

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2-LeastRecentlyUsed-System-Int32-'></a>
### LeastRecentlyUsed(pageLimit) `method`

##### Summary

If the page limit is reached then the page which is least recently used will be chosen for removal.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pageLimit | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Has to be greater than zero (with preloading greater than two) in order to maintain at least one page in the page store (when preloading is active, then the neighbors of the most recently requested page are maintained as well). |

<a name='M-BFF-DataVirtualizingCollection-IPageHoldingBehaviorCollectionBuilder`2-LeastRecentlyUsed-System-Int32,System-Int32-'></a>
### LeastRecentlyUsed(pageLimit,removalCount) `method`

##### Summary

If the page limit is reached then the pages (amount: removal buffer plus one) which are least recently used will be chosen for removal.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pageLimit | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Has to be greater than zero (with preloading greater than two) in order to maintain at least one page in the page store (when preloading is active, then the neighbors of the most recently requested page are maintained as well). |
| removalCount | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Has to be in between one and the page limit minus one (so at least one page remains).
With active preloading the removal count cannot be greater than the page limit minus three. |

<a name='T-BFF-DataVirtualizingCollection-IPageLoadingBehaviorCollectionBuilder`2'></a>
## IPageLoadingBehaviorCollectionBuilder\`2 `type`

##### Namespace

BFF.DataVirtualizingCollection

##### Summary

Lets you configure the page loading behavior.
Here you can turn the preloading on or off. Preloading means that neighboring pages from requested pages are loaded as well, assuming that they'll be requested soon.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TItem | Item type. |
| TVirtualizationKind | IDataVirtualizingCollection or ISlidingWindow. |

<a name='M-BFF-DataVirtualizingCollection-IPageLoadingBehaviorCollectionBuilder`2-NonPreloading'></a>
### NonPreloading() `method`

##### Summary

No preloading. Pages are loaded only as soon as an item of the page is requested.

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-IPageLoadingBehaviorCollectionBuilder`2-Preloading-System-Func{System-Int32,System-Int32,`0}-'></a>
### Preloading(preloadingPlaceholderFactory) `method`

##### Summary

Pages are loaded as soon as an item of the page is requested or as soon as a neighboring page is loaded.
Per default the initially configured background scheduler is taken for the preloads.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| preloadingPlaceholderFactory | [System.Func{System.Int32,System.Int32,\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Int32,System.Int32,`0}') | Initially preloaded pages are filled with placeholders. |

<a name='M-BFF-DataVirtualizingCollection-IPageLoadingBehaviorCollectionBuilder`2-Preloading-System-Func{System-Int32,System-Int32,`0},System-Reactive-Concurrency-IScheduler-'></a>
### Preloading(preloadingPlaceholderFactory,preloadingBackgroundScheduler) `method`

##### Summary

Pages are loaded as soon as an item of the page is requested or as soon as a neighboring page is loaded.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| preloadingPlaceholderFactory | [System.Func{System.Int32,System.Int32,\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Int32,System.Int32,`0}') | Initially preloaded pages are filled with placeholders. |
| preloadingBackgroundScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler exclusively for preloading pages. |

<a name='T-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow'></a>
## ISlidingWindow `type`

##### Namespace

BFF.DataVirtualizingCollection.SlidingWindow

##### Summary

Defines a nongeneric window to the backend (accessed by the page- and count-fetchers).
A window is intended to be a much smaller section of the backend. It is specified by an offset and a size.
Outwards it looks like a small list which contains only a few items of the whole backend. However, the sliding functionality
makes it possible to go through the whole backend.

<a name='P-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-MaximumOffset'></a>
### MaximumOffset `property`

##### Summary

Current maximum possible offset. Depends on the count of all backend items and the size of the window.

<a name='P-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-Offset'></a>
### Offset `property`

##### Summary

Current offset of the window inside of the range of the items from the backend. The Offset marks the first item of the backend which is represented in the sliding window.

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-DecreaseWindowSize'></a>
### DecreaseWindowSize() `method`

##### Summary

Decreases windows size by one.

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-DecreaseWindowSizeBy-System-Int32-'></a>
### DecreaseWindowSizeBy() `method`

##### Summary

Decreases windows size by given increment.

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-IncreaseWindowSize'></a>
### IncreaseWindowSize() `method`

##### Summary

Increases windows size by one.

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-IncreaseWindowSizeBy-System-Int32-'></a>
### IncreaseWindowSizeBy() `method`

##### Summary

Increases windows size by given increment.

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-JumpTo-System-Int32-'></a>
### JumpTo() `method`

##### Summary

Sets the first entry of the window ([Offset](#P-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-Offset 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.Offset')) to the given index of the backend.

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-SetWindowSizeTo-System-Int32-'></a>
### SetWindowSizeTo() `method`

##### Summary

Sets windows size to given size.

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-SlideLeft'></a>
### SlideLeft() `method`

##### Summary

Slides the window ([Offset](#P-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-Offset 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.Offset')) to the backend one step to the start (left).

##### Parameters

This method has no parameters.

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-SlideRight'></a>
### SlideRight() `method`

##### Summary

Slides the window ([Offset](#P-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow-Offset 'BFF.DataVirtualizingCollection.SlidingWindow.ISlidingWindow.Offset')) to the backend one step to the end (right).

##### Parameters

This method has no parameters.

<a name='T-BFF-DataVirtualizingCollection-SlidingWindow-ISlidingWindow`1'></a>
## ISlidingWindow\`1 `type`

##### Namespace

BFF.DataVirtualizingCollection.SlidingWindow

##### Summary

Defines a generic window to the backend (accessed by the page- and count-fetchers).
A window is intended to be a much smaller section of the backend. It is specified by an offset and a size.
Outwards it looks like a small list which contains only a few items of the whole backend. However, the sliding functionality
makes it possible to go through the whole backend.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | Item type. |

<a name='T-BFF-DataVirtualizingCollection-IVirtualizationBase'></a>
## IVirtualizationBase `type`

##### Namespace

BFF.DataVirtualizingCollection

##### Summary

Root interface of all virtualizing collection types. Implements all necessary .Net interfaces (`IList`, `INotifyCollectionChanged`, `INotifyPropertyChanged`) in order to be usable by standard UI controls (like `ItemsControl` from WPF).



Additionally, it has virtualization-specific members of its own. See documentation of the members for further details.

##### Remarks

Implements IList in order to mock a standard .Net list. Controls like the `ItemsControl` from WPF in combination with the `VirtualizingStackPanel` use the indexer in order to load items on demand. Also the Count-property is used to arrange the scroll bar accordingly.



The virtualized collections automatically changes its items. For example, if an async index access or preloading is set up, it will be eventually necessary to replace placeholders with the actually loaded items. In order to notify this changes to the UI the `INotifyCollectionChanged` interface is implemented.



Also the properties of the collection itself may change, especially the Count-property. Hence notifications for such changes are send with `INotifyPropertyChanged`.



Finally, each virtualizing collection has to be disposed after use, because it uses Rx subscriptions which have to be cleaned up. Also the currently active pages including their items which implement `IDisposable` are disposed as well.

<a name='P-BFF-DataVirtualizingCollection-IVirtualizationBase-InitializationCompleted'></a>
### InitializationCompleted `property`

##### Summary

Task is successfully completed when initialization is completed.

##### Remarks

Initialization depends on the initial calculation of the Count-property. Because of the asynchronicity of task-based fetchers the Count-property might not be calculated at the end of construction of the virtualized collection.

<a name='P-BFF-DataVirtualizingCollection-IVirtualizationBase-SelectedIndex'></a>
### SelectedIndex `property`

##### Summary

Can be bound to SelectedIndexProperty on Selector controls in order to workaround issue with resets and selected items.

##### Remarks

In WPF the Selector control will search for the previously selected item after each reset by iterating over all items until found. This behavior is the opposite of virtualization. Hence, the virtualizing collection would set this property to -1 (deselection) and notify the change before notifying any reset.

<a name='M-BFF-DataVirtualizingCollection-IVirtualizationBase-Reset'></a>
### Reset() `method`

##### Summary

Disposes of all current pages and notifies that possibly everything changed.



The Reset-function should be called any time when something in the virtualized backend has changed.

##### Parameters

This method has no parameters.

##### Remarks

Consequently, the Count-property is recalculated. The UI will request all currently rendered items anew, so this items get reloaded.

<a name='T-BFF-DataVirtualizingCollection-IVirtualizationBase`1'></a>
## IVirtualizationBase\`1 `type`

##### Namespace

BFF.DataVirtualizingCollection

##### Summary

The generic version of [IVirtualizationBase](#T-BFF-DataVirtualizingCollection-IVirtualizationBase 'BFF.DataVirtualizingCollection.IVirtualizationBase'). Analogously, it implements [IList\`1](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IList`1 'System.Collections.Generic.IList`1') and [IReadOnlyList\`1](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IReadOnlyList`1 'System.Collections.Generic.IReadOnlyList`1').

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | Item type. |

<a name='P-BFF-DataVirtualizingCollection-IVirtualizationBase`1-Count'></a>
### Count `property`

##### Summary

The Count-property is newed here in order to resolve ambiguities cause by implementing [IList](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.IList 'System.Collections.IList'), [IList\`1](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IList`1 'System.Collections.Generic.IList`1') and [IReadOnlyList\`1](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IReadOnlyList`1 'System.Collections.Generic.IReadOnlyList`1') at the same time.

<a name='P-BFF-DataVirtualizingCollection-IVirtualizationBase`1-Item-System-Int32-'></a>
### Item `property`

##### Summary

The indexer is newed here in order to resolve ambiguities cause by implementing [IList](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.IList 'System.Collections.IList'), [IList\`1](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IList`1 'System.Collections.Generic.IList`1') and [IReadOnlyList\`1](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IReadOnlyList`1 'System.Collections.Generic.IReadOnlyList`1') at the same time.

<a name='T-BFF-DataVirtualizingCollection-PageRemoval-PageReplacementStrategyException'></a>
## PageReplacementStrategyException `type`

##### Namespace

BFF.DataVirtualizingCollection.PageRemoval

##### Summary

Thrown whenever an exception occurs during the process of page replacement.

<a name='T-BFF-DataVirtualizingCollection-SlidingWindow-SlidingWindowBuilder'></a>
## SlidingWindowBuilder `type`

##### Namespace

BFF.DataVirtualizingCollection.SlidingWindow

##### Summary

Initial entry point for creating a sliding window.

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-SlidingWindowBuilder-Build``1-System-Int32,System-Int32,System-Reactive-Concurrency-IScheduler-'></a>
### Build\`\`1(windowSize,initialOffset,notificationScheduler) `method`

##### Summary

Use to configure general virtualization and sliding-window-specific settings.
Further settings are applied via method chaining.
Page size is set to the default value 100.
The background scheduler is per default the [TaskPoolScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.TaskPoolScheduler 'System.Reactive.Concurrency.TaskPoolScheduler').

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| windowSize | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Initial count of items that the window should contain. |
| initialOffset | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Initial starting item within the backend. |
| notificationScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler for sending the notifications ([INotifyCollectionChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Specialized.INotifyCollectionChanged 'System.Collections.Specialized.INotifyCollectionChanged'), [INotifyPropertyChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ComponentModel.INotifyPropertyChanged 'System.ComponentModel.INotifyPropertyChanged')). |

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-SlidingWindowBuilder-Build``1-System-Int32,System-Int32,System-Int32,System-Reactive-Concurrency-IScheduler-'></a>
### Build\`\`1(windowSize,initialOffset,pageSize,notificationScheduler) `method`

##### Summary

Use to configure general virtualization and sliding-window-specific settings.
Further settings are applied via method chaining.
The background scheduler is per default the [TaskPoolScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.TaskPoolScheduler 'System.Reactive.Concurrency.TaskPoolScheduler').

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| windowSize | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Initial count of items that the window should contain. |
| initialOffset | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Initial starting item within the backend. |
| pageSize | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Maximum size of a single page. |
| notificationScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler for sending the notifications ([INotifyCollectionChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Specialized.INotifyCollectionChanged 'System.Collections.Specialized.INotifyCollectionChanged'), [INotifyPropertyChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ComponentModel.INotifyPropertyChanged 'System.ComponentModel.INotifyPropertyChanged')). |

<a name='M-BFF-DataVirtualizingCollection-SlidingWindow-SlidingWindowBuilder-Build``1-System-Int32,System-Int32,System-Int32,System-Reactive-Concurrency-IScheduler,System-Reactive-Concurrency-IScheduler-'></a>
### Build\`\`1(windowSize,initialOffset,pageSize,notificationScheduler,backgroundScheduler) `method`

##### Summary

Use to configure general virtualization and sliding-window-specific settings.
Further settings are applied via method chaining.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| windowSize | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Initial count of items that the window should contain. |
| initialOffset | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Initial starting item within the backend. |
| pageSize | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Maximum size of a single page. |
| notificationScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | A scheduler for sending the notifications ([INotifyCollectionChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Specialized.INotifyCollectionChanged 'System.Collections.Specialized.INotifyCollectionChanged'), [INotifyPropertyChanged](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ComponentModel.INotifyPropertyChanged 'System.ComponentModel.INotifyPropertyChanged')). |
| backgroundScheduler | [System.Reactive.Concurrency.IScheduler](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reactive.Concurrency.IScheduler 'System.Reactive.Concurrency.IScheduler') | Per default this scheduler is used for all background operations (page and count fetches, preloading). In further settings you'll have the option to override this scheduler with another for specific background operations. |
