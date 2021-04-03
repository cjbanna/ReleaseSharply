## Sql Server Setup

1. TODO: add appsettings.json value for migrations assembly? or have caller implement `DesignTimeDbContextFactoryBase`?

```
dotnet tool install --global dotnet-ef --version 3.1.13

dotnet ef migrations add InitialFeaturesDbMigration -c FeaturesDbContext -o Data/Migrations/ReleaseSharply/FeaturesDb -v

dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb -v

dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb -v

dotnet ef database update -c FeaturesDbContext
dotnet ef database update -c ConfigurationDbContext
dotnet ef database update -c ConfigurationDbContext
```