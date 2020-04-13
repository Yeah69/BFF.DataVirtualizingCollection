using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels
{
    
    public interface IObservableObject : INotifyPropertyChanged {}

    public abstract class ObservableObject : IObservableObject
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}