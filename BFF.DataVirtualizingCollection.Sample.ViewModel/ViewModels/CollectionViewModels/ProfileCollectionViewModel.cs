using System.Linq;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
using BFF.DataVirtualizingCollection.Sample.Model.Models;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Interfaces;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.CollectionViewModels
{
    public interface IProfileCollectionViewModel : IDataVirtualizingCollectionViewModel<IProfileViewModel>
    {
    }

    public class ProfileCollectionViewModel : DataVirtualizingCollectionViewModel<IProfile, IProfileViewModel>, IProfileCollectionViewModel
    {
        public ProfileCollectionViewModel(
            IProfilesFakeBackendAccess backendAccess,
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel, 
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel, 
            IFetcherKindViewModelInternal fetcherKindViewModel, 
            IGetSchedulers getSchedulers) 
            : base(
                backendAccess, 
                pageLoadingBehaviorViewModel, 
                pageRemovalBehaviorViewModel, 
                fetcherKindViewModel, 
                getSchedulers)
        {
        }

        protected override IProfileViewModel[] TransformPage(IProfile[] page)
        {
            return page.Select(p => ProfileViewModelStatic.ProfileToViewModel[p]).ToArray();
        }

        protected override IProfileViewModel TransformPlaceholder(IProfile item)
        {
            return ProfileViewModelStatic.ProfileToViewModel[item];
        }
    }
}