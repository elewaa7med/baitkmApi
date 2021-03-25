using System;
using System.Collections.Generic;
using System.Text;

namespace Baitkm.DTO.ViewModels.Persons.Authentication
{
    public class TokenResponseModel
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
