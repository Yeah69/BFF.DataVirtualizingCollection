using System;
using System.Reactive.Concurrency;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Interfaces;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions
{
    public enum IndexAccessBehavior
    {
        Synchronous,
        Asynchronous
    }

    internal interface IIndexAccessBehaviorViewModelInternal : IIndexAccessBehaviorViewModel
    {
        void SetIsSyncEnabled(bool value);
    }

    public interface IIndexAccessBehaviorViewModel
    {
        IndexAccessBehavior IndexAccessBehavior { get; set; }
        
        bool IsSyncEnabled { get; }
        
        public IDataVirtualizingCollection<T> Configure<T>(
            IAsyncOnlyIndexAccessBehaviorCollectionBuilder<T> builder,
            IBackendAccess<T> backendAccess,
            IScheduler notificationScheduler,
            IScheduler backgroundScheduler)
        {
            return IndexAccessBehavior == IndexAccessBehavior.Synchronous
                ? (builder as IIndexAccessBehaviorCollectionBuilder<T>)?.SyncIndexAccess(notificationScheduler)
                    ?? throw new ArgumentException("Cannot choose sync index access when chosen to use task-based fetchers.")
                : builder.AsyncIndexAccess(
                    backendAccess.PlaceholderFetch, 
                    backgroundScheduler,
                    notificationScheduler);
        }
    }

    internal class IndexAccessBehaviorViewModel : ObservableObject, IIndexAccessBehaviorViewModelInternal
    {
        private readonly IGetSchedulers _getSchedulers;
        private IndexAccessBehavior _indexAccessBehavior = IndexAccessBehavior.Asynchronous;
        private bool _isSyncEnabled;

        public IndexAccessBehaviorViewModel(
            IGetSchedulers getSchedulers)
        {
            _getSchedulers = getSchedulers;
        }

        public IndexAccessBehavior IndexAccessBehavior
        {
            get => _indexAccessBehavior;
            set
            {
                if (_indexAccessBehavior == value) return;
                _indexAccessBehavior = value;
                OnPropertyChanged();
            }
        }

        public bool IsSyncEnabled
        {
            get => _isSyncEnabled;
            private set
            {
                if (value == _isSyncEnabled) return;
                _isSyncEnabled = value;
                OnPropertyChanged();

                if (!_isSyncEnabled)
                {
                    IndexAccessBehavior = IndexAccessBehavior.Asynchronous;
                }
            }
        }
        
        public void SetIsSyncEnabled(bool value)
        {
            IsSyncEnabled = value;
        }
    }
}