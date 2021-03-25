using System;

namespace Baitkm.Entities.Base
{
    public class EntityBase
    {
        public int Id { get; set; }
        public DateTime CreatedDt { get; set; }
        public DateTime UpdatedDt { get; set; }
        public bool IsDeleted { get; set; }

        //Delete after db update
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
    }
}