using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Input;
using BFF.DataVirtualizingCollection.SlidingWindow;

namespace BFF.DataVirtualizingCollection.Sample.HandyControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly IScheduler DispatcherScheduler = new DispatcherScheduler(Application.Current.Dispatcher);
        private readonly ISlidingWindow<int> _numberWindow;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            
            _numberWindow  = SlidingWindowBuilder<int>
                .Build(10, 0, DispatcherScheduler, 100)
                .NonPreloading()
                .Hoarding()
                .NonTaskBasedFetchers(
                    (offset, pageSize) =>
                    {
                        Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading page with offset {offset}");
                        return Enumerable.Range(offset, pageSize).ToArray();
                    },
                    () =>
                    {
                        Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading count");
                        return int.MaxValue;
                    })
                .SyncIndexAccess();
            
            SlideLeft = new RxRelayCommand(() => _numberWindow.SlideLeft());
            SlideRight = new RxRelayCommand(() => _numberWindow.SlideRight());
        }
        
        public ICommand SlideLeft { get; }
        
        public ICommand SlideRight { get; }

        public IList<int> AllPositiveIntNumbers => _numberWindow;

    }
}