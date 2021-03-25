using Baitkm.Entities.Base;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class City : EntityBase
    {
        public string Name { get; set; }
        public int CountryId { get; set; }

        public virtual ICollection<User> Users { get; set; }
        public virtual Country Country { get; set; }
    }
}