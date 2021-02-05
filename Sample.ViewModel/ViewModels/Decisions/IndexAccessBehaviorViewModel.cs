using System;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;

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
        
        public TVirtualizationKind Configure<T, TVirtualizationKind>(
            IAsyncOnlyIndexAccessBehaviorCollectionBuilder<T, TVirtualizationKind> builder,
            IBackendAccess<T> backendAccess,
            FetcherKind fetcherDecision)
        {
            return IndexAccessBehavior == IndexAccessBehavior.Synchronous
                ? fetcherDecision == FetcherKind.NonTaskBased
                    ? builder is IIndexAccessBehaviorCollectionBuilder<T, TVirtualizationKind> indexAccessBehaviorCollectionBuilder
                        ? indexAccessBehaviorCollectionBuilder.SyncIndexAccess() 
                        : throw new ArgumentException("Builder should implement all builder interfaces", nameof(builder))
                    : throw new ArgumentException("Cannot choose sync index access when chosen to use task-based fetchers.")
                : builder.AsyncIndexAccess(backendAccess.PlaceholderFetch);
        }
    }

    internal class IndexAccessBehaviorViewModel : ObservableObject, IIndexAccessBehaviorViewModelInternal
    {
        private IndexAccessBehavior _indexAccessBehavior = IndexAccessBehavior.Asynchronous;
        private bool _isSyncEnabled;

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