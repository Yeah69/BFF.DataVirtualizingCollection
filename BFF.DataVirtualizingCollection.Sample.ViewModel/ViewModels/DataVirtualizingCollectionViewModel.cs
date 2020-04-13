using System;
using System.Reactive.Disposables;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
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
    }

    public interface IDataVirtualizingCollectionViewModel<T> : IDataVirtualizingCollectionViewModel
    {
        IDataVirtualizingCollection<T>? Items { get; }
    }

    public abstract class DataVirtualizingCollectionViewModel<TModel, TViewModel> : ObservableObject, IDisposable, IDataVirtualizingCollectionViewModel<TViewModel>
    {
        private readonly IBackendAccess<TViewModel> _backendAccess;
        private int _pageSize = 100;
        private IDataVirtualizingCollection<TViewModel>? _items;
        private readonly SerialDisposable _serialItems = new SerialDisposable();

        public DataVirtualizingCollectionViewModel(
            // parameters
            IBackendAccess<TModel> backendAccess,
            
            // dependencies
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel,
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel,
            IFetcherKindViewModel fetcherKindViewModel,
            IIndexAccessBehaviorViewModel indexAccessBehaviorViewModel)
        {
            PageLoadingBehaviorViewModel = pageLoadingBehaviorViewModel;
            PageRemovalBehaviorViewModel = pageRemovalBehaviorViewModel;
            FetcherKindViewModel = fetcherKindViewModel;
            IndexAccessBehaviorViewModel = indexAccessBehaviorViewModel;
            _backendAccess = TransformingBackendAccess<TModel, TViewModel>.CreateTransformingBackendAccess(
                backendAccess, 
                TransformPage,
                TransformPlaceholder);
            
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
            var collection = IndexAccessBehaviorViewModel.Configure(afterFetcherKindDecision, _backendAccess);
            Items = collection;
        }

        public void Dispose()
        {
            _serialItems.Dispose();
        }
    }
}