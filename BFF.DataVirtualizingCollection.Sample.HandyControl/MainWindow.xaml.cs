using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BFF.DataVirtualizingCollection.Sample.HandyControl.Properties;
using BFF.DataVirtualizingCollection.SlidingWindow;

namespace BFF.DataVirtualizingCollection.Sample.HandyControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private static readonly IScheduler DispatcherScheduler = new DispatcherScheduler(Application.Current.Dispatcher);
        private readonly ISlidingWindow<int> _numberWindow;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            
            _numberWindow  = SlidingWindowBuilder
                .Build<int>(10, 0, 100, DispatcherScheduler, TaskPoolScheduler.Default)
                .NonPreloading()
                .Hoarding()
                /*.NonTaskBasedFetchers(
                    (offset, pageSize) =>
                    {
                        Thread.Sleep(500);
                        Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading page with offset {offset}");
                        return Enumerable.Range(offset, pageSize).ToArray();
                    },
                    () =>
                    {
                        Thread.Sleep(500);
                        Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading count");
                        return int.MaxValue;
                    })*/
                .TaskBasedFetchers(
                    async (offset, pageSize) =>
                    {
                        await Task.Delay(500);
                        Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading page with offset {offset}");
                        return Enumerable.Range(offset, pageSize).ToArray();
                    },
                    async () =>
                    {
                        await Task.Delay(500);
                        Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading count");
                        return int.MaxValue;
                    })
                //.SyncIndexAccess();
                .AsyncIndexAccess((_, __) => -1);
            
            SlideLeft = new RxRelayCommand(() => _numberWindow.SlideLeft());
            SlideRight = new RxRelayCommand(() => _numberWindow.SlideRight());
            IncreaseWindowSize = new RxRelayCommand(() => _numberWindow.IncreaseWindowSize());
            DecreaseWindowSize = new RxRelayCommand(() => _numberWindow.DecreaseWindowSize());
            Reset = new RxRelayCommand(() => _numberWindow.Reset());

            var disposable = _numberWindow
                .ObservePropertyChanges(nameof(ISlidingWindow<int>.Offset))
                .ObserveOn(DispatcherScheduler)
                .Subscribe(_ => OnPropertyChanged(nameof(ScrollPosition)));
            
            Unloaded += OnUnloaded;
            
            void OnUnloaded(object sender, RoutedEventArgs e)
            {
                disposable.Dispose();
                Unloaded -= OnUnloaded;
            }
        }

        public ICommand SlideLeft { get; }
        
        public ICommand SlideRight { get; }

        public ICommand IncreaseWindowSize { get; }
        
        public ICommand DecreaseWindowSize { get; }
        
        public ICommand Reset { get; }

        public ISlidingWindow<int> AllPositiveIntNumbers => _numberWindow;

        public int ScrollPosition
        {
            get => AllPositiveIntNumbers.Offset;
            set => AllPositiveIntNumbers.JumpTo(value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}