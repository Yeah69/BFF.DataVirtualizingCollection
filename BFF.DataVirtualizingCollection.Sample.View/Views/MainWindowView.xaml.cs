using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels;

namespace BFF.DataVirtualizingCollection.Sample.View.Views
{
    public partial class MainWindowView
    {
        public MainWindowView(IMainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();

            DataContext = mainWindowViewModel;
        }
    }
}