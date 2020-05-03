using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Adapters;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Interfaces;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Utility;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Functions;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Options;
using BFF.DataVirtualizingCollection.SlidingWindow;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels
{
    public interface IDataVirtualizingCollectionViewModelBase
    {
        string Name { get; }
        IGeneralOptionsViewModel GeneralOptionsViewModel { get; }
        ISpecificOptionsViewModel SpecificOptionsViewModel { get; }
        IPageLoadingBehaviorViewModel PageLoadingBehaviorViewModel { get; }
        IPageRemovalBehaviorViewModel PageRemovalBehaviorViewModel { get; }
        IFetcherKindViewModel FetcherKindViewModel { get; }
        IIndexAccessBehaviorViewModel IndexAccessBehaviorViewModel { get; }
        ICommand CreateNew { get; }
        IGeneralFunctionsViewModel GeneralFunctionsViewModel { get; }
        ISpecificFunctionsViewModel SpecificFunctionsViewModel { get; }
        BackendAccessKind BackendAccessKind { get; }
        IVirtualizationBase? Items { get; }
    }

    public interface IDataVirtualizingCollectionViewModelBase<T> : IDataVirtualizingCollectionViewModelBase
    {
    }

    public abstract class DataVirtualizingCollectionViewModelBaseBase<TViewModel, TVirtualizationKind> : 
        ObservableObject, IDisposable, IDataVirtualizingCollectionViewModelBase<TViewModel>
        where TVirtualizationKind : class, IVirtualizationBase<TViewModel>
    {
        private readonly IBackendAccessAdapter<TViewModel> _backendAccessAdapter;
        private readonly IGetSchedulers _getSchedulers;
        private IVirtualizationBase? _items;
        private readonly SerialDisposable _serialItems;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        protected DataVirtualizingCollectionViewModelBaseBase(
            // parameters
            IBackendAccessAdapter<TViewModel> backendAccessAdapter,
            
            // dependencies
            IGeneralOptionsViewModel generalOptionsViewModel,
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel,
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel,
            IFetcherKindViewModelInternal fetcherKindViewModel,
            IGeneralFunctionsViewModel generalFunctionsViewModel,
            IGetSchedulers getSchedulers)
        {
            _backendAccessAdapter = backendAccessAdapter;
            _getSchedulers = getSchedulers;
            GeneralOptionsViewModel = generalOptionsViewModel;
            PageLoadingBehaviorViewModel = pageLoadingBehaviorViewModel;
            PageRemovalBehaviorViewModel = pageRemovalBehaviorViewModel;
            FetcherKindViewModel = fetcherKindViewModel;
            GeneralFunctionsViewModel = generalFunctionsViewModel;
            IndexAccessBehaviorViewModel = fetcherKindViewModel.IndexAccessBehaviorViewModel;
            
            _serialItems = new SerialDisposable();
            _compositeDisposable.Add(_serialItems);

            var createNew = new RxRelayCommand(SetItems);
            CreateNew = createNew;
            _compositeDisposable.Add(createNew);
        }

        public string Name => _backendAccessAdapter.Name;
        public BackendAccessKind BackendAccessKind => _backendAccessAdapter.BackendAccessKind;

        public IVirtualizationBase? Items
        {
            get
            {
                if (_items is null) SetItems();
                return _items;
            }
            private set
            {
                if (_items == value) return;
                _items = value;
                _serialItems.Disposable = _items;
                OnPropertyChanged();
            }
        }

        public IGeneralOptionsViewModel GeneralOptionsViewModel { get; }
        public abstract ISpecificOptionsViewModel SpecificOptionsViewModel { get; }
        public IPageLoadingBehaviorViewModel PageLoadingBehaviorViewModel { get; }
        public IPageRemovalBehaviorViewModel PageRemovalBehaviorViewModel { get; }
        public IFetcherKindViewModel FetcherKindViewModel { get; }
        public IIndexAccessBehaviorViewModel IndexAccessBehaviorViewModel { get; }
        public ICommand CreateNew { get; }
        public IGeneralFunctionsViewModel GeneralFunctionsViewModel { get; }
        public abstract ISpecificFunctionsViewModel SpecificFunctionsViewModel { get; }

        protected abstract IPageLoadingBehaviorCollectionBuilder<TViewModel, TVirtualizationKind>
            CreateInitialBuilder(int pageSize, IScheduler notificationScheduler, IScheduler backgroundScheduler);

        private void SetItems()
        {
            var builder = CreateInitialBuilder(
                GeneralOptionsViewModel.PageSize,
                _getSchedulers.NotificationScheduler,
                _getSchedulers.BackgroundScheduler);
            var afterPageLoadingDecision = 
                PageLoadingBehaviorViewModel.Configure(builder, _backendAccessAdapter.BackendAccess);
            var afterPageRemovalDecision = 
                PageRemovalBehaviorViewModel.Configure(afterPageLoadingDecision);
            var afterFetcherKindDecision = 
                FetcherKindViewModel.Configure(afterPageRemovalDecision, _backendAccessAdapter.BackendAccess);
            var collection = IndexAccessBehaviorViewModel.Configure(
                afterFetcherKindDecision, 
                _backendAccessAdapter.BackendAccess,
                FetcherKindViewModel.FetcherKind);
            Items = collection;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }

    public class DataVirtualizingCollectionViewModel<TViewModel> : 
        DataVirtualizingCollectionViewModelBaseBase<TViewModel, IDataVirtualizingCollection<TViewModel>>
    {
        public DataVirtualizingCollectionViewModel(
            // parameters
            IBackendAccessAdapter<TViewModel> backendAccessAdapter,
            
            // dependencies
            IGeneralOptionsViewModel generalOptionsViewModel,
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel,
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel,
            IFetcherKindViewModelInternal fetcherKindViewModel,
            IGeneralFunctionsViewModel generalFunctionsViewModel,
            IGetSchedulers getSchedulers)
            : base(
                backendAccessAdapter,
                generalOptionsViewModel,
                pageLoadingBehaviorViewModel,
                pageRemovalBehaviorViewModel,
                fetcherKindViewModel,
                generalFunctionsViewModel,
                getSchedulers)
        {
        }

        public override ISpecificOptionsViewModel SpecificOptionsViewModel => ViewModels.Options.SpecificOptionsViewModel.Empty;

        public override ISpecificFunctionsViewModel SpecificFunctionsViewModel => ViewModels.Functions.SpecificFunctionsViewModel.Empty;

        protected override IPageLoadingBehaviorCollectionBuilder<TViewModel, IDataVirtualizingCollection<TViewModel>> CreateInitialBuilder(
            int pageSize,
            IScheduler notificationScheduler,
            IScheduler backgroundScheduler)
        {
            return DataVirtualizingCollectionBuilder<TViewModel>.Build(
                pageSize, 
                notificationScheduler,
                backgroundScheduler);
        }
    }

    public class SlidingWindowViewModel<TViewModel> : 
        DataVirtualizingCollectionViewModelBaseBase<TViewModel, ISlidingWindow<TViewModel>>
    {
        private readonly ISlidingWindowOptionsViewModel _slidingWindowOptionsViewModel;

        public SlidingWindowViewModel(
            // parameters
            IBackendAccessAdapter<TViewModel> backendAccessAdapter,
            
            // dependencies
            IGeneralOptionsViewModel generalOptionsViewModel,
            ISlidingWindowOptionsViewModel slidingWindowOptionsViewModel,
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel,
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel,
            IFetcherKindViewModelInternal fetcherKindViewModel,
            IGeneralFunctionsViewModel generalFunctionsViewModel,
            ISlidingWindowFunctionsViewModel slidingWindowFunctionsViewModel,
            IGetSchedulers getSchedulers)
            : base(
                backendAccessAdapter,
                generalOptionsViewModel,
                pageLoadingBehaviorViewModel,
                pageRemovalBehaviorViewModel,
                fetcherKindViewModel,
                generalFunctionsViewModel,
                getSchedulers)
        {
            _slidingWindowOptionsViewModel = slidingWindowOptionsViewModel;
            SpecificFunctionsViewModel = slidingWindowFunctionsViewModel;
        }

        public override ISpecificOptionsViewModel SpecificOptionsViewModel => _slidingWindowOptionsViewModel;

        public override ISpecificFunctionsViewModel SpecificFunctionsViewModel { get; }

        protected override IPageLoadingBehaviorCollectionBuilder<TViewModel, ISlidingWindow<TViewModel>> CreateInitialBuilder(
            int pageSize,
            IScheduler notificationScheduler,
            IScheduler backgroundScheduler)
        {
            return SlidingWindowBuilder<TViewModel>.Build(
                _slidingWindowOptionsViewModel.WindowSize,
                _slidingWindowOptionsViewModel.WindowOffset,
                pageSize, 
                notificationScheduler,
                backgroundScheduler);
        }
    }

    internal interface IDataVirtualizingCollectionViewModelFactory
    {
        IDataVirtualizingCollectionViewModelBase<T> CreateDataVirtualizingCollection<T>(
            IBackendAccessAdapter<T> backendAccessAdapter);
        
        IDataVirtualizingCollectionViewModelBase<T> CreateSlidingWindow<T>(
            IBackendAccessAdapter<T> backendAccessAdapter);
    }

    internal class DataVirtualizingCollectionViewModelFactory : IDataVirtualizingCollectionViewModelFactory
    {
        private readonly IGeneralOptionsViewModel _generalOptionsViewModel;
        private readonly ISlidingWindowOptionsViewModel _slidingWindowOptionsViewModel;
        private readonly IPageLoadingBehaviorViewModel _pageLoadingBehaviorViewModel;
        private readonly IPageRemovalBehaviorViewModel _pageRemovalBehaviorViewModel;
        private readonly IFetcherKindViewModelInternal _fetcherKindViewModel;
        private readonly IGeneralFunctionsViewModel _generalFunctionsViewModel;
        private readonly ISlidingWindowFunctionsViewModel _slidingWindowFunctionsViewModel;
        private readonly IGetSchedulers _getSchedulers;
        private readonly CompositeDisposable _compositeDisposableOfLifetimeScope;

        public DataVirtualizingCollectionViewModelFactory(
            IGeneralOptionsViewModel generalOptionsViewModel,
            ISlidingWindowOptionsViewModel slidingWindowOptionsViewModel,
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel,
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel,
            IFetcherKindViewModelInternal fetcherKindViewModel,
            IGeneralFunctionsViewModel generalFunctionsViewModel,
            ISlidingWindowFunctionsViewModel slidingWindowFunctionsViewModel,
            IGetSchedulers getSchedulers,
            CompositeDisposable compositeDisposableOfLifetimeScope)
        {
            _generalOptionsViewModel = generalOptionsViewModel;
            _slidingWindowOptionsViewModel = slidingWindowOptionsViewModel;
            _pageLoadingBehaviorViewModel = pageLoadingBehaviorViewModel;
            _pageRemovalBehaviorViewModel = pageRemovalBehaviorViewModel;
            _fetcherKindViewModel = fetcherKindViewModel;
            _generalFunctionsViewModel = generalFunctionsViewModel;
            _slidingWindowFunctionsViewModel = slidingWindowFunctionsViewModel;
            _getSchedulers = getSchedulers;
            _compositeDisposableOfLifetimeScope = compositeDisposableOfLifetimeScope;
        }

        public IDataVirtualizingCollectionViewModelBase<T> CreateDataVirtualizingCollection<T>(IBackendAccessAdapter<T> backendAccessAdapter)
        {
            var ret = new DataVirtualizingCollectionViewModel<T>(
                backendAccessAdapter,
                _generalOptionsViewModel,
                _pageLoadingBehaviorViewModel,
                _pageRemovalBehaviorViewModel,
                _fetcherKindViewModel,
                _generalFunctionsViewModel,
                _getSchedulers);
            _compositeDisposableOfLifetimeScope.Add(ret);
            return ret;
        }

        public IDataVirtualizingCollectionViewModelBase<T> CreateSlidingWindow<T>(IBackendAccessAdapter<T> backendAccessAdapter)
        {
            var ret = new SlidingWindowViewModel<T>(
                backendAccessAdapter,
                _generalOptionsViewModel,
                _slidingWindowOptionsViewModel,
                _pageLoadingBehaviorViewModel,
                _pageRemovalBehaviorViewModel,
                _fetcherKindViewModel,
                _generalFunctionsViewModel,
                _slidingWindowFunctionsViewModel,
                _getSchedulers);
            _compositeDisposableOfLifetimeScope.Add(ret);
            return ret;
        }
    }
}