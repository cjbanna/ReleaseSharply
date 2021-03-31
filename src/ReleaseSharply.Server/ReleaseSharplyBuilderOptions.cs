using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace ReleaseSharply.Server
{
    public class ReleaseSharplyBuilderOptions
    {
        public ICollection<Client> Clients { get; set; }
        public SigningCredentials SigningCredentials { get; set; }
        public string SqlServerConnectionString { get; set; }
        public string Authority { get; set; }
    }
}
