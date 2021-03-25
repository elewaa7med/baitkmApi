using Newtonsoft.Json;

namespace Baitkm.Infrastructure.Helpers.AnnouncementLocation
{
    public class AnnouncementLocateModel
    {
        public string BuildingNumber { get; set; }
        public string StreetName { get; set; }
        public string Borough { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }

        [JsonProperty("lon")]
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
    }
}