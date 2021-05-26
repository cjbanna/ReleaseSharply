using ReleaseSharply.Server.Data;
using ReleaseSharply.Server.Models;
using System.Collections.Generic;
using System.Linq;

namespace ReleaseSharply.Server.Webapi.Sample.Data
{
    public class FeaturesDbSeedDataGenerator
    {
        private readonly FeaturesDbContext _dbContext;

        public FeaturesDbSeedDataGenerator(FeaturesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void SeedData()
        {
            if (_dbContext.Database.EnsureCreated())
            {
                var existing = _dbContext.FeatureGroups.SingleOrDefault(g => g.Name == "ConsoleFeatures");
                if (existing == null)
                {
                    _dbContext.FeatureGroups.Add(new FeatureGroup
                    {
                        Name = "ConsoleFeatures",
                        Features = new List<Feature>
                        {
                            new Feature
                            {
                                Name = "foo",
                                IsEnabled = true
                            },
                            new Feature
                            {
                                Name = "bar",
                                IsEnabled = false
                            }
                        }
                    });

                    _dbContext.SaveChanges();
                }
            }
        }
    }
}
