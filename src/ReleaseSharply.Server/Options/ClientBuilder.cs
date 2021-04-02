using IdentityServer4.Models;

namespace ReleaseSharply.Server.Options
{
    public class ClientBuilder
    {
        protected Client Client;

        private ClientBuilder()
        {
            Client = new Client
            {
                AllowedGrantTypes = GrantTypes.ClientCredentials
            };
        }

        public static ClientBuilder Create() => new ClientBuilder();

        public ClientBuilder WithClientId(string clientId)
        {
            Client.ClientId = clientId;
            return this;
        }

        public ClientBuilder WithSecret(string secret)
        {
            Client.ClientSecrets.Add(new Secret(secret.Sha256()));
            return this;
        }

        public ClientBuilder WithSha256Secret(string hashedSecret)
        {
            Client.ClientSecrets.Add(new Secret(hashedSecret));
            return this;
        }

        public ClientBuilder WithReadScope()
        {
            Client.AllowedScopes.Add(Scopes.Read);
            return this;
        }

        public ClientBuilder WithWriteScope()
        {
            Client.AllowedScopes.Add(Scopes.Write);
            return this;
        }

        public Client Build() => Client;
    }
}
