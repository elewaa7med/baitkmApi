using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.Persons;
using Baitkm.DTO.ViewModels.Subscription;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.Subscriptions;
using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Users.Guests
{
    public class GuestService : IGuestService
    {
        private readonly IEntityRepository _repository;
        private readonly IOptionsBinder _optionsBinder;
        public GuestService(IEntityRepository repository,
            IOptionsBinder optionsBinder)
        {
            _repository = repository;
            _optionsBinder = optionsBinder;
        }

        public async Task<GuestProfileModel> GuestProfileAsync(Language language, string deviceId)
        {
            var guest = await _repository.FilterAsNoTracking<Guest>(x => x.DeviceId == deviceId).FirstOrDefaultAsync();
            var favoriteCount = await _repository.FilterAsNoTracking<Favourite>(f => f.GuestId == guest.Id).CountAsync();
            var saveFilterCount = await _repository.FilterAsNoTracking<SaveFilter>(sf => sf.GuestId == guest.Id).CountAsync();
            return new GuestProfileModel
            {
                SaveFilterCount = saveFilterCount,
                FavoriteCount = favoriteCount,
                CurrencyId = guest.CurrencyId
            };
        }

        public async Task<bool> AddGuestAsync(string deviceId, string deviceToken,
            OsType osType, Language language, string currencyCode)
        {
            Currency currency = _repository.Filter<Currency>(c => c.Code == currencyCode && !c.IsDeleted).FirstOrDefault();

            if (string.IsNullOrEmpty(deviceId))
                throw new Exception(_optionsBinder.Error().RequiredParameters);
            var guest = await _repository.Filter<Guest>(x => x.DeviceId == deviceId).FirstOrDefaultAsync();
            if (guest != null)
            {
                var otherSetting = await _repository.Filter<PersonOtherSetting>(x => x.GuestId == guest.Id).FirstOrDefaultAsync();
                if (otherSetting != null)
                {
                    otherSetting.Language = language;
                    otherSetting.AreaUnit = AreaUnit.SquareMeter;
                    _repository.Update(otherSetting);
                }
                else
                {
                    await _repository.CreateAsync(new PersonOtherSetting
                    {
                        GuestId = guest.Id,
                        Language = language,
                        AreaUnit = AreaUnit.SquareMeter
                    });
                }
                await _repository.SaveChangesAsync();
                return true;
            }
            else if (guest == null)
            {
                var newGuest = _repository.Create(new Guest
                {
                    DeviceId = deviceId,
                    OsType = osType,
                    Token = deviceToken,
                    CurrencyId = currency.Id
                });
                foreach (SubscriptionsType variable in Enum.GetValues(typeof(SubscriptionsType)))
                {
                    if (variable == SubscriptionsType.PhoneNumberVisibility)
                        continue;
                    _repository.Create(new PersonSetting
                    {
                        GuestId = newGuest.Id,
                        SubscriptionsType = variable
                    });
                }
                _repository.Create(new PersonOtherSetting
                {
                    GuestId = newGuest.Id,
                    AreaUnit = AreaUnit.SquareMeter,
                    Language = language
                });
            }
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetSupportConversationId(Language language, string deviceId)
        {
            var guest = await _repository.Filter<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
            var admin = await _repository.Filter<User>(u => u.RoleEnum == Role.Admin).FirstOrDefaultAsync();
            var support = await _repository.Filter<SupportConversation>(sc => sc.GuestId == guest.Id).FirstOrDefaultAsync();
            if (support == null)
            {
                support = _repository.Create(new SupportConversation
                {
                    AdminId = admin.Id,
                    GuestId = guest.Id
                });
                await _repository.SaveChangesAsync();
            }
            return support.Id;
        }

        public async Task<bool> EditSubscription(UpdateSubscriptionModel model, string deviceId)
        {
            var guest = await _repository.Filter<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
            var otherSetting = await _repository.Filter<PersonOtherSetting>(pos => pos.GuestId == guest.Id).FirstOrDefaultAsync();
            var settings = await _repository.Filter<PersonSetting>(x => x.GuestId == guest.Id).ToListAsync();
            _repository.HardDeleteRange(settings);

            foreach (var variable in model.Subscriptions)
            {
                _repository.Create(new PersonSetting
                {
                    GuestId = guest.Id,
                    SubscriptionsType = variable
                });
            }
            if (otherSetting != null)
            {
                otherSetting.AreaUnit = model.AreaUnit;
                otherSetting.Language = model.Language;
                _repository.Update(otherSetting);
            }
            else
            {
                await _repository.CreateAsync(new PersonOtherSetting
                {
                    GuestId = guest.Id,
                    Language = model.Language
                });
            }
            guest.CurrencyId = model.CurrencyId;
            _repository.Update(guest);
            if (guest.CurrencyId == 0)
                throw new Exception("Currency id can not be 0!");
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<UpdateSubscriptionModel> GetSubscription(string deviceId)
        {
            var guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
            var subscriptions = await _repository.FilterAsNoTracking<PersonSetting>(ps => ps.GuestId == guest.Id)
                .Select(x => x.SubscriptionsType).ToListAsync();
            var otherSetting = await _repository.FilterAsNoTracking<PersonOtherSetting>(pos => pos.GuestId == guest.Id).FirstOrDefaultAsync();
            Currency currency = _repository.Filter<Currency>(c => c.Id == guest.CurrencyId).FirstOrDefault();
            return new UpdateSubscriptionModel
            {
                Subscriptions = subscriptions,
                AreaUnit = otherSetting?.AreaUnit ?? AreaUnit.SquareMeter,
                Language = otherSetting?.Language ?? Language.English,
                CurrencyId = guest.CurrencyId,
                CurrencySymbol = currency.Symbol,
                CurrencyCode = currency.Code
            };
        }

        public async Task<PagingResponseModel<PersonNotificationModel>> GuestNotificationList(PagingRequestModel model,
            string deviceId, Language language)
        {
            var guest = await _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).FirstOrDefaultAsync();
            var personNotifications = _repository.FilterAsNoTracking<PersonNotification>(n => n.GuestId == guest.Id);
            var result = personNotifications.Skip((model.Page - 1) * model.Count).Take(model.Count)
                .OrderByDescending(pn => pn.CreatedDt)
                .Select(pn => new PersonNotificationModel
                {
                    Title = language == Language.English ? pn.PushNotification.Title
                        : pn.PushNotification.PushNotificationTranslates.Select(p => p.Title).FirstOrDefault(),
                    Description = language == Language.English ? pn.PushNotification.Description
                        : pn.PushNotification.PushNotificationTranslates.Select(p => p.Description).FirstOrDefault(),
                    NotificationSendDate = pn.CreatedDt
                }).AsEnumerable();
            var unSeenNotifications = personNotifications.Where(pn => !pn.IsSeen).ToList();
            foreach (var notification in unSeenNotifications)
            {
                notification.IsSeen = true;
            }
            _repository.UpdateRange(unSeenNotifications);
            await _repository.SaveChangesAsync();
            return new PagingResponseModel<PersonNotificationModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? personNotifications.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = personNotifications.Count(),
                PageCount = Convert.ToInt32(Math.Ceiling(decimal.Divide(personNotifications.Count(), model.Count)))
            };
        }

        public bool CheckExistGuest(string deviceId)
        {
            return _repository.FilterAsNoTracking<Guest>(g => g.DeviceId == deviceId).Any();
        }
    }
}