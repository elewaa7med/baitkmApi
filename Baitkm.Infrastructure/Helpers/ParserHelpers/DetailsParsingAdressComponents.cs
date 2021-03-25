using Newtonsoft.Json;
using System.Collections.Generic;

namespace Baitkm.Infrastructure.Helpers.ParserHelpers
{
    public class DetailsParsingAdressComponents
    {
        [JsonProperty("long_name")]
        public string Name { get; set; }
        [JsonProperty("types")]
        public List<string> Types { get; set; }
    }
}
