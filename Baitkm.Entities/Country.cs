using Baitkm.Entities.Base;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class Country : EntityBase
    {
        public string Name { get; set; }

        public virtual ICollection<City> Cities { get; set; }
    }
}