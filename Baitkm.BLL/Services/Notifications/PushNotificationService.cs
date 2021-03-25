using Baitkm.BLL.Services.Scheduler.Jobs.PushNotificationRelated;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DAL.Repository.Firebase;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.PushNotifications;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.Subscriptions;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Notifications
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IEntityRepository _repository;
        private readonly IFirebaseRepository _firebaseRepository;
        public PushNotificationService(IEntityRepository repository,
            IFirebaseRepository firebaseRepository)
        {
            _repository = repository;
            _firebaseRepository = firebaseRepository;
        }

        public async Task<PagingResponseModel<PushNotificationListModel>> GetList(PagingRequestModel model)
        {
            var query = _repository.GetAll<PushNotification>();
            var count = await query.CountAsync();
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count)));
            var result = await query.OrderByDescending(x => x.CreatedDt).Skip((model.Page - 1) * model.Count)
                .Take(model.Count).Select(x => new PushNotificationListModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    Title = x.Title,
                    PushNotificationStatusType = x.PushNotificationStatusType,
                    SendingDate = x.SendingDate,
                    PushNotificationUserType = x.PushNotificationUserType,
                    PushNotificationActionType = x.PushNotificationActionType
                }).ToListAsync();
            return new PagingResponseModel<PushNotificationListModel>
            {
                ItemCount = count,
                PageCount = page,
                DateFrom = null,
                Data = result
            };
        }

        public async Task<bool> Create(CreatePushNotificationModel model)
        {
            if (model.AnnouncementId.HasValue && model.PushNotificationUserType != null)
                throw new Exception("You can set value or AnnouncementId or PushNotificationUserType!");

            var pushNotification = _repository.Create(new PushNotification
            {
                Description = model.Description,
                Title = model.Title,
                PushNotificationUserType = model.PushNotificationUserType,
                PushNotificationActionType = model.PushNotificationActionType,
                PushNotificationStatusType = model.SendingDate > DateTime.UtcNow
                    ? PushNotificationStatusType.Scheduled
                    : PushNotificationStatusType.Sent,
                SendingDate = model.SendingDate
            });
            _repository.Create(new PushNotificationTranslate
            {
                Language = Language.Arabian,
                PushNotificationId = pushNotification.Id,
                Title = model.ArabianTitle,
                Description = model.ArabianDescription
            });
            await _repository.SaveChangesAsync();
            if (pushNotification.PushNotificationStatusType == PushNotificationStatusType.Scheduled)
            {
                var date = new DateTime(pushNotification.SendingDate.Year, pushNotification.SendingDate.Month,
                    pushNotification.SendingDate.Day, pushNotification.SendingDate.Hour,
                    pushNotification.SendingDate.Minute, pushNotification.SendingDate.Second);
                var sendPushTriggerBuilder = TriggerBuilder.Create()
                    .WithIdentity($"{nameof(PushNotificationSendingJob)}Trigger#{PushNotificationSendingJob.Name}{pushNotification.Id}")
                    .StartAt(date);
                dynamic sendPushProp = new ExpandoObject();
                sendPushProp.promoId = pushNotification.Id;
                await SchedulerHelper.Schedule<PushNotificationSendingJob, IJobListener>(new QuartzScheduleModel
                {
                    Name = $"{PushNotificationSendingJob.Name}{pushNotification.Id}",
                    IsListenerRequested = false,
                    DataMap = sendPushProp,
                    TriggerBuilder = sendPushTriggerBuilder
                });
            }
            if (model.PushNotificationUserType.HasValue)
            {
                switch (pushNotification.PushNotificationUserType)
                {
                    case PushNotificationUserType.All:
                        await SendNotificationUsers(pushNotification, model.AnnouncementId);
                        await SendNotificationGuests(pushNotification, model.AnnouncementId);
                        break;
                    case PushNotificationUserType.Guest:
                        await SendNotificationGuests(pushNotification, model.AnnouncementId);
                        break;
                    case PushNotificationUserType.Registered:
                        await SendNotificationUsers(pushNotification, model.AnnouncementId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (model.AnnouncementId.HasValue)
                await SendNotificationToAnnoucmentCreator(model.AnnouncementId.Value, pushNotification);

            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var pushNotification = await _repository.Filter<PushNotification>(x =>
                    x.Id == id && x.PushNotificationStatusType == PushNotificationStatusType.Scheduled)
                .FirstOrDefaultAsync();
            if (pushNotification == null)
                throw new Exception("push notification not found");
            var pushNotificationTranslate = await _repository.Filter<PushNotificationTranslate>(p =>
                p.PushNotificationId == pushNotification.Id).FirstOrDefaultAsync();
            _repository.Remove<PushNotification>(pushNotification);
            _repository.Remove<PushNotificationTranslate>(pushNotificationTranslate);
            await SchedulerHelper.UnSchedule<PushNotificationSendingJob>(
                $"{PushNotificationSendingJob.Name}{pushNotification.Id}");
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<PushNotificationDetailsModel> Details(int id)
        {
            return await _repository.FilterAsNoTracking<PushNotification>(x => x.Id == id).Select(x =>
                new PushNotificationDetailsModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    Title = x.Title,
                    SendingDate = x.SendingDate,
                    PushNotificationStatusType = x.PushNotificationStatusType,
                    PushNotificationActionType = x.PushNotificationActionType,
                    PushNotificationUserType = x.PushNotificationUserType
                }).FirstOrDefaultAsync();
        }

        public async Task<bool> Update(UpdatePushNotificationModel model)
        {
            var pushNotification = await _repository.Filter<PushNotification>(x =>
                    x.Id == model.Id && x.PushNotificationStatusType == PushNotificationStatusType.Scheduled).FirstOrDefaultAsync();
            pushNotification.Description = model.Description;
            pushNotification.Title = model.Title;
            pushNotification.PushNotificationUserType = model.PushNotificationUserType;
            pushNotification.PushNotificationActionType = model.PushNotificationActionType;
            pushNotification.PushNotificationStatusType = model.SendingDate > DateTime.UtcNow
                ? PushNotificationStatusType.Scheduled
                : PushNotificationStatusType.Sent;
            pushNotification.SendingDate = model.SendingDate;
            _repository.Update(pushNotification);
            await _repository.SaveChangesAsync();
            if (pushNotification.PushNotificationStatusType == PushNotificationStatusType.Scheduled)
            {
                var date = new DateTime(pushNotification.SendingDate.Year, pushNotification.SendingDate.Month,
                    pushNotification.SendingDate.Day, pushNotification.SendingDate.Hour,
                    pushNotification.SendingDate.Minute, pushNotification.SendingDate.Second);
                var sendPushTriggerBuilder = TriggerBuilder.Create()
                    .WithIdentity($"{nameof(PushNotificationSendingJob)}Trigger#{PushNotificationSendingJob.Name}{pushNotification.Id}")
                    .StartAt(date);
                dynamic sendPushProp = new ExpandoObject();
                sendPushProp.pushNotificationId = pushNotification.Id;
                await SchedulerHelper.ReSchedule<PushNotificationSendingJob, IJobListener>(new QuartzScheduleModel
                {
                    Name = $"{PushNotificationSendingJob.Name}{pushNotification.Id}",
                    IsListenerRequested = false,
                    DataMap = sendPushProp,
                    TriggerBuilder = sendPushTriggerBuilder
                });
            }
            else
            {
                switch (pushNotification.PushNotificationUserType)
                {
                    case PushNotificationUserType.All:
                        await SendNotificationUsers(pushNotification, model.AnousmentId);
                        await SendNotificationGuests(pushNotification, model.AnousmentId);
                        break;
                    case PushNotificationUserType.Guest:
                        await SendNotificationGuests(pushNotification, model.AnousmentId);
                        break;
                    case PushNotificationUserType.Registered:
                        await SendNotificationUsers(pushNotification, model.AnousmentId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return true;
        }

        #region Send Notification
        private async Task<bool> SendNotificationUsers(PushNotification pushNotification, int? anousmentId)
        {
            List<User> allUsers = await _repository.GetAll<User>().Include(s => s.PersonSettings).ToListAsync();
            foreach (var user in allUsers)
            {
                var userLanguage = await _repository.FilterAsNoTracking<PersonOtherSetting>(x =>
                    x.UserId == user.Id).ToListAsync();
                if (user.PersonSettings.Select(x => x.SubscriptionsType).Contains(SubscriptionsType.NewMessagesNotifications))
                {
                    _repository.Create(new PersonNotification
                    {
                        UserId = user.Id,
                        PushNotificationId = pushNotification.Id,
                        Announcement = null,
                    });
                    var lang = userLanguage.Select(l => l.Language).FirstOrDefault();
                    await _firebaseRepository.SendIndividualNotification(new FirebaseIndividualNotificationModel
                    {
                        Description = lang == Language.English ? pushNotification.Description :
                             pushNotification.PushNotificationTranslates.Select(n => n.Description).FirstOrDefault(),
                        Title = lang == Language.English ? pushNotification.Title :
                             pushNotification.PushNotificationTranslates.Select(n => n.Title).FirstOrDefault(),
                        NotificationType = NotificationType.PushNotification,
                        PushNotificationActionType = pushNotification.PushNotificationActionType,
                        FromAdmin = true,
                        SenderId = null,
                        ReceiverId = user.Id,
                        GenericId = anousmentId.HasValue ? anousmentId.Value : 0
                    }, false);
                }
            }
            await _repository.SaveChangesAsync();
            return true;
        }

        private async Task<bool> SendNotificationToAnnoucmentCreator(int annoucmentId, PushNotification pushNotification)
        {
            Announcement announcement = await _repository.Filter<Announcement>(a => a.Id == annoucmentId)
                .Include(a => a.User).ThenInclude(u => u.PersonSettings).FirstOrDefaultAsync();
            User user = announcement.User;
            var userLanguage = await _repository.FilterAsNoTracking<PersonOtherSetting>(x =>
                x.UserId == user.Id).ToListAsync();
            if (user.PersonSettings.Select(x => x.SubscriptionsType).Contains(SubscriptionsType.NewMessagesNotifications))
            {
                _repository.Create(new PersonNotification
                {
                    UserId = user.Id,
                    PushNotificationId = pushNotification.Id,
                    Announcement = announcement,
                    AnnouncementId = annoucmentId,
                });
                var lang = userLanguage.Select(l => l.Language).FirstOrDefault();
                await _firebaseRepository.SendIndividualNotification(new FirebaseIndividualNotificationModel
                {
                    Description = lang == Language.English ? pushNotification.Description :
                         pushNotification.PushNotificationTranslates.Select(n => n.Description).FirstOrDefault(),
                    Title = lang == Language.English ? pushNotification.Title :
                         pushNotification.PushNotificationTranslates.Select(n => n.Title).FirstOrDefault(),
                    NotificationType = NotificationType.PushNotification,
                    PushNotificationActionType = pushNotification.PushNotificationActionType,
                    FromAdmin = true,
                    SenderId = null,
                    ReceiverId = user.Id,
                    GenericId = annoucmentId
                }, false);
            }

            await _repository.SaveChangesAsync();
            return true;
        }

        private async Task<bool> SendNotificationGuests(PushNotification pushNotification, int? anousmentId)
        {
            List<Guest> allGuests = await _repository.GetAll<Guest>().Include(s => s.PersonSettings).ToListAsync();
            foreach (var guest in allGuests)
            {
                var userLanguage = await _repository.FilterAsNoTracking<PersonOtherSetting>(x =>
                    x.GuestId == guest.Id).ToListAsync();
                if (guest.PersonSettings.Select(x => x.SubscriptionsType).Contains(SubscriptionsType.NewMessagesNotifications))
                {
                    _repository.Create(new PersonNotification
                    {
                        PushNotificationId = pushNotification.Id,
                        GuestId = guest.Id,
                        AnnouncementId = null
                    });
                    var lang = userLanguage.Select(l => l.Language).FirstOrDefault();
                    await _firebaseRepository.SendIndividualNotification(new FirebaseIndividualNotificationModel
                    {
                        Description = lang == Language.English ? pushNotification.Description :
                            pushNotification.PushNotificationTranslates.Select(n => n.Description).FirstOrDefault(),
                        Title = lang == Language.English ? pushNotification.Title :
                            pushNotification.PushNotificationTranslates.Select(n => n.Title).FirstOrDefault(),
                        NotificationType = NotificationType.PushNotification,
                        PushNotificationActionType = pushNotification.PushNotificationActionType,
                        FromAdmin = true,
                        SenderId = null,
                        ReceiverId = guest.Id,
                        GenericId = anousmentId.HasValue ? anousmentId.Value : 0
                    }, true);
                }
            }
            await _repository.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}