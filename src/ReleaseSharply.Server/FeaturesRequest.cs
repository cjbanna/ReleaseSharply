using ReleaseSharply.Server.Models;
using System.Collections.Generic;

namespace ReleaseSharply.Server
{
    public class FeaturesRequest
    {
        public string FeatureGroup { get; set; }
        public IEnumerable<Feature> Features { get; set; }
    }
}
