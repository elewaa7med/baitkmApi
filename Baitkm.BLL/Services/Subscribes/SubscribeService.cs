using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Subscribes;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Subscribes
{
    public class SubscribeService : ISubscribeService
    {
        private readonly IEntityRepository repository;
        private readonly IOptionsBinder optionsBinder;

        public SubscribeService(IEntityRepository repository,
            IOptionsBinder optionsBinder)
        {
            this.repository = repository;
            this.optionsBinder = optionsBinder;
        }

        public async Task<bool> SubscribeAsync(AddSubscribeRequestModel model, int userId, string deviceId, Language language)
        {
            Guest guest = null;
            SubscribeAnnouncement subscribeAnnouncement = null;
            var user = await repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(optionsBinder.Error().NoGuest);
            }
            var announcement = await repository.FilterAsNoTracking<Announcement>(a => a.Id == model.AnnouncementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(optionsBinder.Error().AnnouncementNotFound);
            if (user != null)
                subscribeAnnouncement = await repository.Filter<SubscribeAnnouncement>(s => s.UserId == user.Id
                    && s.AnnouncementId == announcement.Id).FirstOrDefaultAsync();
            else
                subscribeAnnouncement = await repository.Filter<SubscribeAnnouncement>(s => s.GuestId == guest.Id
                    && s.AnnouncementId == announcement.Id).FirstOrDefaultAsync();
            if (subscribeAnnouncement != null)
                throw new Exception("Subscribe Announcement already exist");

            repository.Create(new SubscribeAnnouncement
            {
                User = user ?? null,
                Guest = guest ?? null,
                AnnouncementId = announcement.Id,
                Email = model.Email,
                Address = announcement.AddressEn,
                AnnouncementType = announcement.AnnouncementType,
                AnnouncementEstateType = announcement.AnnouncementEstateType
            });
            await repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnSubscribeAsync(int announcementId, int userId, string deviceId, Language language)
        {
            Guest guest = null;
            SubscribeAnnouncement subscribeAnnouncement = null;
            var user = await repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(optionsBinder.Error().UserNotFound);
            }
            if (user != null)
                subscribeAnnouncement = await repository.Filter<SubscribeAnnouncement>(s => s.UserId == user.Id
                && s.AnnouncementId == announcementId).FirstOrDefaultAsync();
            else
                subscribeAnnouncement = await repository.Filter<SubscribeAnnouncement>(s => s.GuestId == guest.Id
                    && s.AnnouncementId == announcementId).FirstOrDefaultAsync();
            if (subscribeAnnouncement == null)
                throw new Exception("Subscribe Announcement not found");
            repository.Remove(subscribeAnnouncement);
            await repository.SaveChangesAsync();
            return true;
        }
    }
}