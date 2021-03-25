using Baitkm.DTO.ViewModels.Bases;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Baitkm.DTO.ViewModels.Persons.Authentication
{
    public class TokenProviderOptions : IViewModel
    {
        public string Path { get; set; } = "/Token";
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public TimeSpan Expiration { get; set; } = TimeSpan.FromDays(30);
        public SigningCredentials SigningCredentials { get; set; }
    }
}
