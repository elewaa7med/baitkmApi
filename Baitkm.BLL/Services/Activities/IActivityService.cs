using Baitkm.DTO.ViewModels;
using Baitkm.Entities;
using Baitkm.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Activities
{
    public interface IActivityService
    {
        Task<IEnumerable<ActivityResponseModel>> GetByUserId(int userId, string deviceToken);
        Task AddOrUpdate(Announcement announcement, int userId, bool isGuest, ActivityType activityType);
    }
}
