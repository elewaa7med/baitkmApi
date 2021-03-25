using Newtonsoft.Json;
using System.Collections.Generic;

namespace Baitkm.Infrastructure.Helpers.AnnouncementLocation
{
    public class UserLocationParsingBase
    {
        [JsonProperty("address_components")]
        public List<AddressComponentsModel> AddressComponents { get; set; }
        public GeometryModel Geometry { get; set; }
    }
}
