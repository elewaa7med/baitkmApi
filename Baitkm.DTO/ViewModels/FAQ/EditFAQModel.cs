using Baitkm.DTO.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baitkm.DTO.ViewModels.FAQ
{
    public class EditFAQModel : IViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
