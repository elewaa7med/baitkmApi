using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Configurations
{
    public class ConfigurationViewModel : IViewModel
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}