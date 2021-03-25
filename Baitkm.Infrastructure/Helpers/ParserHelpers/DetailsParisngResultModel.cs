using Newtonsoft.Json;
using System.Collections.Generic;

namespace Baitkm.Infrastructure.Helpers.ParserHelpers
{
    public class DetailsParisngResultModel
    {
        [JsonProperty("address_components")]
        public List<DetailsParsingAdressComponents> AdressComponents { get; set; }
    }
}
