using System.Windows.Input;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Utility;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Functions
{
    public interface IGeneralFunctionsViewModel
    {
        ICommand Reset { get; }
    }

    public class GeneralFunctionsViewModel : ObservableObject, IGeneralFunctionsViewModel
    {
        public ICommand Reset { get; } = new RxRelayCommand<IVirtualizationBase>(dvc => dvc.Reset());
    }
}