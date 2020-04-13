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

    public interface IIndexAccessBehaviorViewModel
    {
        IndexAccessBehavior IndexAccessBehavior { get; set; }
        IDataVirtualizingCollection<T> Configure<T>(
            IIndexAccessBehaviorCollectionBuilder<T> builder,
            IBackendAccess<T> backendAccess);
    }

    internal class IndexAccessBehaviorViewModel : ObservableObject, IIndexAccessBehaviorViewModel
    {
        private readonly IGetSchedulers _getSchedulers;
        private IndexAccessBehavior _indexAccessBehavior = IndexAccessBehavior.Asynchronous;

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

        public IDataVirtualizingCollection<T> Configure<T>(
            IIndexAccessBehaviorCollectionBuilder<T> builder,
            IBackendAccess<T> backendAccess)
        {
            return _indexAccessBehavior == IndexAccessBehavior.Synchronous
                ? builder.SyncIndexAccess(_getSchedulers.NotificationScheduler)
                : builder.AsyncIndexAccess(
                    backendAccess.PlaceholderFetch, 
                    _getSchedulers.BackgroundScheduler,
                    _getSchedulers.NotificationScheduler);
        }
    }
}