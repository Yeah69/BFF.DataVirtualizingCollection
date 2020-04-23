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

    public interface IIndexAccessBehaviorViewModel
    {
        IndexAccessBehavior IndexAccessBehavior { get; set; }
        
        public IDataVirtualizingCollection<T> Configure<T>(
            IIndexAccessBehaviorCollectionBuilder<T> builder,
            IBackendAccess<T> backendAccess,
            IScheduler notificationScheduler,
            IScheduler backgroundScheduler)
        {
            return IndexAccessBehavior == IndexAccessBehavior.Synchronous
                ? builder.SyncIndexAccess(notificationScheduler)
                : builder.AsyncIndexAccess(
                    backendAccess.PlaceholderFetch, 
                    backgroundScheduler,
                    notificationScheduler);
        }
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
    }
}