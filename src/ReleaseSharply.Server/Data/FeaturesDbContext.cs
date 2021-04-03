using Microsoft.EntityFrameworkCore;
using ReleaseSharply.Server.Models;

namespace ReleaseSharply.Server.Data
{
    public class FeaturesDbContext : DbContext
    {
        public FeaturesDbContext(DbContextOptions<FeaturesDbContext> options)
            : base(options) { }

        public DbSet<FeatureGroup> FeatureGroups { get; set; }
        public DbSet<Feature> Features { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feature>()
                .HasIndex(f => new { f.Name, f.FeatureGroupId }).IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
