using Microsoft.EntityFrameworkCore;
using ReleaseSharply.Server.Models;

namespace ReleaseSharply.Server.Data
{
    public class FeaturesDbContext : DbContext
    {
        public FeaturesDbContext(DbContextOptions<FeaturesDbContext> options)
            : base(options) { }

        public DbSet<Models.FeatureGroup> FeatureGroups { get; set; }
        public DbSet<Models.Feature> Features { get; set; }

    }
}
