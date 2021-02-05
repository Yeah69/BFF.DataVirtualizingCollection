using System.Linq;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
using BFF.DataVirtualizingCollection.Sample.Model.Models;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Utility;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.Adapters
{
    internal interface IProfileCollectionAdapter : IBackendAccessAdapter<IProfileViewModel>
    {
    }

    internal class ProfileCollectionAdapter : IProfileCollectionAdapter
    {
        private readonly IProfilesFakeBackendAccess _profilesFakeBackendAccess;

        public ProfileCollectionAdapter(IProfilesFakeBackendAccess profilesFakeBackendAccess)
        {
            _profilesFakeBackendAccess = profilesFakeBackendAccess;
            BackendAccess = TransformingBackendAccess<IProfile, IProfileViewModel>.CreateTransformingBackendAccess(
                _profilesFakeBackendAccess,
                TransformPage,
                TransformPlaceholder);
        }

        public string Name => _profilesFakeBackendAccess.Name;
        
        public IBackendAccess<IProfileViewModel> BackendAccess { get; }

        public BackendAccessKind BackendAccessKind => BackendAccessKind.Profiles;

        private IProfileViewModel[] TransformPage(IProfile[] page)
        {
            return page.Select(p => ProfileViewModelStatic.ProfileToViewModel[p]).ToArray();
        }

        private IProfileViewModel TransformPlaceholder(IProfile item)
        {
            return ProfileViewModelStatic.ProfileToViewModel[item];
        }
    }
}