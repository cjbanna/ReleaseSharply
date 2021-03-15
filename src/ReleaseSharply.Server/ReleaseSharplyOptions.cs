using System;
using System.Collections.Generic;
using System.Text;

namespace ReleaseSharply.Server
{
    public class ReleaseSharplyOptions
    {
        public Func<string, IEnumerable<Feature>> FeatureProvider { get; set; }
    }
}
