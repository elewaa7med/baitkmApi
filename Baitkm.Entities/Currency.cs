using Baitkm.Entities.Base;

namespace Baitkm.Entities
{
    public class Currency : EntityBase
    {
        public int RequestId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int NumericCode { get; set; }
    }
}