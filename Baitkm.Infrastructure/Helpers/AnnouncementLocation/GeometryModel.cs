using Newtonsoft.Json;

namespace Baitkm.Infrastructure.Helpers.AnnouncementLocation
{
    public class GeometryModel
    {
        public LocationBaseModel Location { get; set; }
        [JsonProperty("location_type")]
        public string LocationType { get; set; }
    }
}
