using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;

namespace BFF.DataVirtualizingCollection.Sample.GettingStarted;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Welcome to the "Getting Started" sample
        
        // It is intentionally held as minimalistic as possible. It should be a quick starting point.
        // In this sample we want to virtualize the numbers from 0 to 2,147,483,646 (int.MaxValue - 1)
        
        // At minimum three settings are required from the user (you) of the data vitualizing collection
        // The notification scheduler, the page fetcher and the count fetcher
        
        // For WPF applications the notification scheduler should be based on the current applications dispatcher
        // That'll lead to the notifications (INotifyPropertyChanged & INotifyCollectionChanged) to being marshalled to the UI thread
        var notificationScheduler =
            new SynchronizationContextScheduler(
                new DispatcherSynchronizationContext(
                    Application.Current.Dispatcher));
        
        // Because we would like to virtualize the numbers from 0 to 2,147,483,646 (int.MaxValue - 1),
        // … the page fetcher will generate an array of consecutive numbers starting with the offset and of same count as the requested page size and
        Func<int, int, CancellationToken, int[]> pageFetcher = (offset, pageSize, _) => 
            Enumerable.Range(offset, pageSize).ToArray();
        // … the count just needs to return int.MaxValue
        Func<CancellationToken, int> countFetcher = _ => 
            int.MaxValue;
        
        // Given those pre-requirements, we'll create a data virtualizing collections with the most basic options (like hoarding)
        var dataVirtualizingCollection = DataVirtualizingCollectionBuilder
            .Build<int>(notificationScheduler)
            .NonPreloading() // neighboring pages are not preloaded
            .Hoarding() // once loaded pages are not removed for the lifetime of the data virtualizing collection
            .NonTaskBasedFetchers(pageFetcher, countFetcher) // synchronous fetchers
            .SyncIndexAccess(); // synchronous indexer (of IReadOnlyList<T>) access (i.e. UI will block for not yet loaded pages)
        
        // And assign it to the ItemsSource dependency property of a ListBox
        // (in an MVVM application you actually would bind it to the property)
        List.ItemsSource = dataVirtualizingCollection;
        
        // That's it. We've virtualized some numbers (up until the billions, that is :D)
        
        // Please see this as a starting point. Depending of what you would like to do with the data virtualizing collection you can go different ways from here on
        
        // For example, you could replace virtualizing consecutive number with an other source, for example a database.
        
        // For beginners it might be of interest to dive deeper into this minimalistic example. You could put a breakpoint to the page and count fetchers code in order to explore when and how often they get called
        
        // You could also explore options deviating from these basic options (like preloading or least recently used page removal).
        // In that case, I'll also recommend to have a look into the UI of the sample project "Sample.View". It is like a playground for exploring the different options
        
        // This project has a very different kind of virtualization called the "sliding window".
        // It is meant for the special use case where a range of element of rather fixed size slides through the virtualized data.
        // If that matches your use case, then the sliding window might be right for you.
    }
}