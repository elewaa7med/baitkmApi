using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Activities
{
    public class ActivityService : IActivityService
    {
        private readonly IEntityRepository repository;

        public ActivityService(IEntityRepository repository)

        {
            this.repository = repository;
        }

        public async Task<IEnumerable<ActivityResponseModel>> GetByUserId(int callerId, string deviceId)
        {
            int userId = 0;
            bool isGuest = false;
            if (callerId != 0)
            {
                User user = await repository.FilterAsNoTracking<User>(u => u.Id == callerId).FirstOrDefaultAsync();
                userId = user.Id;
            }
            else
            {
                if (!string.IsNullOrEmpty(deviceId))
                {
                    Guest guest = await repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                    userId = guest.Id;
                    isGuest = true;
                }
            }
            var result = repository.FilterAsNoTracking<Fact>(f => f.UserId == userId && f.IsGuest == isGuest)
                .Include(f => f.Announcement).OrderByDescending(f => f.UpdatedDt);
            foreach (var r in result)
            {
                if (r.AnnouncementPhoto == null && r.Announcement.AnnouncementResidentialType.HasValue)
                    r.AnnouncementPhoto = DefaultCoverImgePath(r.Announcement.AnnouncementResidentialType.Value);
                if (r.AnnouncementPhoto == null && r.Announcement.CommercialType.HasValue)
                    r.AnnouncementPhoto = DefaultCommercialIMagePath(r.Announcement.CommercialType.Value);
                if (r.AnnouncementPhoto == null && r.Announcement.AnnouncementEstateType == AnnouncementEstateType.Land)
                    r.AnnouncementPhoto = DefaultLandMagePath();
            }
            return result.Select(s => new ActivityResponseModel
            {
                ActivityType = s.ActivityType,
                AnnouncementId = s.AnnouncementId,
                CreatedDt = s.CreatedDt,
                Photo = s.AnnouncementPhoto
            });
        }

        public async Task AddOrUpdate(Announcement announcement, int userId, bool isGuest, ActivityType activityType)
        {
            Fact fact = repository.Filter<Fact>(f => f.AnnouncementId == announcement.Id && f.UserId == userId
                && f.IsGuest == isGuest && f.ActivityType == activityType).FirstOrDefault();

            if (fact == null)
            {
                repository.Create<Fact>(new Fact
                {
                    ActivityType = activityType,
                    AnnouncementId = announcement.Id,
                    AnnouncementPhoto = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                          UploadType.AnnouncementBasePhoto, announcement.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                    IsGuest = isGuest,
                    UserId = userId
                });
            }
            else
            {
                fact.UpdatedDt = DateTime.UtcNow;
            }

            await repository.SaveChangesAsync();
        }

        private async Task<int> CalculateRating(int announcementId)
        {
            var ratings = await repository.Filter<Rating>(r => r.AnnouncementId == announcementId).Select(r => r.Rat).ToListAsync();
            int u = 0;
            foreach (var r in ratings)
                u += r;
            return u / (ratings.Count() + 1);
        }

        private string DefaultCoverImgePath(AnnouncementResidentialType type)
        {
            switch (type)
            {
                case AnnouncementResidentialType.Apartment:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/appartament.png/1500/1500/False/0";
                case AnnouncementResidentialType.Building:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/building.png/1500/1500/False/0";
                case AnnouncementResidentialType.Chalet:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/chalet.png/1500/1500/False/0";
                case AnnouncementResidentialType.Compound:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/compound.png/1500/1500/False/0";
                case AnnouncementResidentialType.Duplex:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/duplex.png/1500/1500/False/0";
                case AnnouncementResidentialType.FarmHouse:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/farmhouse.png/1500/1500/False/0";
                case AnnouncementResidentialType.Studio:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/studio.png/1500/1500/False/0";
                case AnnouncementResidentialType.Tower:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/tower.png/1500/1500/False/0";
                case AnnouncementResidentialType.Villa:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/villa.png/1500/1500/False/0";
                default:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/appartament.png/1500/1500/False/0";
            }
        }

        private string DefaultCommercialIMagePath(CommercialType type)
        {
            switch (type)
            {
                case CommercialType.OfficeSpace:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/office.png/1500/1500/False/0";
                case CommercialType.Shop:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/shop.png/1500/1500/False/0";
                case CommercialType.Showroom:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/showroom.png/1500/1500/False/0";
                case CommercialType.WereHouse:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/werehouse.png/1500/1500/False/0";
                default:
                    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/office.png/1500/1500/False/0";
            }
        }

        private string DefaultLandMagePath()
        {
            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/land.png/1500/1500/False/0";
        }
    }
}
