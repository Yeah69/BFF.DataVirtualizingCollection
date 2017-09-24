# BFF.DataVirtualizingCollection
This is an approach to data-virtualizing collections intended to be used in WPF projects.

Install it from nuget (see [nuget page](https://www.nuget.org/packages/BFF.DataVirtualizingCollection/)):

```
> Install-Package BFF.DataVirtualizingCollection
```

## Why data virtualization?

Say, you want to display a table from a database which contains thousands of rows in your WPF-application. If an ordinary list is used in combination of a DataGrid, then all the rows have to be loaded into memory. Even though, the amount of rows which can be displayed at once usually doesn't surpass 50 rows. Fortunately, DataGrids have UI-virtualization integrated. Which means that only the visible rows get representing UI-elements. However, still the whole data has to be loaded into memory. In order to virtualize the data in analogous manner to the UI-elements data virtualization has to be used.

## How?

Data virtualizing collections act as if they have all the data. However, they only fetch the data wich is currently requested on demand. Usually the data is fetched in chunks called "pages" of fixed size. For example, if the page size is set 100 and the DataGrid is initially pointing to the top most row with index zero, then initially only the first 100 elements (i.e. the first page) is fetched. As soon as the user scrolls further down and the row with index 100 is displayed, another page is loaded.
The advantage is generally a better performance and less UI freeze, because lesser data is loaded at once and only on demand. Furthermore, the memory consumption improves as well.

## Use case of this project

Up until now this data-virtualizing collection has been used exclusively for WPF-DataGrids. Hence, it wasn't tested for other ItemsControls. However, it is assumed that they work similarly to the DataGrids.
The ItemsSource of DataGrids accepts arbitrary instances of IEnumerable. Which would be problematic from data-virtualizing perspective. Fortunately, the DataGrid are optimized to IList implementations and use theis indexers if the ItemsSource is castable to IList. This project uses it to its advantage in order to track which indexes were requested in recent past. Therefore, only page can be loaded only on demand.

## Break in Liskov's Substitution Principle

The collections from this project do implement the IList interface. However, they don't support most of the interfaces functions besides the indexer and the Count property. That is a heavy break in Liskov's Substitution Principle. Hence, these collections cannot be used like full-fledged IList implementation. But this principle break is on purpose. Let's take the function "Contains" for example. How should be checked if the data represented by this collection contains the searched element? Sure, you could check the already loaded pages. But what if you are not lucky? What if the collection doesn't even contain the element. The consequence would be to load all remaining elements in order to check for the searched element. This would contradict the initial purpose of the data virtualization and eliminate its advantages. And besides that, usually a data virtualizing solution is used in combination with databases, hence such a "Contains"-request is better of as a query applied directly to the database.

## Requirements to use BFF.DataVirtualizingCollection

An implementation of either the interface IBasicSyncDataAccess or the interface IBasicAsyncDataAccess. The "Sync" variant requires a function to fetch a page of data of certain size and starting from a certain offset. Furthermore, a function is demanded, which fetches the total count of the element contained in the accessed data. The "Async" (which doesn't mean that the functions have to be implemented in async/await way) interface has only one further function, which is a factory method for placeholders. The reason for it is that the "Async"-way would return a placeholder instantly, if the page is not yet loaded, and then replace the placeholder with the right data as soon as it arrives. The "Sync" way, however, would block if an element is requested from a page which is yet missing. Hence, "Sync" doesn't need placeholders. This project provide the classes RelayBasicSyncDataAccess and RelayBasicAsyncDataAccess, which are constructed with appropriate lambdas and implement the described interfaces.

Collections can than be build with the help of the CollectionBuilder class. Everyone is encouraged to use this project and raise an issue if something is unclear and further explaination is necessary.


## Are there altenative solutions?

This project was created as a replacement for the data-virtualizing solution "AlphaChiTech.Virtualization" (see [their project site](https://github.com/anagram4wander/VirtualizingObservableCollection/tree/master/AlphaChiTech.Virtualization "AlphaChiTech.Virtualization")) in a yet-secret project called BFF. However, unlike its intelectual predecessor this project's collections do implement the IDisposable interface. On disposal all stored disposable elements are disposed of. Furthermore, the async variants of the collections do operate in asynchronous fashion "under the hood". This means that all it is sufficient to provide synchronous implementation of the data access interface. The asynchronous variants call the provided data acces from the background. Hence, the the current thread (assumeably the UI thread) is not blocked.
Besides the advantages, this project is still young and may not have yet all features which the "AlphaChiTech.Virtualization" solution provides. Hence, it is recommended to have a look in there, if this project doesn't meet your personal requirements.
