using System;

namespace ReleaseSharply.Server.Options
{
    public static class ReleaseSharplyOptionsExtensions
    {
        public static ReleaseSharplyOptions AddInMemoryClient(this ReleaseSharplyOptions options, Action<ClientBuilder> configure)
        {
            var builder = ClientBuilder.Create();
            configure(builder);
            options.Clients.Add(builder.Build());
            return options;
        }
    }
}
