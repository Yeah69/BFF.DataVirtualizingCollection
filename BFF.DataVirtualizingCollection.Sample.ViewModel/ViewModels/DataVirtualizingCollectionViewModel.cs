using System;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Interfaces;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Utility;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels
{
    public interface IDataVirtualizingCollectionViewModel
    {
        string Name { get; }
        IPageLoadingBehaviorViewModel PageLoadingBehaviorViewModel { get; }
        IPageRemovalBehaviorViewModel PageRemovalBehaviorViewModel { get; }
        IFetcherKindViewModel FetcherKindViewModel { get; }
        IIndexAccessBehaviorViewModel IndexAccessBehaviorViewModel { get; }
        int PageSize { get; set; }
        
        ICommand CreateNew { get; }
    }

    public interface IDataVirtualizingCollectionViewModel<T> : IDataVirtualizingCollectionViewModel
    {
        IDataVirtualizingCollection<T>? Items { get; }
    }

    public abstract class DataVirtualizingCollectionViewModel<TModel, TViewModel> : ObservableObject, IDisposable, IDataVirtualizingCollectionViewModel<TViewModel>
    {
        private readonly IGetSchedulers _getSchedulers;
        private readonly IBackendAccess<TViewModel> _backendAccess;
        private int _pageSize = 100;
        private IDataVirtualizingCollection<TViewModel>? _items;
        private readonly SerialDisposable _serialItems;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public DataVirtualizingCollectionViewModel(
            // parameters
            IBackendAccess<TModel> backendAccess,
            
            // dependencies
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel,
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel,
            IFetcherKindViewModelInternal fetcherKindViewModel,
            IGetSchedulers getSchedulers)
        {
            _getSchedulers = getSchedulers;
            PageLoadingBehaviorViewModel = pageLoadingBehaviorViewModel;
            PageRemovalBehaviorViewModel = pageRemovalBehaviorViewModel;
            FetcherKindViewModel = fetcherKindViewModel;
            IndexAccessBehaviorViewModel = fetcherKindViewModel.IndexAccessBehaviorViewModel;
            _backendAccess = TransformingBackendAccess<TModel, TViewModel>.CreateTransformingBackendAccess(
                backendAccess, 
                TransformPage,
                TransformPlaceholder);
            
            _serialItems = new SerialDisposable();
            _compositeDisposable.Add(_serialItems);

            var createNew = new RxRelayCommand(SetItems);
            CreateNew = createNew;
            _compositeDisposable.Add(createNew);
            
            SetItems();
        }

        public string Name => _backendAccess.Name;

        public IDataVirtualizingCollection<TViewModel>? Items
        {
            get => _items;
            private set
            {
                if (_items == value) return;
                _items = value;
                _serialItems.Disposable = _items;
                OnPropertyChanged();
            }
        }

        public IPageLoadingBehaviorViewModel PageLoadingBehaviorViewModel { get; }
        public IPageRemovalBehaviorViewModel PageRemovalBehaviorViewModel { get; }
        public IFetcherKindViewModel FetcherKindViewModel { get; }
        public IIndexAccessBehaviorViewModel IndexAccessBehaviorViewModel { get; }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize == value) return;
                _pageSize = value;
                OnPropertyChanged();
            }
        }

        public ICommand CreateNew { get; }

        protected abstract TViewModel[] TransformPage(TModel[] page);

        protected abstract TViewModel TransformPlaceholder(TModel item);

        private void SetItems()
        {
            var builder = DataVirtualizingCollectionBuilder<TViewModel>
                .Build(_pageSize);
            var afterPageLoadingDecision = 
                PageLoadingBehaviorViewModel.Configure(builder);
            var afterPageRemovalDecision = 
                PageRemovalBehaviorViewModel.Configure(afterPageLoadingDecision);
            var afterFetcherKindDecision = 
                FetcherKindViewModel.Configure(afterPageRemovalDecision, _backendAccess);
            var collection = IndexAccessBehaviorViewModel.Configure(
                afterFetcherKindDecision, 
                _backendAccess, 
                _getSchedulers.NotificationScheduler,
                _getSchedulers.BackgroundScheduler);
            Items = collection;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}