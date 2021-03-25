using Newtonsoft.Json;
using System.Collections.Generic;

namespace Baitkm.Infrastructure.Helpers.ParserHelpers
{
    public class ParsingBaseModel
    {
        [JsonProperty("structured_formatting")]
        public ParsingAddressSecondaryModel AddressModel { get; set; }
        public List<string> Types { get; set; }
        public List<ParsingTermsModel> Terms { get; set; }
        [JsonProperty("place_id")]
        public string PlaceId { get; set; }
    }
}
