using Baitkm.DTO.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baitkm.DTO.ViewModels.Cities
{
    public class CityEditModel : IViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
