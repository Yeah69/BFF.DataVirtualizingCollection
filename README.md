
# BFF.DataVirtualizingCollection
This is an approach to data-virtualizing collections intended to be used in WPF projects.

Install it from nuget (see [nuget page](https://www.nuget.org/packages/BFF.DataVirtualizingCollection/)):

```
> Install-Package BFF.DataVirtualizingCollection
```

## Why data virtualization?

Say, you want to display a table from a database which contains thousands of rows in your WPF-application. If an ordinary list is used in combination of a `DataGrid`, then all the rows have to be loaded into memory. Even though, the amount of rows which can be displayed at once usually doesn't surpass 50 rows. Fortunately, `DataGrid`s have UI-virtualization integrated. Which means that only the visible rows get representing UI-elements. However, still the whole data has to be loaded into memory. In order to virtualize the data in analogous manner to the UI-elements data virtualization has to be used.

## How?

Data virtualizing collections act as if they have all the data. However, they only fetch the data wich is currently requested on demand. Usually the data is fetched in chunks called "pages" of fixed size. For example, if the page size is set 100 and the `DataGrid` is initially pointing to the top most row with index zero, then initially only the first 100 elements (i.e. the first page) is fetched. As soon as the user scrolls further down and the row with index 100 is displayed, another page is loaded.
The advantage is generally a better performance and less UI freeze, because lesser data is loaded at once and only on demand. Furthermore, the memory consumption improves as well.

## Use case of this project

Up until now this data-virtualizing collection has been used exclusively for WPF-`DataGrid`s. Hence, it wasn't tested for other `ItemsControl`s. However, it is assumed that they work similarly to the `DataGrid`s.
The `ItemsSource` of `DataGrid`s accepts arbitrary instances of `IList`. The `DataGrid` uses `IList`'s indexer to access the items. This project uses this insight to its advantage in order to track which indexes are requested. Therefore, the pages can be loaded on demand rather than all at once.

## Break in Liskov's Substitution Principle

The collections from this project do implement the `IList` interface. However, they don't support most of the interfaces functions besides the indexer and the `Count` property. That is a heavy break in Liskov's Substitution Principle. Hence, these collections cannot be used like full-fledged `IList` implementation. But this principle break is on purpose. Let's take the function `Contains`, for example. How should be checked if the data represented by this collection contains the searched element? Sure, you could check the already loaded pages. But what if you are not lucky? What if the collection doesn't even contain the element. The consequence would be to load all remaining elements in order to check for the searched element. This would contradict the initial purpose of the data virtualization and eliminate its advantages. And besides that, usually a data virtualizing solution is used in combination with databases, hence such a "Contains"-request is better of as a query applied directly to the database.

## Example

```csharp
public IList<int> AllPositiveIntNumbers { get; } = 
    DataVirtualizingCollectionBuilder<int>
        .Build(pageSize: 100)
        .Hoarding()
        .NonPreloading()
        .NonTaskBasedFetchers(
            pageFetcher: (offset, pageSize) => Enumerable.Range(offset, pageSize).ToArray(),
            countFetcher: () => int.MaxValue)
        .SyncIndexAccess();
```

This data virtualizing collection virtualizes an `IList<int>` with a number sequence of 0 to `int.Max` - 1. Hence, it emulates a list of maximum count, because the `Count` property of `IList<T>` is of type `int`. Data virtualized collections can only be build with the `DataVirtualizingCollectionBuilder<T>`. In fact, it is the only externally public entry point of the whole library. The collections have to be build by calling the `Build()`-function and making four decision. The deciding is done by choosing the appropriate function during the method chaining. The example above is the most simple in regard of the chosen options, which makes it the "Hello, world!" of this data virtualizing library. Here is a short introduction to the options:

### Build

The `Build()`-function doesn't count as a decision. It is a necessity in order to create a new builder instance where onto the decisions are made. The `pageSize`-parameter is optional and its default is 100. This parameter determines the usual requested page size (the last page may have a different size).

### The Page-Loading Decision

This decides the loading process of the pages. The option here are `NonPreloading()` and `Preloading()`. If `NonPreloading()` is active a page is loaded as soon as the first item of this page is requested but not sooner. If `Preloading()` is active, then the neighboring pages (next and previous; if not done already) are loaded as soon as an item of the current page is requested. The assumption here is that given a certain starting point, the user is likely to scroll in any direction. Hence, the probability is high that one of the neighboring pages is requested soon. The preloading happens in background, so requests of the current page are not blocked.

### The Page-Removal Decision

The second decision sets the page-removal strategy. At the moment of writing there are three option: `Hoarding()`, `LeastRecentlyUsed(…)`, and `CustomPageRemovalStrategy(…)`.

- __Hoarding__ The pages are never removed until the data virtualizing collection is disposed of.
- __LeastRecentlyUsed__ As soon as the page limit (which you have to set) is reached the least recently used page or pages (you can set how many pages are removed at once) are removed.
- __CustomPageRemovalStrategy__ You can define you own page removal strategy. In order to accomplish that you'll get an observable which streams the page key (index of all pages) and page index (index of element inside the page) of the indexer calls of the data virtualizing collection. The _LeastRecentlyUsed_-strategy was implemented the same way, I would recommend to look into its sources first if you would like to implement your own strategy.

### The Definition of the Fetchers

Next, the fetcher have to be defined. In order to work properly the data virtualizing collection needs two kinds of fetcher. The page fetcher is given the integer parameters `offset` and `pageSize` (doesn't have to be the same value as provided to `Build()`) and expects an array of elements with amount of `pageSize` starting from the `offset`. What the page fetcher has to do in order to accomplish this result depends on the data source which is virtualized. In the example above the numbers are procedurally generated. If the data source is a database, then the page fetcher could delegate the request to ORM or a query of some kind.
The count fetcher tells the amount of items of the data source to the data virtualizing collection. Again, the actual implementation will depend on the kind of data source. 

It can be decided whether the provided fetcher are synchronous or task-based asynchronous functions.

### Decision about the Index Access

The last decision determines whether the item access is synchronous (`Sync()`) or asynchronous (`Async([…])`). The synchronous variant would block the thread and wait if an item from a not-yet-loaded page is requested. In same situation the asynchronous variant would return a placeholder (the user has to provide a placeholder-function) first instead of blocking the current thread and emit a notification event as soon as the item arrives. In case the page is already loaded the right item is returned directly and no notification is emitted. That way or the other the asynchronous variant does not block.

This last call returns the build data virtualized collection.

## Are there altenative solutions?

This project was created as a replacement for the data-virtualizing solution "AlphaChiTech.Virtualization" in a yet-secret project called BFF. However, unlike its intelectual predecessor this project's collections do implement the `IDisposable` interface. On disposal all stored disposable elements are disposed of. Furthermore, the async variants of the collections do operate in asynchronous fashion "under the hood". This means that all it is sufficient to provide synchronous implementation of the data access interface. The asynchronous variants call the provided data acces from the background. Hence, the the current thread (assumeably the UI thread) is not blocked.
Besides the advantages, this project is still young and may not have yet all features which the "AlphaChiTech.Virtualization" solution provides. Hence, it is recommended to have a look in there, if this project doesn't meet your personal requirements.
