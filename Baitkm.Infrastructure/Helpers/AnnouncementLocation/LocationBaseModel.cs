using Newtonsoft.Json;

namespace Baitkm.Infrastructure.Helpers.AnnouncementLocation
{
    public struct LocationBaseModel
    {
        public decimal Lat { get; set; }
        [JsonProperty("lng")]
        public decimal Lng { get; set; }
    }
}
