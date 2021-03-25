using Baitkm.Infrastructure.Validation.Attributes;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Bases
{
    public interface IGroupNotificationBase : INotificationBase
    {
        [PropertyNotMapped]
        List<int> UserIds { get; set; }
        [PropertyNotMapped]
        List<int> GuestIds { get; set; }
    }
}
