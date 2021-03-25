using Baitkm.Entities.Base;
using Baitkm.Enums;

namespace Baitkm.Entities
{
    public class Configuration : EntityBase
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public Language Language { get; set; }
    }
}