using Newtonsoft.Json;

namespace Baitkm.Infrastructure.Helpers.ParserHelpers
{
    public class ParsingAddressSecondaryModel
    {
        [JsonProperty("main_text")]
        public string Address { get; set; }
    }
}
