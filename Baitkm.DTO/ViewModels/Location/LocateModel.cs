using Baitkm.DTO.ViewModels.Bases;
using Newtonsoft.Json;

namespace Baitkm.DTO.ViewModels.Location
{
    public class LocateModel : IViewModel
    {
        public int UserId { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        [JsonProperty("lon")]
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public string Ip { get; set; }
        public string Currency { get; set; }
    }
}