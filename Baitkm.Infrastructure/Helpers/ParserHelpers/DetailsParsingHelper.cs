using Newtonsoft.Json;

namespace Baitkm.Infrastructure.Helpers.ParserHelpers
{
    public class DetailsParsingHelper
    {
        [JsonProperty("result")]
        public DetailsParisngResultModel DetailsParisngResultModel { get; set; }
    }
}
