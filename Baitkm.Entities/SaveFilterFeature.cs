using Baitkm.Entities.Base;
using Baitkm.Enums.Attachments;

namespace Baitkm.Entities
{
    public class SaveFilterFeature : EntityBase
    {
        public int SaveFilterId { get; set; }
        public AnnouncementFeaturesType FeatureType { get; set; }

        public virtual SaveFilter SaveFilter { get; set; }
    }
}