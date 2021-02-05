namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Options
{
    public interface IGeneralOptionsViewModel
    {
        int PageSize { get; set; }
    }
    
    internal class GeneralOptionsViewModel : ObservableObject, IGeneralOptionsViewModel
    {
        private int _pageSize = 100;

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize == value) return;
                _pageSize = value;
                OnPropertyChanged();
            }
        }
    }
}