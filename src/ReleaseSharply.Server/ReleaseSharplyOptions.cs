using ReleaseSharply.Server.Models;
using System;
using System.Collections.Generic;

namespace ReleaseSharply.Server
{
    public class ReleaseSharplyOptions
    {
        public Func<string, IEnumerable<Feature>> FeatureGroupProvider { get; set; }
    }
}
