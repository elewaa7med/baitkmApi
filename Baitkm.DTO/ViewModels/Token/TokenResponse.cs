using Baitkm.Enums;
using System;

namespace Baitkm.DTO.ViewModels.Token
{
    public class TokenResponse 
    {
        public int Id { get; set; } 
        public string AccessToken { get; set; }
        public DateTime DateTime { get; set; }
        public SocialLoginProvider Provider { get; set; }
    }
}
