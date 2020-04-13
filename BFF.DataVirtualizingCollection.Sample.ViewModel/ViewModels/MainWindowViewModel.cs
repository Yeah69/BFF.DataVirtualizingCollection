using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.CollectionViewModels;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels
{
    public interface IMainWindowViewModel
    {
        IReadOnlyCollection<IDataVirtualizingCollectionViewModel> DataVirtualizingCollections { get; }
    }

    internal class MainWindowViewModel : ObservableObject, IMainWindowViewModel
    {
        public MainWindowViewModel(
            IAllNumbersCollectionViewModel allNumbersCollectionViewModel,
            IMillionNumbersCollectionViewModel millionNumbersCollectionViewModel,
            IHighWorkloadCollectionViewModel highWorkloadCollectionViewModel,
            IProfileCollectionViewModel profileCollectionViewModel)
        {
            DataVirtualizingCollections = new ReadOnlyCollection<IDataVirtualizingCollectionViewModel>(
                new List<IDataVirtualizingCollectionViewModel>
                {
                    allNumbersCollectionViewModel,
                    millionNumbersCollectionViewModel,
                    highWorkloadCollectionViewModel,
                    profileCollectionViewModel
                });
        }
        
        public IReadOnlyCollection<IDataVirtualizingCollectionViewModel> DataVirtualizingCollections { get; }
    }
}