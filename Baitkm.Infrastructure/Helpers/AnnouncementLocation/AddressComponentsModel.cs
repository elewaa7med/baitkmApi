using Newtonsoft.Json;
using System.Collections.Generic;

namespace Baitkm.Infrastructure.Helpers.AnnouncementLocation
{
    public class AddressComponentsModel
    {
        [JsonProperty("long_name")]
        public string Name { get; set; }
        public List<string> Types { get; set; }
    }
}
