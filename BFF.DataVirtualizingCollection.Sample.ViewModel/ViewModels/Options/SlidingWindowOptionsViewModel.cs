namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Options
{
    public interface ISpecificOptionsViewModel
    {
    }

    public class SpecificOptionsViewModel : ObservableObject, ISpecificOptionsViewModel
    {
        public static SpecificOptionsViewModel Empty = new SpecificOptionsViewModel();
        
        protected SpecificOptionsViewModel()
        {}
    }
    
    public interface ISlidingWindowOptionsViewModel : ISpecificOptionsViewModel
    {
        int WindowSize { get; set; }
        
        int WindowOffset { get; set; }
    }
    
    public class SlidingWindowOptionsViewModel : SpecificOptionsViewModel, ISlidingWindowOptionsViewModel
    {
        private int _windowSize = 4;
        
        private int _windowOffset = 0;

        public int WindowSize
        {
            get => _windowSize;
            set
            {
                if (_windowSize == value) return;
                _windowSize = value;
                OnPropertyChanged();
            }
        }

        public int WindowOffset
        {
            get => _windowOffset;
            set
            {
                if (_windowOffset == value) return;
                _windowOffset = value;
                OnPropertyChanged();
            }
        }
    }
}