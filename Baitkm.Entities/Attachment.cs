using Baitkm.Entities.Base;
using Baitkm.Enums.Attachments;

namespace Baitkm.Entities
{
    public class Attachment : EntityBase
    {
        //public int AnnouncementId { get; set; }
        //public string Photo { get; set; }
        //public AttachmentType AttachmentType { get; set; }
        ////public string DocumentImage { get; set; }
        //public virtual Announcement Announcement { get; set; }
        public string File { get; set; }
        //public string Photo { get; set; }
        public AttachmentType AttachmentType { get; set; }
        public UploadType UploadType { get; set; }

        public int? AnnouncementId { get; set; }

        public virtual Announcement Announcement { get; set; }
    }
}