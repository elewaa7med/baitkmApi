using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Conversations.SupportConversations;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Conversations;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Conversations.SupportConversations
{
    public class SupportConversationService : ISupportConversationService
    {
        private readonly IEntityRepository _repository;
        private readonly IOptionsBinder _optionsBinder;
        public SupportConversationService(IEntityRepository repository,
            IOptionsBinder optionsBinder)
        {
            _repository = repository;
            _optionsBinder = optionsBinder;
        }

        public async Task<PagingResponseModel<SupportConversationListModel>> GetList(SupportConversationPagingRequestModel model, int userId)
        {
            var caller = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (caller == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            var query = _repository.Filter<SupportConversation>(sp => sp.SupportMessages.Count(s => !s.IsDeleted) > 0);
            //if (!string.IsNullOrEmpty(model.Search))
            //{
            //    //model.Search = model.Search.ToLower();
            //    var searchParams = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //    foreach (var variable in searchParams)
            //    {
            //        query = query.Where(x =>
            //            x.UserId != null
            //                ? x.User.FullName.ToLower().Contains(variable) : x.GuestId.Value.ToString().Contains(variable));
            //    }
            //}
            if (!string.IsNullOrEmpty(model.Search))
            {
                //var searchParams = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                //foreach (var variable in searchParams)
                //{
                //    query = query.Where(x =>
                //        x.UserId != null
                //            ? x.User.FullName.ToLower().Contains(variable) : x.GuestId.Value.ToString().Contains(variable));
                //}
                query = query.Where(sp => sp.UserId != null
                    ? sp.User.FullName.Contains(model.Search) : sp.GuestId.ToString().Contains(model.Search));
            }
            var count = await query.CountAsync();
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count)));
            var result = await query.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(x => x.SupportMessages.OrderByDescending(s => s.CreatedDt).Select(s => s.CreatedDt).FirstOrDefault())
                .Select(x => new SupportConversationListModel
                {
                    Id = x.Id,
                    FullName = x.User != null ? x.User.FullName : $"Guest N {x.GuestId}",
                    SupportMessageBodyType = x.SupportMessages.OrderByDescending(s => s.CreatedDt)
                        .Select(s => s.SupportMessageBodyType)
                        .FirstOrDefault(),
                    MessageText = x.SupportMessages.OrderByDescending(s => s.CreatedDt).Select(s => s.MessageText)
                        .FirstOrDefault(),
                    UnSeenCount = x.SupportMessages.Where(s => s.UserSenderId != caller.Id).Count(s => !s.IsSeen),
                    Photo = x.User != null
                        ? new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, x.User.ProfilePhoto, 300, 300, false, 0),
                            PhotoBlur = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, x.User.ProfilePhoto, 100, 100, true, 0)
                        } : new ImageOptimizer()
                }).ToListAsync();
            foreach (var variable in result)
            {
                if (variable.SupportMessageBodyType == SupportMessageBodyType.Message)
                    continue;
                variable.MessageText = variable.SupportMessageBodyType == SupportMessageBodyType.Photo ? "Sent Photo" :
                    variable.SupportMessageBodyType == SupportMessageBodyType.Announcement ? "Sent Announcement" :
                    "Sent Image";
            }
            return new PagingResponseModel<SupportConversationListModel>
            {
                PageCount = page,
                DateFrom = null,
                ItemCount = count,
                Data = result
            };
        }

        public async Task<int> Create(int userId, Language language, string deviceId)
        {
            Guest guest = null;
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
                if (guest == null)
                    throw new Exception(_optionsBinder.Error().UserNotFound);
            }

            var admin = await _repository.Filter<User>(x => x.RoleEnum == Role.Admin).FirstOrDefaultAsync();
            if (admin == null)
                throw new Exception("configuration not found");
            if (user?.Id == admin.Id)
                throw new Exception(_optionsBinder.Error().ConversationWithYou);
            var conversation = new SupportConversation
            {
                AdminId = admin.Id
            };
            if (user != null)
            {
                conversation.UserId = user.Id;
                conversation.GuestId = null;
            }
            else
            {
                conversation.UserId = null;
                conversation.GuestId = guest.Id;
            }
            _repository.Create(conversation);
            await _repository.SaveChangesAsync();
            return conversation.Id;
        }
    }
}