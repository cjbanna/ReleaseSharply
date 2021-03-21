using ReleaseSharply.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReleaseSharply.Server
{
    public interface IFeatureManager
    {
        Task<FeatureGroup> AddFeatureGroupAsync(FeatureGroup featureGroup);
        Task<Feature> AddFeatureAsync(Feature feature);
        Task<Feature> UpdateFeatureAsync(Feature feature);
        Task RemoveFeatureAsync(Feature feature);
        Task<IEnumerable<Feature>> GetFeaturesAsync(string featureGroup);
    }
}
