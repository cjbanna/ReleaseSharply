using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace ReleaseSharply.Server
{
    public class ReleaseSharplyBuilderOptions
    {
        public ICollection<ApiResource> ApiResources { get; set; }
        public ICollection<ApiScope> ApiScopes { get; set; }
        public SigningCredentials SigningCredentials { get; set; }
        public ICollection<Client> Clients { get; set; }
    }
}
