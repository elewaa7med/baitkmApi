using Baitkm.DTO.ViewModels.Bases;
using Newtonsoft.Json;
using System;

namespace Baitkm.DTO.ViewModels.Persons.Users.CommonModels
{
    public class StatisticViewModel : IStoredProcedureResponse
    {
        public DateTime Day { get; set; }
        public float Duration { get; set; }
        [JsonIgnore]
        public int Iterator { get; set; }
        public int UserCount { get; set; }
    }
}
