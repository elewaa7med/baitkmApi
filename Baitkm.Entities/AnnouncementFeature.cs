using Baitkm.Entities.Base;
using Baitkm.Enums.Attachments;

namespace Baitkm.Entities
{
    public class AnnouncementFeature : EntityBase
    {
        public int AnnouncementId { get; set; }
        public AnnouncementFeaturesType FeatureType { get; set; }

        public virtual Announcement Announcement { get; set; }
    }
}