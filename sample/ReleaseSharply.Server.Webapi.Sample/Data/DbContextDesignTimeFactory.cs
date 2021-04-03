using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using ReleaseSharply.Server.Data;
using System.Reflection;

namespace ReleaseSharply.Server.Webapi.Sample.Data
{
    public class ConfigurationContextDesignTimeFactory : DesignTimeDbContextFactoryBase<ConfigurationDbContext>
    {
        public ConfigurationContextDesignTimeFactory()
            : base("ReleaseSharplyConnectionString", typeof(ConfigurationContextDesignTimeFactory).GetTypeInfo().Assembly.GetName().Name)
        {
        }

        protected override ConfigurationDbContext CreateNewInstance(DbContextOptions<ConfigurationDbContext> options)
        {
            return new ConfigurationDbContext(options, new ConfigurationStoreOptions());
        }
    }

    public class PersistedGrantContextDesignTimeFactory : DesignTimeDbContextFactoryBase<PersistedGrantDbContext>
    {
        public PersistedGrantContextDesignTimeFactory()
            : base("ReleaseSharplyConnectionString", typeof(PersistedGrantContextDesignTimeFactory).GetTypeInfo().Assembly.GetName().Name)
        {
        }

        protected override PersistedGrantDbContext CreateNewInstance(DbContextOptions<PersistedGrantDbContext> options)
        {
            return new PersistedGrantDbContext(options, new OperationalStoreOptions());
        }
    }

    public class FeaturesDbContextDesignTimeFactory : DesignTimeDbContextFactoryBase<FeaturesDbContext>
    {
        public FeaturesDbContextDesignTimeFactory()
            : base("ReleaseSharplyConnectionString", typeof(FeaturesDbContextDesignTimeFactory).GetTypeInfo().Assembly.GetName().Name)
        {
        }

        protected override FeaturesDbContext CreateNewInstance(DbContextOptions<FeaturesDbContext> options)
        {
            return new FeaturesDbContext(options);
        }
    }
}
