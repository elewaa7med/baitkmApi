using Baitkm.BLL.Helpers.Socket.SupportChat;
using Baitkm.BLL.Helpers.Socket.SupportChat.SupportBase;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DAL.Repository.Firebase;
using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Conversations.Messages;
using Baitkm.DTO.ViewModels.Conversations.SupportConversations.SupportMessages;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.Notifications;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Conversations;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.Subscriptions;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Conversations.SupportConversations.SupportMessages
{
    public class SupportMessageService : ISupportMessageService
    {
        private readonly IEntityRepository _repository;
        private readonly MediaAccessor _accessor;
        private readonly IFirebaseRepository _firebaseRepository;
        private readonly IOptionsBinder _optionsBinder;
        public SupportMessageService(IEntityRepository repository,
            IFirebaseRepository firebaseRepository,
            IOptionsBinder optionsBinder)
        {
            _repository = repository;
            _accessor = new MediaAccessor();
            _firebaseRepository = firebaseRepository;
            _optionsBinder = optionsBinder;
        }

        public async Task<SupportMessageListModel> Send(SendSupportMessageModel model, int userId, Language language, string deviceId)
        {
            //User caller = null;
            Guest guest = null;
            var caller = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (caller == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
            }

            var conversation = await _repository.Filter<SupportConversation>(x => x.Id == model.ConversationId)
                .FirstOrDefaultAsync();
            if (conversation == null)
                throw new Exception("connversation not found");
            var body = model.MessageText;
            var messageBodyType = model.SupportMessageBodyType;
            long length = 0;
            if (model.File != null)
            {
                messageBodyType = SupportMessageBodyType.Image;
                body = await _accessor.Upload(model.File, UploadType.SupportConversationFiles, conversation.Id);
                var extension = Path.GetExtension(body);
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    messageBodyType = SupportMessageBodyType.Photo;
                length = await _accessor.GetLength(body, UploadType.SupportConversationFiles, conversation.Id);
            }
            int? replayValue = null;
            var supportMessage = new SupportMessage
            {
                SupportConversationId = conversation.Id,
                SupportMessageBodyType = messageBodyType,
                MessageText = body,
                FileLength = length,
                ReplayMessageId = model.ReplayMessageId.HasValue ? model.ReplayMessageId.Value : replayValue
            };
            if (caller != null)
            {
                supportMessage.UserSenderId = caller.Id;
                supportMessage.GuestSenderId = null;
            }
            else
            {
                supportMessage.GuestSenderId = guest.Id;
                supportMessage.UserSenderId = null;
            }
            _repository.Create(supportMessage);
            await _repository.SaveChangesAsync();
            int receiverId;
            if (caller != null)
            {
                receiverId = conversation.AdminId == caller.Id
                    ? conversation.GuestId ?? conversation.UserId ?? 0
                    : conversation.AdminId;
            }
            else
                receiverId = conversation.AdminId;
            var isGuest = conversation.GuestId != null;
            var callerId = caller?.Id ?? guest.Id;
            AnnouncementListViewModel announcement = null;
            if (messageBodyType == SupportMessageBodyType.Announcement)
            {
                int.TryParse(model.MessageText, out var id);
                announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == id).Select(x =>
                    new AnnouncementListViewModel
                    {
                        Id = x.Id,
                        BathroomCount = x.BathroomCount,
                        AnnouncementResidentialType = x.AnnouncementResidentialType,
                        AnnouncementRentType = x.AnnouncementRentType,
                        IsFavourite = false,
                        Address = x.AddressEn.Trim(),
                        AnnouncementEstateType = x.AnnouncementEstateType,
                        AnnouncementType = x.AnnouncementType,
                        Area = Convert.ToInt64(x.Area),
                        BedroomCount = x.BedroomCount,
                        Price = Convert.ToInt64(x.Price),
                        Title = x.Title,
                        CreateDate = x.CreatedDt,
                        Photo = new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, 300, 300, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, 100, 100, true, 0)
                        }
                    }).FirstOrDefaultAsync();
            }
            var notify = (new SupportMessageListModel
            {
                ConversationId = conversation.Id,
                MessageId = supportMessage.Id,
                MessageText = supportMessage.SupportMessageBodyType == SupportMessageBodyType.Image ?
                    Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.SupportConversationFiles, supportMessage.MessageText, 1000, 1000, false, conversation.Id) : supportMessage.MessageText,
                MessageBodyType = messageBodyType,
                SenderId = caller?.Id ?? guest.Id,
                FullName = caller != null ? supportMessage.UserSender.FullName : guest.Id.ToString(),
                Photo = caller != null ? new ImageOptimizer
                {
                    Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.ProfilePhoto, supportMessage.UserSender.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                    PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.ProfilePhoto, supportMessage.UserSender.ProfilePhoto, 100, 100, true, 0)
                } : new ImageOptimizer(),
                CreatedDate = supportMessage.CreatedDt,
                IsSentFromMe = false,
                Announcement = announcement,
                FileSize = length,
                FileUrl = messageBodyType == SupportMessageBodyType.Photo
                    ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                        UploadType.SupportConversationFiles, supportMessage.MessageText, false, conversation.Id) : null,
                ReplayMessage = supportMessage.ReplayMessageId != null ? await _repository.Filter<SupportMessage>(s => s.Id == supportMessage.ReplayMessageId)
                    .Select(s => new SupportMessageListModel
                    {
                        MessageBodyType = s.SupportMessageBodyType,
                        MessageId = s.Id,
                        MessageText = s.SupportMessageBodyType == SupportMessageBodyType.Image ?
                                    Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                    UploadType.SupportConversationFiles, s.MessageText, 1000, 1000, false, conversation.Id) : s.MessageText,
                        CreatedDate = s.CreatedDt,
                        IsSentFromMe = (s.UserSenderId ?? s.GuestSenderId ?? 0) == callerId,
                        Photo = s.UserSender != null ? new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                           UploadType.ProfilePhoto, s.UserSender.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                          UploadType.ProfilePhoto, s.UserSender.ProfilePhoto, 100, 100, true, 0)
                        } : new ImageOptimizer(),
                        SenderId = s.UserSenderId ?? s.GuestSenderId.GetValueOrDefault(),
                        FullName = s.UserSenderId != null ? s.UserSender.FullName : "User",
                        FileUrl = s.SupportMessageBodyType == SupportMessageBodyType.Photo ?
                                    Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                                    UploadType.SupportConversationFiles, s.MessageText, false, conversation.Id) : null,
                        FileSize = s.FileLength
                    }).FirstOrDefaultAsync() : null
            });
            if (notify.ReplayMessage != null && notify.ReplayMessage.MessageBodyType == SupportMessageBodyType.Announcement)
            {
                int.TryParse(notify.ReplayMessage.MessageText, out var replayMessage);
                notify.ReplayMessage.Announcement = _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == replayMessage)
                    .Select(x => new AnnouncementListViewModel
                    {
                        Id = x.Id,
                        BathroomCount = x.BathroomCount,
                        AnnouncementResidentialType = x.AnnouncementResidentialType,
                        AnnouncementRentType = x.AnnouncementRentType,
                        IsFavourite = false,
                        Address = x.AddressEn.Trim(),
                        AnnouncementEstateType = x.AnnouncementEstateType,
                        AnnouncementType = x.AnnouncementType,
                        Area = Convert.ToInt64(x.Area),
                        BedroomCount = x.BedroomCount,
                        Price = Convert.ToInt64(x.Price),
                        Title = x.Title,
                        Photo = new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, 300, 300, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, 100, 100, true, 0)
                        }
                    }).FirstOrDefault();
            }
            var notifyToString = Utilities.SerializeObject(notify);
            var isSent = await SupportChatMessageHandler.SendMessageAsync(conversation.Id, receiverId,
                notifyToString, receiverId == conversation.GuestId);
            bool enableNotification = true;
            notify.IsSentFromMe = true;
            if (receiverId == conversation.AdminId && !isSent)
            {
                await SupportBaseMessageHandler.SendMessageAsync(conversation.Id, conversation.AdminId);
                return notify;
            }
            if (!isSent && receiverId != conversation.AdminId)
            {
                if (isGuest && receiverId != conversation.AdminId)
                    enableNotification = _repository.FilterAsNoTracking<PersonSetting>(s => s.GuestId == receiverId)
                        .Select(s => s.SubscriptionsType).Contains(SubscriptionsType.EnableMessageNotification);
                else if (!isGuest && caller != null && receiverId != conversation.AdminId)
                    enableNotification = _repository.FilterAsNoTracking<PersonSetting>(s => s.UserId == receiverId)
                        .Select(s => s.SubscriptionsType).Contains(SubscriptionsType.EnableMessageNotification);

                if (enableNotification)
                {
                    await _firebaseRepository.SendIndividualNotification(new NewMessageNotificationModel
                    {
                        SenderId = caller?.Id ?? guest.Id,
                        Description = messageBodyType == SupportMessageBodyType.Image ? "Image" :
                            messageBodyType == SupportMessageBodyType.Photo ? "Photo" :
                            messageBodyType == SupportMessageBodyType.Announcement ? "Announcement" : model.MessageText,
                        GenericId = conversation.Id,
                        NotificationType = NotificationType.SupportMessage,
                        ReceiverId = receiverId,
                        Title = caller != null
                            ? $"{caller.FullName} sent you a message"
                            : $"Guest N {guest.Id} sent you a message"
                    }, isGuest);
                }
            }
            return notify;
        }

        public async Task<PagingResponseSupportMessageModel> GetAdminList(MessagePagingRequestModel model, int userId)
        {
            var caller = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (caller == null)
                throw new Exception("caller not found");
            var conversation = await _repository.Filter<SupportConversation>(x => x.Id == model.ConversationId)
                .FirstOrDefaultAsync();
            if (conversation == null)
                throw new Exception("conversation not found");
            var query = _repository.Filter<SupportMessage>(x => x.SupportConversationId == conversation.Id);
            var result = new PagingResponseSupportMessageModel();
            if (model.Page == 1)
            {
                if (conversation.GuestId != null)
                    result.FullName = $"Guest N {conversation.GuestId}";
                else
                {
                    var user = await _repository.Filter<User>(x => x.Id == conversation.UserId).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        var announcementCount =
                            await _repository.Filter<Announcement>(x => !x.IsDraft && x.UserId == user.Id).CountAsync();
                        result.UserId = user.Id;
                        result.FullName = user.FullName;
                        result.Photo = new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, user.ProfilePhoto, 500, 500),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, user.ProfilePhoto, 100, 100, true)
                        };
                        result.AnnouncementCount = announcementCount;
                    }
                }
            }
            var count = await query.CountAsync();
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count)));
            if (model.Page > 1)
                query = query.Where(x => x.CreatedDt < model.DateFrom);
            var data = await query.OrderByDescending(x => x.CreatedDt)
                .Skip((model.Page - 1) * model.Count).Take(model.Count)
                .Select(x => new SupportMessageListModel
                {
                    MessageId = x.Id,
                    MessageText = x.SupportMessageBodyType == SupportMessageBodyType.Image
                        ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.SupportConversationFiles, x.MessageText, 400, 400, false, conversation.Id)
                        : x.MessageText,
                    ConversationId = x.SupportConversationId,
                    IsSentFromMe = (x.UserSenderId ?? x.GuestSenderId ?? 0) == caller.Id,
                    MessageBodyType = x.SupportMessageBodyType,
                    CreatedDate = x.CreatedDt,
                    SenderId = x.UserSenderId ?? x.GuestSenderId ?? 0,
                    FullName = x.UserSender.FullName ?? x.GuestSenderId.ToString() ?? null,
                    Photo = x.UserSender != null ? new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, x.UserSender.ProfilePhoto, 500, 500, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, x.UserSender.ProfilePhoto, 100, 100, true, 0)
                    } : new ImageOptimizer(),
                    FileUrl = x.SupportMessageBodyType == SupportMessageBodyType.Photo
                        ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                            UploadType.SupportConversationFiles, x.MessageText, false, conversation.Id) : null,
                    FileSize = x.FileLength,
                    ReplayMessage = x.ReplayMessageId != null ?
                        query.Where(w => w.Id == x.ReplayMessageId)
                            .Select(s => new SupportMessageListModel
                            {
                                MessageId = s.Id,
                                MessageText = s.SupportMessageBodyType == SupportMessageBodyType.Image
                                     ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                     UploadType.SupportConversationFiles, s.MessageText, 400, 400, false, conversation.Id) : s.MessageText,
                                ConversationId = s.SupportConversationId,
                                IsSentFromMe = (s.UserSenderId ?? s.GuestSenderId ?? 0) == caller.Id,
                                MessageBodyType = s.SupportMessageBodyType,
                                CreatedDate = s.CreatedDt,
                                SenderId = s.UserSenderId ?? s.GuestSenderId ?? 0,
                                FullName = s.UserSender.FullName ?? s.GuestSenderId.ToString() ?? null,
                                Photo = s.UserSender != null ? new ImageOptimizer
                                {
                                    Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                        UploadType.ProfilePhoto, s.UserSender.ProfilePhoto, 500, 500, false, 0),
                                    PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                        UploadType.ProfilePhoto, s.UserSender.ProfilePhoto, 100, 100, true, 0)
                                } : new ImageOptimizer(),
                                FileUrl = x.SupportMessageBodyType == SupportMessageBodyType.Photo ?
                                    Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                                        UploadType.SupportConversationFiles, x.MessageText, false, conversation.Id) : null,
                                FileSize = x.FileLength
                            }).FirstOrDefault() : null
                }).ToListAsync();
            foreach (var variable in data)
            {
                if (variable.MessageBodyType != SupportMessageBodyType.Announcement)
                {
                    if (variable.ReplayMessage != null)
                    {
                        if (variable.ReplayMessage.MessageBodyType == SupportMessageBodyType.Announcement)
                        {
                            int.TryParse(variable.ReplayMessage.MessageText, out var announcementId);
                            var replayAnnouncement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == announcementId)
                                .Select(x => new AnnouncementListViewModel
                                {
                                    Id = x.Id,
                                    BathroomCount = x.BathroomCount,
                                    AnnouncementResidentialType = x.AnnouncementResidentialType,
                                    AnnouncementRentType = x.AnnouncementRentType,
                                    IsFavourite = false,
                                    Address = x.AddressEn.Trim(),
                                    AnnouncementEstateType = x.AnnouncementEstateType,
                                    AnnouncementType = x.AnnouncementType,
                                    Area = Convert.ToInt64(x.Area),
                                    BedroomCount = x.BedroomCount,
                                    CurrencyId = x.CurrencyId,
                                    Price = Convert.ToInt64(x.Price),
                                    Title = x.Title,
                                    CreateDate = x.CreatedDt,
                                    Photo = new ImageOptimizer
                                    {
                                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                        UploadType.AnnouncementBasePhoto, x.BasePhoto, 500, 500, false, 0),
                                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                        UploadType.AnnouncementBasePhoto, x.BasePhoto, 100, 100, true, 0)
                                    }
                                }).FirstOrDefaultAsync();
                            variable.ReplayMessage.Announcement = replayAnnouncement ?? null;
                            if (variable.ReplayMessage.Announcement != null)
                            {
                                Currency currency = _repository.Filter<Currency>(c => c.Id == replayAnnouncement.CurrencyId).FirstOrDefault();
                                replayAnnouncement.CurrencyCode = currency.Code;
                                replayAnnouncement.CurrencySymbol = currency.Symbol;
                                if (replayAnnouncement.CurrencyId != 1)
                                {
                                    decimal currentRate = _repository.Filter<Rate>(r => r.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                                    replayAnnouncement.Price /= currentRate;
                                }
                            }
                        }
                    }
                    continue;
                }
                int.TryParse(variable.MessageText, out var id);
                var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == id).Select(x =>
                    new AnnouncementListViewModel
                    {
                        Id = x.Id,
                        BathroomCount = x.BathroomCount,
                        AnnouncementResidentialType = x.AnnouncementResidentialType,
                        AnnouncementRentType = x.AnnouncementRentType,
                        IsFavourite = false,
                        Address = x.AddressEn.Trim(),
                        AnnouncementEstateType = x.AnnouncementEstateType,
                        AnnouncementType = x.AnnouncementType,
                        Area = Convert.ToInt64(x.Area),
                        BedroomCount = x.BedroomCount,
                        Price = Convert.ToInt64(x.Price),
                        CurrencyId = x.CurrencyId,
                        Title = x.Title,
                        CommercialType = x.CommercialType,
                        CreateDate = x.CreatedDt,
                        Photo = new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, 500, 500, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, 100, 100, true, 0)
                        }
                    }).FirstOrDefaultAsync();
                variable.Announcement = announcement;
                if (variable.Announcement != null)
                {
                    Currency currency = _repository.Filter<Currency>(c => c.Id == announcement.CurrencyId).FirstOrDefault();
                    announcement.CurrencyCode = currency.Code;
                    announcement.CurrencySymbol = currency.Symbol;
                    if (announcement.CurrencyId != 1)
                    {
                        decimal currentRate = _repository.Filter<Rate>(r => r.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                        announcement.Price /= currentRate;
                    }
                    //if (announcement.Photo.Photo == null && announcement.AnnouncementResidentialType.HasValue)
                    //    announcement.Photo = new ImageOptimizer { Photo = DefaultCoverImgePath(announcement.AnnouncementResidentialType.Value) };
                    //if (announcement.Photo.Photo == null && announcement.CommercialType.HasValue)
                    //    announcement.Photo = new ImageOptimizer { Photo = DefaultCommercialIMagePath(announcement.CommercialType.Value) };
                    //if (announcement.Photo.Photo == null && announcement.AnnouncementEstateType == AnnouncementEstateType.Land)
                    //    announcement.Photo = new ImageOptimizer { Photo = DefaultLandMagePath() };
                }
            }
            var unSeen = await _repository.Filter<SupportMessage>(x =>
                !x.IsSeen && x.SupportConversationId == conversation.Id && x.UserSenderId != caller.Id).ToListAsync();
            foreach (var message in unSeen)
            {
                message.IsSeen = true;
                _repository.Update(message);
            }
            await _repository.SaveChangesAsync();
            result.Data = data;
            result.ItemCount = count;
            result.PageCount = page;
            result.DateFrom = model.Page == 1 ? data.FirstOrDefault()?.CreatedDate : model.DateFrom;
            return result;
        }

        public async Task<PagingResponseModel<SupportMessageListModel>> GetMobileList(MessagePagingRequestModel model,
            int userId, Language language, string deviceId)
        {
            Currency currency;
            Guest guest = null;
            var caller = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (caller == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
                currency = await _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefaultAsync();
            }
            else
                currency = await _repository.Filter<Currency>(c => c.Id == caller.CurrencyId).FirstOrDefaultAsync();

            var conversation = await _repository.Filter<SupportConversation>(x => x.Id == model.ConversationId)
                .FirstOrDefaultAsync();
            if (conversation == null)
                throw new Exception("conversation not found");
            var callerId = caller?.Id ?? guest.Id;
            var query = _repository.Filter<SupportMessage>(x => x.SupportConversationId == conversation.Id);
            var count = await query.CountAsync();
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count)));
            if (model.Page > 1)
                query = query.Where(x => x.CreatedDt < model.DateFrom);
            var data = await query.OrderByDescending(x => x.CreatedDt)
                .Skip((model.Page - 1) * model.Count).Take(model.Count)
                .Select(x => new SupportMessageListModel
                {
                    MessageId = x.Id,
                    MessageText = x.SupportMessageBodyType == SupportMessageBodyType.Image
                                    ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                        UploadType.SupportConversationFiles, x.MessageText, 400, 400, false, conversation.Id) : x.MessageText,
                    ConversationId = x.SupportConversationId,
                    IsSentFromMe = (x.UserSenderId ?? x.GuestSenderId ?? 0) == callerId,
                    MessageBodyType = x.SupportMessageBodyType,
                    CreatedDate = x.CreatedDt,
                    SenderId = x.UserSenderId ?? x.GuestSenderId ?? 0,
                    FullName = x.UserSender.FullName ?? x.GuestSenderId.ToString() ?? null,
                    Photo = x.UserSender != null
                                    ? new ImageOptimizer
                                    {
                                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                            UploadType.ProfilePhoto, x.UserSender.ProfilePhoto, 500, 500, false, 0),
                                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                            UploadType.ProfilePhoto, x.UserSender.ProfilePhoto, 100, 100, true, 0)
                                    }
                                    : new ImageOptimizer(),
                    FileUrl = x.SupportMessageBodyType == SupportMessageBodyType.Photo
                                    ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                                        UploadType.SupportConversationFiles, x.MessageText, false, conversation.Id)
                                    : null,
                    FileSize = x.FileLength,
                    ReplayMessage = x.ReplayMessageId != null ?
                        query.Where(w => w.Id == x.ReplayMessageId)
                            .Select(s => new SupportMessageListModel
                            {
                                MessageId = s.Id,
                                MessageText = s.SupportMessageBodyType == SupportMessageBodyType.Image
                                                 ? Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                                 UploadType.SupportConversationFiles, s.MessageText, 400, 400, false, conversation.Id) : s.MessageText,
                                ConversationId = s.SupportConversationId,
                                IsSentFromMe = (s.UserSenderId ?? s.GuestSenderId ?? 0) == callerId,
                                MessageBodyType = s.SupportMessageBodyType,
                                CreatedDate = s.CreatedDt,
                                SenderId = s.UserSenderId ?? s.GuestSenderId ?? 0,
                                FullName = s.UserSender.FullName ?? s.GuestSenderId.ToString() ?? null,
                                Photo = s.UserSender != null ? new ImageOptimizer
                                {
                                    Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                                    UploadType.ProfilePhoto, s.UserSender.ProfilePhoto, 500, 500, false, 0),
                                    PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                                    UploadType.ProfilePhoto, s.UserSender.ProfilePhoto, 100, 100, true, 0)
                                } : new ImageOptimizer(),


                                FileUrl = x.SupportMessageBodyType == SupportMessageBodyType.Photo ?
                                                Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaDownload,
                                                    UploadType.SupportConversationFiles, x.MessageText, false, conversation.Id) : null,
                                FileSize = x.FileLength,
                            }).FirstOrDefault() : null
                }).ToListAsync();
            foreach (var variable in data)
            {
                if (variable.MessageBodyType != SupportMessageBodyType.Announcement)
                {
                    if (variable.ReplayMessage != null)
                    {
                        if (variable.ReplayMessage.MessageBodyType == SupportMessageBodyType.Announcement)
                        {
                            int.TryParse(variable.ReplayMessage.MessageText, out var announcementId);
                            var replayAnnouncement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == announcementId)
                                .Select(x => new AnnouncementListViewModel
                                {
                                    Id = x.Id,
                                    BathroomCount = x.BathroomCount,
                                    AnnouncementResidentialType = x.AnnouncementResidentialType,
                                    AnnouncementRentType = x.AnnouncementRentType,
                                    IsFavourite = false,
                                    Address = x.AddressEn.Trim(),
                                    AnnouncementEstateType = x.AnnouncementEstateType,
                                    AnnouncementType = x.AnnouncementType,
                                    Area = Convert.ToInt64(x.Area),
                                    BedroomCount = x.BedroomCount,
                                    Price = Convert.ToInt64(x.Price),
                                    CurrencyId = x.CurrencyId,
                                    Title = x.Title,
                                    CreateDate = x.CreatedDt,
                                    Photo = new ImageOptimizer
                                    {
                                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                        UploadType.AnnouncementBasePhoto, x.BasePhoto, 500, 500, false, 0),
                                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                        UploadType.AnnouncementBasePhoto, x.BasePhoto, 100, 100, true, 0)
                                    }
                                }).FirstOrDefaultAsync();
                            variable.ReplayMessage.Announcement = replayAnnouncement != null ? replayAnnouncement : null;
                            if (variable.ReplayMessage.Announcement != null)
                            {
                                replayAnnouncement.CurrencyCode = currency.Code;
                                if (currency.Id != 1)
                                {
                                    decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                                    replayAnnouncement.Price /= currentRate;
                                }
                            }
                        }
                    }
                    continue;
                }
                int.TryParse(variable.MessageText, out var id);
                var announcement = await _repository.Filter<Announcement>(x => !x.IsDraft && x.Id == id
                    && !x.IsDeleted).Select(x =>
                    new AnnouncementListViewModel
                    {
                        Id = x.Id,
                        BathroomCount = x.BathroomCount,
                        AnnouncementResidentialType = x.AnnouncementResidentialType,
                        AnnouncementRentType = x.AnnouncementRentType,
                        IsFavourite = false,
                        Address = x.AddressEn.Trim(),
                        AnnouncementEstateType = x.AnnouncementEstateType,
                        AnnouncementType = x.AnnouncementType,
                        Area = Convert.ToInt64(x.Area),
                        CurrencyId = x.CurrencyId,
                        BedroomCount = x.BedroomCount,
                        Price = Convert.ToInt64(x.Price),
                        Title = x.Title,
                        CreateDate = x.CreatedDt,
                        Photo = new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, 500, 500, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.AnnouncementBasePhoto, x.BasePhoto, 100, 100, true, 0)
                        }
                    }).FirstOrDefaultAsync();
                if (announcement != null)
                {
                    announcement.CurrencyCode = currency.Code;
                    if (currency.Id != 1)
                    {
                        decimal currentRate = _repository.Filter<Rate>(rate => rate.CurrencyId == currency.Id).FirstOrDefault().CurrentRate;
                        announcement.Price /= currentRate;
                    }
                    //if (announcement.Photo == null && announcement.AnnouncementResidentialType.HasValue)
                    //    announcement.Photo = new ImageOptimizer { Photo = DefaultCoverImgePath(announcement.AnnouncementResidentialType.Value) };
                    //if (announcement.Photo.Photo == null && announcement.CommercialType.HasValue)
                    //    announcement.Photo = new ImageOptimizer { Photo = DefaultCommercialIMagePath(announcement.CommercialType.Value) };
                    //if (announcement.Photo.Photo == null && announcement.AnnouncementEstateType == AnnouncementEstateType.Land)
                    //    announcement.Photo = new ImageOptimizer { Photo = DefaultLandMagePath() };
                }
                if (announcement == null && variable.MessageBodyType == SupportMessageBodyType.Announcement)
                    continue;
                else
                    variable.Announcement = announcement;
            }
            var unSeenQuery = _repository
                .Filter<SupportMessage>(x => !x.IsSeen && x.SupportConversationId == conversation.Id);
            unSeenQuery = caller != null
                ? unSeenQuery.Where(x => x.UserSenderId != caller.Id)
                : unSeenQuery.Where(x => x.GuestSenderId != guest.Id);
            var unSeen = await unSeenQuery.ToListAsync();
            foreach (var message in unSeen)
            {
                message.IsSeen = true;
                _repository.Update(message);
            }
            await _repository.SaveChangesAsync();
            return new PagingResponseModel<SupportMessageListModel>
            {
                DateFrom = model.Page == 1 ? data.FirstOrDefault()?.CreatedDate : model.DateFrom,
                ItemCount = count,
                Data = data,
                PageCount = page
            };
        }


        //private string DefaultCoverImgePath(AnnouncementResidentialType type)
        //{
        //    switch (type)
        //    {
        //        case AnnouncementResidentialType.Apartment:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/appartament.png/1500/1500/False/0";
        //        case AnnouncementResidentialType.Building:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/building.png/1500/1500/False/0";
        //        case AnnouncementResidentialType.Chalet:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/chalet.png/1500/1500/False/0";
        //        case AnnouncementResidentialType.Compound:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/compound.png/1500/1500/False/0";
        //        case AnnouncementResidentialType.Duplex:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/duplex.png/1500/1500/False/0";
        //        case AnnouncementResidentialType.FarmHouse:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/farmhouse.png/1500/1500/False/0";
        //        case AnnouncementResidentialType.Studio:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/studio.png/1500/1500/False/0";
        //        case AnnouncementResidentialType.Tower:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/tower.png/1500/1500/False/0";
        //        case AnnouncementResidentialType.Villa:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/villa.png/1500/1500/False/0";
        //        default:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/appartament.png/1500/1500/False/0";
        //    }
        //}

        //private string DefaultCommercialIMagePath(CommercialType type)
        //{
        //    switch (type)
        //    {
        //        case CommercialType.OfficeSpace:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/office.png/1500/1500/False/0";
        //        case CommercialType.Shop:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/shop.png/1500/1500/False/0";
        //        case CommercialType.Showroom:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/showroom.png/1500/1500/False/0";
        //        case CommercialType.WereHouse:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/werehouse.png/1500/1500/False/0";
        //        default:
        //            return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/office.png/1500/1500/False/0";
        //    }
        //}

        //private string DefaultLandMagePath()
        //{
        //    return "https://media.baitkm.com/api/Image/Resize/AnnouncementBasePhoto/land.png/1500/1500/False/0";
        //}
    }
}