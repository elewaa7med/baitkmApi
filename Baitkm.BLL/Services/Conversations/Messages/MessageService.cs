using Baitkm.BLL.Helpers.Socket.Chat;
using Baitkm.BLL.Helpers.Socket.Chat.Base;
using Baitkm.BLL.Services.Activities;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DAL.Repository.Firebase;
using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Conversations.Messages;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Notifications;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Conversations;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.Subscriptions;
using Baitkm.Enums.UserRelated;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Conversations.Messages
{
    public class MessageService : IMessageService
    {
        private readonly IEntityRepository _repository;
        private readonly MediaAccessor _accessor;
        private readonly IFirebaseRepository _firebaseRepository;
        private readonly IOptionsBinder _optionsBinder;
        private readonly IActivityService _activityService;

        public MessageService(IEntityRepository repository,
            IFirebaseRepository firebaseRepository,
            IOptionsBinder optionsBinder,
            IActivityService activityService)
        {
            _repository = repository;
            _accessor = new MediaAccessor();
            _firebaseRepository = firebaseRepository;
            _optionsBinder = optionsBinder;
            _activityService = activityService;
        }

        public async Task<PagingResponseMessageModel> GetList(MessagePagingRequestModel model, int userId)
        {
            AnnouncementViewModel announcement = null;
            var caller = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (caller == null)
                throw new Exception("caller not found");
            var conversation = await _repository.Filter<Conversation>(x => x.Id == model.ConversationId)
            .Where(x => (x.AnnouncementCreatorId == caller.Id) || x.QuestionerId == caller.Id).FirstOrDefaultAsync();
            int announcementCount = 0;
            if (conversation.QuestionerId == caller.Id)
                announcementCount = await _repository.Filter<Announcement>(x => !x.IsDraft && x.UserId == conversation.AnnouncementCreatorId).CountAsync();
            else
                announcementCount = await _repository.Filter<Announcement>(x => !x.IsDraft && x.UserId == conversation.QuestionerId).CountAsync();

            Currency c = _repository.Filter<Currency>(c => c.Id == caller.CurrencyId).FirstOrDefault();

            var deletedMessages = _repository.Filter<Message>(s => s.ConversationId == conversation.Id)
                .Where(x => x.SenderId == caller.Id && x.SenderMessageIsDeleted
                    || x.ReciverId != caller.Id && x.ReciverMessageIsDeleted).Select(s => s.Id);
            var query = _repository.Filter<Message>(x => x.ConversationId == conversation.Id)
                .Where(s => !deletedMessages.Contains(s.Id));
            if (model.Page == 1)
            {
                announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == conversation.AnnouncementId)
                    .Skip((model.Page - 1) * model.Count).Take(model.Count).Select(
                    x => new AnnouncementViewModel
                    {
                        Id = x.Id,
                        AnnouncementType = x.AnnouncementType,
                        AnnouncementRentType = x.AnnouncementRentType,
                        AnnouncementResidentialType = x.AnnouncementResidentialType,
                        AnnouncementEstateType = x.AnnouncementEstateType,
                        AnnouncementStatus = x.AnnouncementStatus,
                        Area = Convert.ToInt64(x.Area),
                        BathroomCount = x.BathroomCount,
                        BedroomCount = x.BedroomCount,
                        Address = x.AddressEn.Trim(),
                        Price = Convert.ToInt64(x.Price),
                        Title = x.Title,
                        UserAnnouncementCount = announcementCount,
                        UserId = x.UserId,
                        UserName = x.User.FullName,
                        CurrencyId = x.CurrencyId,

                        Description = x.Description,
                        TitleArabian = x.TitleArabian,
                        CountryId = x.CountryId,
                        CityId = x.CityId,
                        SittingCount = x.SittingCount,
                        ConstructionStatus = x.ConstructionStatus,
                        SaleType = x.SaleType,
                        FurnishingStatus = x.FurnishingStatus,
                        OwnerShip = x.OwnerShip,
                        BuildingAge = x.BuildingAge,
                        CommercialType = x.CommercialType,
                        LandType = x.LandType,
                        FacadeType = x.FacadeType,
                        DisctrictName = x.DisctrictName,
                        ShareUrl = $"https://baitkm.com/products/details/{x.Id}",
                        Lat = x.Lat,
                        Lng = x.Lng,
                        CreateDate = x.CreatedDt,
                        BalconyArea = x.BalconyArea,
                        KitchenArea = x.KitchenArea,
                        MeterPrice = x.MeterPrice,
                        LaundryArea = x.LaundryArea,
                        LivingArea = x.LivingArea,
                        CurrencySymbol = c.Symbol,
                        LandNumber = x.LandNumber,
                        PlanNumber = x.PlanNumber,
                        CurrencyCode = c.Code,
                        StreetWidth = x.StreetWidth,
                        NumberOfAppartment = x.NumberOfAppartment,
                        NumberOfFloors = x.NumberOfFloors,
                        CreatedDt = x.CreatedDt,
                        NumberOfVilla = x.NumberOfVilla,
                        OfficeSpace = x.OfficeSpace,
                        LaborResidence = x.LaborResidence,
                        District = x.District,
                        NumberOfWareHouse = x.NumberOfWareHouse,
                        NumberOfShop = x.NumberOfShop,
                        NumberOfUnits = x.NumberOfUnits,
                        FireSystem = x.FireSystem,
                        LandCategory = x.LandCategory,
                        UserProfilePhoto = new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                              UploadType.ProfilePhoto, x.User.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                        },
                        Photo = new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, 100, 100, true, 0)
                        },
                        IsDeleted = x.IsDeleted,
                    }).FirstOrDefaultAsync();

                if (announcement != null)
                {
                    announcement.CurrencyCode = c.Code;
                    announcement.CurrencySymbol = c.Symbol;
                    if (c.Id != 1)
                    {
                        decimal currentRate = _repository.Filter<Rate>(r => r.CurrencyId == c.Id).FirstOrDefault().CurrentRate;
                        announcement.Price /= currentRate;
                    }

                    City city = _repository.Filter<City>(c => c.Id == announcement.CityId).Include(c => c.Country).FirstOrDefault();
                    announcement.City = city.Name;
                    announcement.Country = city.Country.Name;

                    if (announcement.Photo.Photo == null && announcement.AnnouncementResidentialType.HasValue)
                        announcement.Photo = new ImageOptimizer { Photo = DefaultCoverImgePath(announcement.AnnouncementResidentialType.Value) };
                }
            }

            var count = await query.CountAsync();
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count)));
            if (model.Page > 1)
                query = query.Where(x => x.CreatedDt < model.DateFrom);
            var result = await query.OrderByDescending(x => x.CreatedDt).Skip((model.Page - 1) * model.Count)
                .Take(model.Count).Select(x => new MessageListModel
                {
                    MessageBodyType = x.MessageBodyType,
                    Photo = x.User != null
                        ? new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, x.User.ProfilePhoto, 300, 300, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, x.User.ProfilePhoto, 100, 100, true, 0)
                        } : new ImageOptimizer(),
                    SenderId = x.SenderId,
                    FullName = x.User != null ? x.User.FullName : "User",
                    MessageId = x.Id,
                    MessageText = x.MessageBodyType == MessageBodyType.Image
                        ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.MessageFiles, x.MessageText, 1000, 1000, false, conversation.Id)
                        : x.MessageText,
                    CreatedDate = x.CreatedDt,
                    IsSentFromMe = x.SenderId == caller.Id,
                    FileUrl = x.MessageBodyType == MessageBodyType.File
                        ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.MessageFiles, x.MessageText, false, conversation.Id)
                        : null,
                    FileSize = x.FileLength,
                    ReplayMessage = x.ReplayMessageId != null ?
                    query.Where(s => s.Id == x.ReplayMessageId.GetValueOrDefault())
                        .Select(s => new MessageListModel
                        {
                            MessageBodyType = s.MessageBodyType,
                            Photo = s.User != null ? new ImageOptimizer
                            {
                                Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                             UploadType.ProfilePhoto, s.User.ProfilePhoto, 300, 300, false, 0),
                                PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                             UploadType.ProfilePhoto, s.User.ProfilePhoto, 100, 100, true, 0)
                            } : new ImageOptimizer(),
                            SenderId = s.SenderId,
                            FullName = s.User != null ? s.User.FullName : "User",
                            MessageId = s.Id,
                            MessageText = s.MessageBodyType == MessageBodyType.Image ?
                                Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.MessageFiles, s.MessageText, 1000, 1000, false, conversation.Id) : s.MessageText,
                            CreatedDate = s.CreatedDt,
                            IsSentFromMe = s.SenderId == caller.Id,
                            FileUrl = s.MessageBodyType == MessageBodyType.File ?
                                Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                                UploadType.MessageFiles, s.MessageText, false, conversation.Id) : null,
                            FileSize = s.FileLength
                        }).FirstOrDefault() : null
                }).ToListAsync();
            var unSeen = await _repository
                .Filter<Message>(x => !x.IsSeen && x.ConversationId == conversation.Id && x.SenderId != caller.Id)
                .ToListAsync();
            foreach (var message in unSeen)
            {
                message.IsSeen = true;
                _repository.Update(message);
            }
            await _repository.SaveChangesAsync();
            return new PagingResponseMessageModel
            {
                Announcement = announcement,
                ItemCount = count,
                PageCount = page,
                DateFrom = model.Page == 1 ? result.FirstOrDefault()?.CreatedDate : model.DateFrom,
                Data = result
            };
        }

        public async Task<MessageListModel> Send(SendMessageModel model, int userId, Language language, string deviceId)
        {
            string sentMessage;
            var caller = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (caller == null)
                throw new Exception("caller not found");
            var conversation = await _repository.Filter<Conversation>(x => x.Id == model.ConversationId)
                .FirstOrDefaultAsync();
            if (conversation.AnnouncementCreatorId != caller.Id && conversation.QuestionerId != caller.Id)
                throw new Exception(_optionsBinder.Error().NotParticipating);
            if (conversation.AnnouncementCreatorConversationIsDeleted || conversation.QuestionerConversationIsDeleted)
            {
                conversation.QuestionerConversationIsDeleted = false;
                conversation.AnnouncementCreatorConversationIsDeleted = false;
                _repository.Update(conversation);
            }

            var body = model.MessageText;
            var messageBodyType = MessageBodyType.Text;
            long length = 0;
            if (model.File != null)
            {
                messageBodyType = MessageBodyType.Image;
                body = await _accessor.Upload(model.File, UploadType.MessageFiles, conversation.Id);
                var extension = Path.GetExtension(body);
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    messageBodyType = MessageBodyType.File;
                length = await _accessor.GetLength(body, UploadType.MessageFiles, conversation.Id);
            }
            int? replayValue = null;
            var receiverId = conversation.AnnouncementCreatorId == caller.Id
              ? conversation.QuestionerId
              : conversation.AnnouncementCreatorId;
            var message = _repository.Create(new Message
            {
                ConversationId = conversation.Id,
                MessageBodyType = messageBodyType,
                MessageText = body,
                SenderId = caller.Id,
                FileLength = length,
                ReciverId = receiverId,
                ReplayMessageId = model.ReplayMessageId.HasValue ? model.ReplayMessageId.Value : replayValue
            });
            await _repository.SaveChangesAsync();
            int unreadConversationCount = _repository.Filter<Message>(m => m.ReciverId == receiverId && !m.IsSeen).GroupBy(m => m.ConversationId).Count();
            var success = _repository.FilterAsNoTracking<PersonSetting>(x => x.UserId == receiverId).Where(x =>
                 x.SubscriptionsType == SubscriptionsType.EnableMessageNotification).Count();
            var notify = (new MessageListModel
            {
                SenderId = caller.Id,
                FullName = message.User != null ? message.User.FullName : "User",
                Photo = message.User != null ? new ImageOptimizer
                {
                    Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.ProfilePhoto, message.User.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                    PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.ProfilePhoto, message.User.ProfilePhoto, 100, 100, true, 0)
                } : new ImageOptimizer(),
                CreatedDate = new DateTime(message.CreatedDt.Ticks),
                IsSentFromMe = false,
                MessageId = message.Id,
                MessageBodyType = messageBodyType,
                MessageText = message.MessageBodyType == MessageBodyType.Image
                    ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.MessageFiles, message.MessageText, 200, 200, false, conversation.Id) : message.MessageText,
                FileSize = length,
                FileUrl = messageBodyType == MessageBodyType.File
                    ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                        UploadType.MessageFiles, message.MessageText, false, conversation.Id) : null,
                ReplayMessage = message.ReplayMessageId != null ? await _repository.Filter<Message>(s => s.Id == message.ReplayMessageId)
                    .Select(s => new MessageListModel
                    {
                        MessageBodyType = s.MessageBodyType,
                        Photo = s.User != null ? new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                           UploadType.ProfilePhoto, s.User.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                          UploadType.ProfilePhoto, s.User.ProfilePhoto, 100, 100, true, 0)
                        } : new ImageOptimizer(),
                        SenderId = s.SenderId,
                        FullName = s.User != null ? s.User.FullName : "User",
                        MessageId = s.Id,
                        MessageText = s.MessageBodyType == MessageBodyType.Image ?
                                    Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                    UploadType.MessageFiles, s.MessageText, 1000, 1000, false, conversation.Id) : s.MessageText,
                        CreatedDate = s.CreatedDt,
                        IsSentFromMe = s.SenderId == caller.Id,
                        FileUrl = s.MessageBodyType == MessageBodyType.File ?
                                    Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                                    UploadType.MessageFiles, s.MessageText, false, conversation.Id) : null,
                        FileSize = s.FileLength
                    }).FirstOrDefaultAsync() : null
            });

            var ann = _repository.Filter<Announcement>(a => a.Id == model.AnnouncementId).FirstOrDefault();
            if (ann != null)
            {
                await _activityService.AddOrUpdate(ann, caller.Id, false, ActivityType.Contacted);
            }
            var notifyToString = Utilities.SerializeObject(notify);
            var isSent = await ChatMessageHandler.SendMessageAsync(conversation.Id, caller.Id, notifyToString, deviceId);
            if (isSent)
            {
                message.IsSeen = true;
                await _repository.SaveChangesAsync();
            }
            if (!isSent)
            {
                var receiverLanguage = await _repository.Filter<PersonOtherSetting>(x => x.UserId == receiverId)
                    .Select(x => x.Language).FirstOrDefaultAsync();
                if (receiverLanguage == Language.English)
                    sentMessage = "sent you a message";
                else
                    sentMessage = "ارسلت لك رساله";
                await BaseMessageHandler.SendMessageAsync(conversation.Id, receiverId);
                if (success > 0)
                    await _firebaseRepository.SendIndividualNotification(new NewMessageNotificationModel
                    {
                        SenderId = caller.Id,
                        Description = messageBodyType == MessageBodyType.Image ? "Image" :
                            messageBodyType == MessageBodyType.File ? "Photo" : model.MessageText,
                        GenericId = conversation.Id,
                        NotificationType = NotificationType.NewMessage,
                        ReceiverId = receiverId,
                        Title = $"{caller.FullName} {sentMessage}",
                        UnreadConversationCount = unreadConversationCount
                    }, false);
                await _firebaseRepository.SendIndividualNotification(new NewMessageNotificationModel
                {
                    SenderId = caller.Id,
                    Description = messageBodyType == MessageBodyType.Image ? "Image" :
                        messageBodyType == MessageBodyType.File ? "Photo" : model.MessageText,
                    GenericId = conversation.Id,
                    NotificationType = NotificationType.NewMessageGeneralInformation,
                    ReceiverId = receiverId,
                    Title = $"{caller.FullName} {sentMessage}",
                    UnreadConversationCount = unreadConversationCount
                }, false);
            }
            return notify;
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
    }
}