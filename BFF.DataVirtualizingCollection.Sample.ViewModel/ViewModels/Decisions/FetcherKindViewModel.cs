using System.Threading;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.Sample.Model;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions
{
    public enum FetcherKind
    {
        NonTaskBased,
        TaskBased
    }

    public interface IFetcherKindViewModel
    {
        FetcherKind FetcherKind { get; set; }
        IIndexAccessBehaviorCollectionBuilder<T> Configure<T>(
            IFetchersKindCollectionBuilder<T> builder, 
            IBackendAccess<T> backendAccess);
    }

    internal class FetcherKindViewModel : ObservableObject, IFetcherKindViewModel
    {
        private FetcherKind _fetcherKind = FetcherKind.TaskBased;
        private int _delayPageFetcherInMilliseconds = 2000;
        private int _delayCountFetcherInMilliseconds = 2000;

        public FetcherKind FetcherKind
        {
            get => _fetcherKind;
            set
            {
                if (_fetcherKind == value) return;
                _fetcherKind = value;
                OnPropertyChanged();
            }
        }

        public int DelayPageFetcherInMilliseconds
        {
            get => _delayPageFetcherInMilliseconds;
            set
            {
                if (_delayPageFetcherInMilliseconds == value) return;
                _delayPageFetcherInMilliseconds = value;
                OnPropertyChanged();
            }
        }

        public int DelayCountFetcherInMilliseconds
        {
            get => _delayCountFetcherInMilliseconds;
            set
            {
                if (_delayCountFetcherInMilliseconds == value) return;
                _delayCountFetcherInMilliseconds = value;
                OnPropertyChanged();
            }
        }

        public IIndexAccessBehaviorCollectionBuilder<T> Configure<T>(
            IFetchersKindCollectionBuilder<T> builder, 
            IBackendAccess<T> backendAccess)
        {
            return _fetcherKind == FetcherKind.NonTaskBased
                ? builder.NonTaskBasedFetchers(
                    (offset, size) =>
                    {
                        Thread.Sleep(_delayPageFetcherInMilliseconds);
                        return backendAccess.PageFetch(offset, size);
                    },
                    () =>
                    {
                        Thread.Sleep(_delayCountFetcherInMilliseconds);
                        return backendAccess.CountFetch();
                    })
                : builder.TaskBasedFetchers(
                    async (offset, size) =>
                    {
                        await Task.Delay(_delayPageFetcherInMilliseconds);
                        return backendAccess.PageFetch(offset, size);
                    },
                    async () =>
                    {
                        await Task.Delay(_delayCountFetcherInMilliseconds);
                        return backendAccess.CountFetch();
                    });
        }
    }
}