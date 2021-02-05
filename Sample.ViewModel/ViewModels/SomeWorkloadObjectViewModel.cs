using BFF.DataVirtualizingCollection.Sample.Model.Models;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels
{
    public interface ISomeWorkloadObjectViewModel
    {
        public int Number { get; }
    }
    
    public class SomeWorkloadObjectViewModel : ISomeWorkloadObjectViewModel
    {
        private readonly ISomeWorkloadObject _someWorkloadObject;

        public SomeWorkloadObjectViewModel(
            ISomeWorkloadObject someWorkloadObject)
        {
            _someWorkloadObject = someWorkloadObject;
        }

        public int Number => _someWorkloadObject.Number;
    }
}