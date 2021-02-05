using System.Linq;
using BFF.DataVirtualizingCollection.Sample.Model.Models;

namespace BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses
{
    public interface IProfilesFakeBackendAccess : IBackendAccess<IProfile>
    {
    }
    
    internal class ProfilesFakeBackendAccess : IProfilesFakeBackendAccess
    {
        
        public string Name => "Profiles";
    
        public IProfile[] PageFetch(int pageOffset, int pageSize)
        {
            return Enumerable
                .Range(pageOffset, pageSize)
                .Select(i => ProfileStatic.ProfilePool[i % ProfileStatic.ProfilePool.Count])
                .ToArray();
        }

        public IProfile PlaceholderFetch(int _, int __)
        {
            return ProfileStatic.Empty;
        }

        public IProfile PreloadingPlaceholderFetch(int pageOffset, int indexInsidePage)
        {
            return ProfileStatic.Preloading;
        }

        public int CountFetch()
        {
            return 420420;
        }
    }
}