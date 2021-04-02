using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace ReleaseSharply.Server.Options
{
    public class ReleaseSharplyOptions
    {
        public ICollection<Client> Clients { get; set; } = new List<Client>();
        public SigningCredentials SigningCredentials { get; set; }
        public string SqlServerConnectionString { get; set; }
        public string Authority { get; set; }
    }
}
