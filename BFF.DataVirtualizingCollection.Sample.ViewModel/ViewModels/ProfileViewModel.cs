using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.DataVirtualizingCollection.Sample.Model.Models;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels
{
    public interface IProfileViewModel
    {
    }

    internal static class ProfileViewModelStatic
    {
        internal static IReadOnlyDictionary<IProfile, IProfileViewModel> ProfileToViewModel { get; } =
            new ReadOnlyDictionary<IProfile, IProfileViewModel>(
                new Dictionary<IProfile, IProfileViewModel>(
                    ProfileStatic
                        .ProfilePool
                        .Select(p => new KeyValuePair<IProfile, IProfileViewModel>(p, new ProfileViewModel(p)))
                        .Concat(new []{ new KeyValuePair<IProfile, IProfileViewModel>(ProfileStatic.Empty, EmptyProfileViewModel.Instance) })));
    }

    public class EmptyProfileViewModel : IProfileViewModel
    {
        public static EmptyProfileViewModel Instance { get; } = new EmptyProfileViewModel();
        
        private EmptyProfileViewModel(){}
    }

    public class ProfileViewModel : IProfileViewModel
    {
        private readonly IProfile _profile;

        public ProfileViewModel(
            IProfile profile)
        {
            _profile = profile;
        }

        public string Occupation => _profile.Occupation;

        public string Salary => _profile.Salary;

        public string Name => _profile.Name;

        public string Description => _profile.Description;

        public bool IsAvailable => _profile.IsAvailable;

        public bool IsFreelancer => _profile.IsFreelancer;

        public string? CompanyName => _profile.CompanyName;

        public IReadOnlyList<string> Abilities => _profile.Abilities;

        public int HiddenAbilitiesCount => _profile.HiddenAbilitiesCount;

        public string PicturePath => _profile.PicturePath;
    }
}