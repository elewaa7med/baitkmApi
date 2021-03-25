using Baitkm.DTO.ViewModels.Bases;
using System;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Persons.Users
{
    public class UserEditModel : IViewModel
    {
        [Required]
        //[RegularExpression(@"^[a-zA-Z]+$")] // TO DO check-it's work
        public string FullName { get; set; }
        public string PhoneEmail { get; set; }
        public string PhoneCode { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
    }
}
