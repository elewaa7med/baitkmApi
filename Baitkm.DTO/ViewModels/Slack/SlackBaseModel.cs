using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Slack
{
    public class SlackBaseModel
    {
        public string Channel { get; set; }
        public string Username { get; set; }
        public List<SlackAttachmentModel> Attachments { get; set; }
    }
}
