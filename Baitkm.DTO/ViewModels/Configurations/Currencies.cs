using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Configurations
{
    public class Currencies
    {
        public int USD { get; set; }
        public int RUB { get; set; }
        public int EUR { get; set; }
        public int AMD { get; set; }
        public int GEL { get; set; }
        public int GBP { get; set; }
        public int CAD { get; set; }
        public int CHF { get; set; }
        public int SEK { get; set; }
        public int CNY { get; set; }
        public int AED { get; set; }
        public int SAR { get; set; }
    }

    public class TestDic
    {
        public Dictionary<string, int> Val { get; set; }
    }
}
