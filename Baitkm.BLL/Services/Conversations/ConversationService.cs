using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Conversations;
using Baitkm.DTO.ViewModels.Conversations.Messages;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.Entities;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Conversations;
using Baitkm.Enums.Notifications;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Conversations
{
    public class ConversationService : IConversationService
    {
        private readonly IEntityRepository _repository;
        private readonly IOptionsBinder _optionsBinder;

        public ConversationService(IEntityRepository repository,
            IOptionsBinder optionsBinder)
        {
            _repository = repository;
            this._optionsBinder = optionsBinder;
        }

        public async Task<int> Add(int announcementId, int userId)
        {
            var caller = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (caller == null)
                throw new Exception("caller not found");
            var announcement = await _repository
                .Filter<Announcement>(x => !x.IsDraft && x.UserId != caller.Id && x.Id == announcementId).FirstOrDefaultAsync();
            if (announcement == null)
                throw new Exception(_optionsBinder.Error().AnnouncementNotFound);
            var conversation = await _repository.Filter<Conversation>(c => c.AnnouncementCreatorId == announcement.UserId
                && c.AnnouncementId == announcement.Id && c.QuestionerId == caller.Id).FirstOrDefaultAsync();
            if (conversation != null)
            {
                conversation.QuestionerConversationIsDeleted = false;
                conversation.AnnouncementCreatorConversationIsDeleted = false;
                _repository.Update(conversation);
            }
            else
            {
                conversation = _repository.Create(new Conversation
                {
                    AnnouncementCreatorId = announcement.UserId,
                    AnnouncementId = announcement.Id,
                    QuestionerId = caller.Id
                });
            }
            await _repository.SaveChangesAsync();
            return conversation.Id;
        }

        public async Task<PagingResponseModel<ConversationListModel>> GetList(ConversationPagingRequestModel model, int userId, OsType osType)
        {
            var caller = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (caller == null)
                throw new Exception("caller not found");
            var query = _repository.Filter<Conversation>(x =>
                    ((x.AnnouncementCreatorId == caller.Id && !x.AnnouncementCreatorConversationIsDeleted)
                         || (x.QuestionerId == caller.Id && !x.QuestionerConversationIsDeleted))
                             && x.Messages.Count(s => !s.IsDeleted) != 0);
            if (!string.IsNullOrEmpty(model.Search))
            {
                var search = model.Search.ToLower();
                var searchParams = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in searchParams)
                {
                    query = query.Where(x => x.AnnouncementCreator.FullName.ToLower().Contains(item) ||
                        x.Questioner.FullName.ToLower().Contains(item));
                }
            }
            var count = query.Count();
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count)));
            var result = await query.OrderByDescending(x =>
                 x.Messages.OrderByDescending(s => s.CreatedDt).Select(s => s.CreatedDt).FirstOrDefault())
                .Skip((model.Page - 1) * model.Count).Take(model.Count).Select(x => new ConversationListModel
                {
                    AnnouncementId = x.AnnouncementId,
                    ConversationId = x.Id,
                    FullName = x.AnnouncementCreatorId == caller.Id
                ? x.Questioner.FullName
                : x.AnnouncementCreator.FullName,
                    MessageBodyType = x.Messages.OrderByDescending(s => s.CreatedDt).Select(s => s.MessageBodyType)
                .FirstOrDefault(),
                    MessageDate = x.Messages.OrderByDescending(s => s.CreatedDt).Select(s => s.CreatedDt)
                .FirstOrDefault(),
                    MessageText = x.Messages.OrderByDescending(s => s.CreatedDt).Select(s => s.MessageText)
                .FirstOrDefault(),
                    UnSeenCount = x.Messages.Where(s => s.SenderId != caller.Id).Count(s => !s.IsSeen),
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.ProfilePhoto,
                    x.AnnouncementCreatorId == caller.Id
                        ? x.Questioner.ProfilePhoto
                        : x.AnnouncementCreator.ProfilePhoto, 300, 300, false, 0),
                        PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.ProfilePhoto,
                    x.AnnouncementCreatorId == caller.Id
                        ? x.Questioner.ProfilePhoto
                        : x.AnnouncementCreator.ProfilePhoto, 100, 100, true, 0)
                    }
                }).ToListAsync();
            foreach (var variable in result)
            {
                if (variable.MessageBodyType == MessageBodyType.Text)
                    continue;
                variable.MessageText = variable.MessageBodyType == MessageBodyType.File ? "Sent Photo" : "Sent Image";
            }
            return new PagingResponseModel<ConversationListModel>
            {
                Data = result,
                DateFrom = null,
                ItemCount = count,
                PageCount = page
            };
        }

        public async Task<bool> Delete(int conversationId, int userId)
        {
            var user = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception("caller not found");
            var conversation = await _repository.Filter<Conversation>(x => x.Id == conversationId).FirstOrDefaultAsync();
            if (conversation == null)
                throw new Exception("not found");
            List<Message> mess = await _repository.Filter<Message>(x => x.ConversationId == conversation.Id).ToListAsync();
            var questioner = await _repository.Filter<Conversation>
                (x => x.Id == conversationId && x.QuestionerId == user.Id).FirstOrDefaultAsync();
            var announcementCreator = await _repository.Filter<Conversation>
                (x => x.Id == conversationId && x.AnnouncementCreatorId == user.Id).FirstOrDefaultAsync();
            if (questioner != null)
            {
                conversation.QuestionerConversationIsDeleted = true;
                foreach (var item in mess)
                {
                    if (item.SenderId != questioner.QuestionerId)
                        item.ReciverMessageIsDeleted = true;
                    if (item.SenderId == questioner.QuestionerId)
                        item.SenderMessageIsDeleted = true;
                }
            }
            else
            {
                conversation.AnnouncementCreatorConversationIsDeleted = true;
                foreach (var item in mess)
                {
                    if (item.SenderId != announcementCreator.AnnouncementCreatorId)
                        item.ReciverMessageIsDeleted = true;
                    if (item.SenderId == announcementCreator.AnnouncementCreatorId)
                        item.SenderMessageIsDeleted = true;
                }
            }
            _repository.Update(conversation);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}