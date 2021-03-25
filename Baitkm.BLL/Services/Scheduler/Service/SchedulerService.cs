using Baitkm.DAL.Context;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DAL.Repository.Firebase;
using Baitkm.DTO.ViewModels;
using Baitkm.DTO.ViewModels.Notifications;
using Baitkm.DTO.ViewModels.PushNotifications;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Notifications;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Scheduler.Service
{
    public class SchedulerService : ISchedulerService
    {
        public async Task RemoveFromTop()
        {
            using (var repository = new EntityRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
            {
                var announcements = await repository.Filter<Announcement>(x =>
                    !x.IsDraft && x.TopAnnouncement).ToListAsync();
                if (announcements == null)
                    return;

                announcements = announcements.Where(x => x.TopAnnouncementDayTo.Date == DateTime.Today.Date).ToList();
                foreach (var item in announcements)
                {
                    item.TopAnnouncement = false;
                    repository.Update(item);
                }
                await repository.SaveChangesAsync();
            }
        }

        public async Task Deactivate()
        {
            //TODO for TOP ANNOUNCEMENT
            using (var repository = new EntityRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
            {
                var announcements = await repository.Filter<Announcement>(x => !x.IsDraft && x.AnnouncementStatus == AnnouncementStatus.Accepted).ToListAsync();
                if (announcements == null)
                    return;
                announcements = announcements.Where(x => x.AnnouncementStatusLastDay.Date == DateTime.Today.Date).ToList();
                foreach (var item in announcements)
                {
                    item.AnnouncementStatus = AnnouncementStatus.Expired;
                    repository.Update(item);
                }
                await repository.SaveChangesAsync();
            }
        }

        public async Task Expired()
        {
            using (var repository = new EntityRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
            {
                var announcements = await repository
                    .Filter<Announcement>(x => !x.IsDraft && x.AnnouncementStatus == AnnouncementStatus.Accepted && x.AnnouncementStatusLastDay.Date == DateTime.UtcNow.Date)
                    .ToListAsync();

                var announcemenExpired = await repository.Filter<Notification>(n =>
                n.NotificationType == AnnouncementNotificationType.Available).Include(n => n.NotificationTranslate).FirstOrDefaultAsync();

                foreach (var a in announcements)
                {
                    var questionerLanguage = repository.FilterAsNoTracking<PersonOtherSetting>(x =>
                          x.UserId == a.UserId).Select(s => s.Language).FirstOrDefault();
                    using (var firebaseRepository = new FirebaseRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
                    {
                        await firebaseRepository.SendIndividualNotification(new IndividualNotificationModel
                        {
                            Description = $"{a.Title}",
                            GenericId = a.Id,
                            NotificationType = NotificationType.AnnouncementAvailable,
                            ReceiverId = a.UserId,
                            SenderId = null,
                            Title = questionerLanguage == Language.English ? announcemenExpired.Title
                     : announcemenExpired.NotificationTranslate.Select(n => n.Title).FirstOrDefault()
                        }, false);
                    }
                    repository.Create(new PersonNotification
                    {
                        AnnouncementId = a.Id,
                        UserId = a.UserId,
                        NotificationId = announcemenExpired.Id
                    });
                }
            }
        }

        public async Task DeleteStatistics()
        {
            using (var repository = new EntityRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
            {
                var statistics = await repository.Filter<Statistic>(x => x.ActivityDate.Date == DateTime.Now.AddDays(-7).Date)
                     .ToListAsync();
                if (statistics.Count == 0)
                    return;
                repository.HardDeleteRange<Statistic>(statistics);
                await repository.SaveChangesAsync();
            }
        }

        public async Task UnBlock()
        {
            using (var repository = new EntityRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
            {
                var users = await repository.Filter<User>(x => x.IsBlocked).ToListAsync();
                if (users == null)
                    return;
                foreach (var item in users)
                {
                    dynamic property = new ExpandoObject();
                    item.IsBlocked = false;
                    item.UserStatusType = UserStatusType.Active;
                    repository.Update(item);
                }
                await repository.SaveChangesAsync();
            }
        }

        public async Task SendPushNotification(int id)
        {
            using (var repository = new EntityRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
            {
                var pushNotification = await repository.Filter<PushNotification>(x => x.Id == id).FirstOrDefaultAsync();
                if (pushNotification == null)
                    return;
                var userIds = new List<int>();
                var guestIds = new List<int>();
                switch (pushNotification.PushNotificationUserType)
                {
                    case PushNotificationUserType.All:
                        userIds = await repository.GetAll<User>().Select(x => x.Id).ToListAsync();
                        guestIds = await repository.GetAll<Guest>().Select(x => x.Id).ToListAsync();
                        break;
                    case PushNotificationUserType.Guest:
                        guestIds = await repository.GetAll<Guest>().Select(x => x.Id).ToListAsync();
                        break;
                    case PushNotificationUserType.Registered:
                        userIds = await repository.GetAll<User>().Select(x => x.Id).ToListAsync();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                using (var fireBaseRepository = new FirebaseRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
                {
                    await fireBaseRepository.SendCampaignNotification(new FirebaseCampaignNotificationModel
                    {
                        Description = pushNotification.Description,
                        PushNotificationActionType = pushNotification.PushNotificationActionType,
                        Title = pushNotification.Title,
                        UserIds = userIds,
                        GuestIds = guestIds,
                        FromAdmin = true,
                        SenderId = null
                    });
                }
                pushNotification.PushNotificationStatusType = PushNotificationStatusType.Sent;
                repository.Update(pushNotification);
                await repository.SaveChangesAsync();
            }
        }

        public async Task CalculatePrices()
        {
            using (var repository = new EntityRepository(new BaitkmDbContext(new DbContextOptions<BaitkmDbContext>())))
            {
                IEnumerable<Rate> rates = repository.Filter<Rate>(r => !r.IsDeleted);
                IEnumerable<Currency> currencies = repository.Filter<Currency>(c => c.Id != 1 && !c.IsDeleted);
                foreach (Currency c in currencies)
                {
                    using (var client = new HttpClient())
                    {
                        try
                        {
                            var response = client.GetAsync($"https://itfllc.am/api/rate/exchange?from={1}&to={c.RequestId}&value={1}").Result;
                            var responseResult = JsonConvert.DeserializeObject<BaseExchangeResponseModel>(response.Content.ReadAsStringAsync().Result);

                            if (rates.Count() < 1)
                            {
                                repository.Create<Rate>(new Rate
                                {
                                    CurrencyId = c.Id,
                                    CurrentRate = Math.Round(responseResult.Data.Result, 3, MidpointRounding.ToEven)
                                });
                            }
                            else
                            {
                                Rate rate = await repository.Filter<Rate>(r => r.CurrencyId == c.Id).FirstOrDefaultAsync();
                                rate.CurrentRate = Math.Round(responseResult.Data.Result, 3, MidpointRounding.ToEven);
                                repository.Update(rate);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }
                    }
                }
                repository.SaveChanges();
            }
        }
    }
}
