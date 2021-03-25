using Baitkm.Entities.Base;
using System;

namespace Baitkm.Entities
{
    public class Statistic : EntityBase
    {
        public int UserId { get; set; }
        public DateTime ActivityDate { get; set; }

        public virtual User User { get; set; }
    }
}