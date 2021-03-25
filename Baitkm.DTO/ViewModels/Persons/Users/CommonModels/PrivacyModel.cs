using Baitkm.DTO.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baitkm.DTO.ViewModels.Persons.Users.CommonModels
{
    public class PrivacyModel : IViewModel
    {
        public string Terms { get; set; }
        public string Privacy { get; set; }
    }
}
