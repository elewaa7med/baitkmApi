using Newtonsoft.Json;

namespace Baitkm.DTO.ViewModels.Helpers.Matrix.ParserHelpers
{
    public class ParsingAddressSecondaryModel
    {
        [JsonProperty("main_text")]
        public string Address { get; set; }
    }
}
