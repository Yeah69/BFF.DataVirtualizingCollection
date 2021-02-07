using System.Windows.Input;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Utility;
using BFF.DataVirtualizingCollection.SlidingWindow;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Functions
{
    public interface ISpecificFunctionsViewModel
    {
    }

    public class SpecificFunctionsViewModel : ObservableObject, ISpecificFunctionsViewModel
    {
        public static SpecificFunctionsViewModel Empty = new();
        
        protected SpecificFunctionsViewModel()
        {}
    }
    
    public interface ISlidingWindowFunctionsViewModel : ISpecificFunctionsViewModel
    {
        ICommand SlideLeft { get; }
        ICommand SlideRight { get; }
        ICommand IncreaseWindowSize { get; }
        ICommand DecreaseWindowSize { get; }
    }

    public class SlidingWindowFunctionsViewModel : ISlidingWindowFunctionsViewModel
    {
        public ICommand SlideLeft { get; } = new RxRelayCommand<ISlidingWindow>(sw => sw.SlideLeft());
        public ICommand SlideRight { get; } = new RxRelayCommand<ISlidingWindow>(sw => sw.SlideRight());
        public ICommand IncreaseWindowSize { get; } = new RxRelayCommand<ISlidingWindow>(sw => sw.IncreaseWindowSize());
        public ICommand DecreaseWindowSize { get; } = new RxRelayCommand<ISlidingWindow>(sw => sw.DecreaseWindowSize());
    }
}