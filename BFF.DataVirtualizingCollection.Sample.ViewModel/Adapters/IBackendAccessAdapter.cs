using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.Adapters
{
    public enum BackendAccessKind
    {
        AllNumbers,
        HighWorkload,
        MillionNumbers,
        Profiles
    }
    
    public interface IBackendAccessAdapter<TViewModel>
    {
        string Name { get; }
        
        IBackendAccess<TViewModel> BackendAccess { get; }
        
        BackendAccessKind BackendAccessKind { get; }
    }
}