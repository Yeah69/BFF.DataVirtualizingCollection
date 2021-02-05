using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Adapters;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels
{
    public interface IMainWindowViewModel
    {
        IReadOnlyCollection<IDataVirtualizingCollectionViewModelBase> DataVirtualizingCollections { get; }
        
        IReadOnlyCollection<IDataVirtualizingCollectionViewModelBase> SlidingWindows { get; }
    }

    internal class MainWindowViewModel : ObservableObject, IMainWindowViewModel
    {
        public MainWindowViewModel(
            IAllNumbersCollectionAdapter allNumbersCollectionAdapter,
            IMillionNumbersCollectionAdapter millionNumbersCollectionAdapter,
            IHighWorkloadCollectionAdapter highWorkloadCollectionAdapter,
            IProfileCollectionAdapter profileCollectionAdapter,
            IDataVirtualizingCollectionViewModelFactory dataVirtualizingCollectionViewModelFactory)
        {
            DataVirtualizingCollections = new ReadOnlyCollection<IDataVirtualizingCollectionViewModelBase>(
                new List<IDataVirtualizingCollectionViewModelBase>
                {
                    dataVirtualizingCollectionViewModelFactory.CreateDataVirtualizingCollection(allNumbersCollectionAdapter),
                    dataVirtualizingCollectionViewModelFactory.CreateDataVirtualizingCollection(millionNumbersCollectionAdapter),
                    dataVirtualizingCollectionViewModelFactory.CreateDataVirtualizingCollection(highWorkloadCollectionAdapter),
                    dataVirtualizingCollectionViewModelFactory.CreateDataVirtualizingCollection(profileCollectionAdapter)
                });
            SlidingWindows = new ReadOnlyCollection<IDataVirtualizingCollectionViewModelBase>(
                new List<IDataVirtualizingCollectionViewModelBase>
                {
                    dataVirtualizingCollectionViewModelFactory.CreateSlidingWindow(allNumbersCollectionAdapter),
                    dataVirtualizingCollectionViewModelFactory.CreateSlidingWindow(millionNumbersCollectionAdapter),
                    dataVirtualizingCollectionViewModelFactory.CreateSlidingWindow(highWorkloadCollectionAdapter),
                    dataVirtualizingCollectionViewModelFactory.CreateSlidingWindow(profileCollectionAdapter)
                });
        }
        
        public IReadOnlyCollection<IDataVirtualizingCollectionViewModelBase> DataVirtualizingCollections { get; }
        
        public IReadOnlyCollection<IDataVirtualizingCollectionViewModelBase> SlidingWindows { get; }
    }
}