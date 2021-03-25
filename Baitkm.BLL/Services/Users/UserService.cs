using Baitkm.BLL.Services.Scheduler.Jobs;
using Baitkm.BLL.Services.Token;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels;
using Baitkm.DTO.ViewModels.ForgotPasswords;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.Persons;
using Baitkm.DTO.ViewModels.Persons.Authentication;
using Baitkm.DTO.ViewModels.Persons.Users;
using Baitkm.DTO.ViewModels.Persons.Users.Verification;
using Baitkm.DTO.ViewModels.Subscription;
using Baitkm.DTO.ViewModels.Token;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.Subscriptions;
using Baitkm.Enums.UserRelated;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.Binders;
using Baitkm.Infrastructure.Helpers.Models;
using Baitkm.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IEntityRepository _repository;
        private readonly ITokenService _tokenService;
        private readonly MediaAccessor _accessor;
        private readonly IOptionsBinder _optionsBinder;
        private readonly IOptions<ErrorMessagesEnglish> errorMessages;

        public UserService(IEntityRepository repository,
            IOptions<TokenProviderOptions> options,
            IOptions<AppSettings> settings,
            IHostingEnvironment hostingEnvironment,
            ITokenService tokenService,
            IOptionsBinder optionsBinder,
            IOptions<ErrorMessagesEnglish> errorMessage)
        {
            _repository = repository;
            var option = options.Value;
            option.SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.Value.SecretKey)),
                SecurityAlgorithms.HmacSha256);
            option.Audience = "BaitkmReactJsWebApp";
            option.Issuer = "Baitkm" + hostingEnvironment.EnvironmentName;
            _tokenService = tokenService;
            _accessor = new MediaAccessor();
            _optionsBinder = optionsBinder;
            this.errorMessages = errorMessage;
        }

        public async Task<TokenResponse> RegisterAsync(RegisterModel model, string deviceToken, OsType osType,
            Language language, string deviceId, string currencyCode)
        {
            User user;
            var existUser = await _repository.FilterAsNoTracking<User>(u => (u.Email == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Email)
                 || (u.PhoneCode == model.PhoneCode && u.Phone == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Phone)).FirstOrDefaultAsync();
            if (existUser != null)
                throw new Exception(_optionsBinder.Error().UsernameExists);
            var verified = await _repository.Filter<Verified>(v => v.Email == model.VerificationTerm && v.VerifiedBy == VerifiedBy.Email
                || (v.PhoneCode == model.PhoneCode && v.Phone == model.VerificationTerm && v.VerifiedBy == VerifiedBy.Phone)).FirstOrDefaultAsync();

            if (verified == null || !verified.IsVerified)
                throw new Exception(_optionsBinder.Error().IsNotVerified);
            if (verified.IsRegistered)
                throw new Exception("Contact already is exist");
            verified.IsRegistered = true;
            _repository.Update(verified);

            int currencyId = 1;
            var currency = await _repository.FilterAsNoTracking<Currency>(c => c.Code == currencyCode).FirstOrDefaultAsync();
            if (currency != null)
                currencyId = currency.Id;
            user = new User
            {
                FullName = model.FullName,
                DateOfBirth = model.DateOfBirth,
                VerifiedBy = VerifiedBy.Phone,
                OsType = osType,
                RoleEnum = Role.User,
                IsLocal = true,
                CurrencyId = currencyId
            };
            if (Regex.Replace(model.VerificationTerm, @"\s+", "").All(c => Char.IsDigit(c)))
            {
                user.PhoneCode = model.PhoneCode.Trim();
                user.Phone = model.VerificationTerm.Trim();
                user.Email = model.PhoneEmail.Trim();
                user.VerifiedBy = VerifiedBy.Phone;
            }
            else
            {
                user.PhoneCode = model.PhoneCode.Trim();
                user.Phone = model.PhoneEmail.Trim();
                user.Email = model.VerificationTerm.Trim();
                user.VerifiedBy = VerifiedBy.Email;
            }
            _repository.Create(user);
            if (model.CityId != 0)
                user.CityId = model.CityId;
            else
            {
                var city = _repository.Create(new City
                {
                    Name = model.CityName
                });
                user.CityId = city.Id;
            }
            var password = Utilities.HashPassword(model.Password);
            _repository.Create(new Password
            {
                LoginProvider = SocialLoginProvider.Local,
                PasswordHash = Utilities.HashPassword(model.Password),
                User = user,
                UniqueIdentifier = "0"
            });
            foreach (SubscriptionsType variable in Enum.GetValues(typeof(SubscriptionsType)))
            {
                _repository.Create(new PersonSetting
                {
                    User = user,
                    SubscriptionsType = variable
                });
            }
            _repository.Create(new PersonOtherSetting
            {
                User = user,
                AreaUnit = AreaUnit.SquareMeter,
                Language = language
            });
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                var device = await _repository.Filter<DeviceToken>(d => d.DeviceId == deviceId).FirstOrDefaultAsync();
                if (device == null)
                {
                    device = new DeviceToken
                    {
                        DeviceId = deviceId,
                        OsType = osType,
                        Token = deviceToken,
                        User = user
                    };
                    _repository.Create(device);
                }
                else
                {
                    device.OsType = osType;
                    device.Token = deviceToken;
                    device.UserId = user.Id;
                    _repository.Update(device);
                }
            }
            TokenViewModel tokenView;
            if (model.VerificationTerm.Contains("@"))
            {
                tokenView = new TokenViewModel
                {
                    VerificationTerm = model.VerificationTerm,
                    Password = model.Password
                };
            }
            else
            {
                tokenView = new TokenViewModel
                {
                    VerificationTerm = model.PhoneCode + model.VerificationTerm,
                    Password = model.Password
                };
            }
            await _repository.SaveChangesAsync();
            if (user.VerifiedBy == VerifiedBy.Email)
            {
                var html = Utilities.NewUserEmailStyle(user.Email);
                Utilities.SendEmail(user.Email, "Baitkm", html);
            }
            var token = await _tokenService.TokenAsync(tokenView, deviceToken, osType, language, deviceId);
            return new TokenResponse
            {
                Id = user.Id,
                AccessToken = token.AccessToken,
                DateTime = token.DateTime,
                Provider = SocialLoginProvider.Local
            };
        }

        public async Task<bool> EditAsync(UserEditModel model, int userId, Language language)
        {
            var user = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            //var name = model.FullName.Trim();
            //if (!Regex.IsMatch(model.FullName, @"^[\p{L} \.\-]+$"))
            //    throw new ArgumentException(_optionsBinder.Error().SpecialCharacters);
            //name = name.Replace("'", "&#39;");
            //dynamic property = new ExpandoObject();
            //if (model.CityId != null)
            //    property.CityId = model.CityId.Value;
            //else
            //{
            //    var city = _repository.Create(new City
            //    {
            //        Name = model.CityName
            //    });
            //    property.CityId = city.Id;
            //}
            if (model.CityId.HasValue || model.CityId > 0)
            {
                City city = _repository.Filter<City>(c => c.Id == model.CityId.Value).Include(c => c.Country).FirstOrDefault();
                if (city == null)
                    throw new Exception("Invalid city id");
                user.City = city;
                user.CityId = city.Id;
                _repository.Update(city);
                if (model.CountryId.HasValue)
                {
                    city.Country.Id = model.CountryId.Value;
                    _repository.Update(city.Country);
                }
            }
            if (user.VerifiedBy == VerifiedBy.Email)
            {
                if (string.IsNullOrEmpty(model.PhoneCode))
                    throw new Exception(_optionsBinder.Error().EnterTheCode);
                if (string.IsNullOrEmpty(model.PhoneEmail))
                    throw new Exception(_optionsBinder.Error().EnterThePhone);
                user.FullName = model.FullName;
                user.DateOfBirth = model.DateOfBirth;
                user.PhoneCode = model.PhoneCode;
                user.Phone = model.PhoneEmail;
            }
            else
            {
                if (string.IsNullOrEmpty(model.PhoneEmail))
                    throw new Exception(_optionsBinder.Error().EnterEmail);
                if (!model.PhoneEmail.Contains("@") && !model.PhoneEmail.Contains("."))
                    throw new Exception(_optionsBinder.Error().EnterTheCorrectEmail);
                user.FullName = model.FullName;
                user.DateOfBirth = model.DateOfBirth;
                user.Email = model.PhoneEmail;
            }

            _repository.Update(user);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<UserDetailsModel> UserDetailsAsync(int userId, Language language)
        {
            var result = await _repository.Filter<User>(u => u.Id == userId)
                .Select(u => new UserDetailsModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    VerifiedBy = u.VerifiedBy,
                    PhoneCode = u.PhoneCode,
                    Phone = u.Phone,
                    Email = u.Email,
                    CityId = u.CityId ?? 0,
                    City = $"{u.City.Name}, {u.City.Country.Name}",
                    DateOfBirth = u.DateOfBirth,
                    IsLocal = u.IsLocal,
                    OsType = u.OsType,
                    CountryId = u.City.CountryId,
                    CountryName = u.City.Country.Name,
                    ProfilePhoto = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                           UploadType.ProfilePhoto, u.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                    UnSeenNotificationCount = u.PersonNotifications.Count(s => !s.IsDeleted && !s.IsSeen),
                    ActiveAnnouncementCount = u.Announcements.Count(s => !s.IsDeleted && !s.IsDraft
                        && s.AnnouncementStatus == AnnouncementStatus.Accepted || s.TopAnnouncement),
                    HiddenAnnouncementCount = u.Announcements.Count(s => !s.IsDeleted && !s.IsDraft
                        && s.AnnouncementStatus == AnnouncementStatus.Hidden)
                }).FirstOrDefaultAsync();
            if (result == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            var conversation = await _repository.FilterAsNoTracking<Conversation>(x => x.AnnouncementCreatorId == result.Id
                 || x.QuestionerId == result.Id).ToListAsync();
            var messagesCount = _repository.Filter<Message>(m => !m.IsSeen
                && conversation.Select(c => c.Id).Contains(m.ConversationId) && m.SenderId != result.Id).Distinct().Count();
            result.UnreadConversationCount += messagesCount;
            return result;
        }

        public async Task<UserProfileModel> UserProfileAsync(int userId)
        {
            var result = await _repository.FilterAsNoTracking<User>(u => u.Id == userId)
                .Select(u => new UserProfileModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    VerificationTerm = u.VerifiedBy == VerifiedBy.Email ? u.Email : (u.PhoneCode + u.Phone),
                    ProfilePhoto = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                            UploadType.ProfilePhoto, u.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                    LoginProvider = u.Passwords.Where(s => s.UserId == u.Id).Select(s => s.LoginProvider).FirstOrDefault(),
                    PhoneCode = u.PhoneCode,
                    Phone = u.Phone,
                    Email = u.Email,
                    DateOfBirth = u.DateOfBirth,
                    IsBlocked = u.IsBlocked,
                    VerifiedBy = u.VerifiedBy,
                    IsLocal = u.IsLocal,
                    Subscriptions = u.PersonSettings.Select(y => y.SubscriptionsType).ToList(),
                    City = u.City == null ? null : $"{u.City.Name}, {u.City.Country.Name}",
                    CityId = u.CityId ?? 0,
                    OsType = u.OsType,
                    CurrencyId = u.CurrencyId,
                    ConversationId = u.SupportConversation == null ? 0 : u.SupportConversation.Id,
                    UnSeenNotificationCount = u.PersonNotifications.Count(s => !s.IsDeleted && !s.IsSeen),
                    ActiveAnnouncementCount = u.Announcements.Count(s => !s.IsDeleted && !s.IsDraft
                        && s.AnnouncementStatus == AnnouncementStatus.Accepted || s.TopAnnouncement),
                    HiddenAnnouncementCount = u.Announcements.Count(s => !s.IsDeleted && !s.IsDraft
                        && s.AnnouncementStatus == AnnouncementStatus.Hidden),
                    MyAnnouncementCount = u.Announcements.Count(s => !s.IsDeleted && s.UserId == u.Id && !s.IsDraft),
                    SaveFilterCount = u.SaveFilters.Count(s => !s.IsDeleted && s.UserId == u.Id),
                    FavoriteCount = u.Favourites.Count(s => !s.IsDeleted && s.UserId == u.Id
                        && s.Announcement.AnnouncementStatus == AnnouncementStatus.Accepted)
                }).FirstOrDefaultAsync();

            Currency currency = _repository.Filter<Currency>(c => c.Id == result.CurrencyId).FirstOrDefault();
            result.CurrencyCode = currency.Code;
            result.UnreadConversationCount = _repository.Filter<Message>(m => m.ReciverId == result.Id && !m.IsSeen).GroupBy(k => k.ConversationId).Count();
            var conversation = await _repository.FilterAsNoTracking<Conversation>(x => x.AnnouncementCreatorId == result.Id
                 || x.QuestionerId == result.Id).ToListAsync();
            var messagesCount = _repository.Filter<Message>(m => !m.IsSeen
                && conversation.Select(c => c.Id).Contains(m.ConversationId) && m.SenderId != result.Id).Distinct().Count();
            if (result.ConversationId == 0)
            {
                var admin = await _repository.Filter<User>(x => x.RoleEnum == Role.Admin).FirstOrDefaultAsync();
                if (admin == null)
                    throw new Exception("admin not found");
                var supportConversation = _repository.Create(new SupportConversation
                {
                    UserId = result.Id,
                    AdminId = admin.Id
                });
                await _repository.SaveChangesAsync();
                result.ConversationId = supportConversation.Id;
            }
            await _repository.SaveChangesAsync();
            return result;
        }

        public async Task<User> GetUser(string userName, VerifiedBy verifiedBy)
        {
            return await _repository.FilterAsNoTracking<User>(x => (x.Email == userName
                || (x.PhoneCode + x.Phone) == userName) && x.VerifiedBy == verifiedBy).FirstOrDefaultAsync();
        }

        public async Task<UserDetailsAdminModel> GetByIdAsync(int userId)
        {
            var res = await _repository.FilterAsNoTracking<User>(x => x.Id == userId)
                .Select(u => new UserDetailsAdminModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneCode = u.PhoneCode,
                    Phone = u.Phone,
                    CityId = u.CityId ?? 0,
                    City = u.City.Name,
                    DateOfBirth = u.DateOfBirth,
                    IsLocal = u.IsLocal,
                    OsType = u.OsType,
                    IsBlocked = u.IsBlocked,
                    IpAddress = u.UserLocation.IpAddress,
                    CityByIpAddress = u.UserLocation.City,
                    CountryByIpAddress = u.UserLocation.Country,
                    ProfilePhoto = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                          UploadType.ProfilePhoto, u.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                    ActiveAnnouncementCount = u.Announcements.Count(s => !s.IsDeleted && s.AnnouncementStatus == AnnouncementStatus.Accepted),
                    HiddenAnnouncementCount = u.Announcements.Count(s => !s.IsDeleted && s.AnnouncementStatus == AnnouncementStatus.Hidden),
                    VerifiedBy = u.VerifiedBy
                }).FirstOrDefaultAsync();
            var l = UserActivityCounter(res.Id);
            res.Activities = await l;
            res.AverageDailyUsage = l.Result.Where(d => d.Day.Date == DateTime.Now.Date).Select(d => d.Duration).FirstOrDefault();
            return res;
        }

        public async Task<PagingResponseModel<UserViewModel>> GetUserListAsync(PagingRequestModel model)
        {
            var users = _repository.GetAll<User>();
            var usersCount = users.Count();
            var announcementsCount = _repository.FilterAsNoTracking<Announcement>(x => !x.IsDraft && x.UserId == x.Id).Count();
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(usersCount, model.Count)));
            var result = await users.OrderByDescending(x => x.CreatedDt).Skip((model.Page - 1) * model.Count)
                .Take(model.Count).Select(x =>
                    new UserViewModel
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        AnnouncementCount = announcementsCount,
                        Email = x.Email,
                        Phone = x.Phone,
                        City = x.City.Name,
                        CityId = x.CityId ?? 0,
                        UserStatusType = x.UserStatusType,
                        IsBlocked = x.IsBlocked,
                        IpLocation = x.UserLocation.Country,
                        ProfilePhoto = new ImageOptimizer
                        {
                            Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, x.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                        }
                    }).ToListAsync();
            return new PagingResponseModel<UserViewModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? users.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = usersCount,
                PageCount = page
            };
        }

        public async Task<ImageOptimizer> Photo(UploadFileModel model, int userId)
        {
            var user = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(user.ProfilePhoto))
                await _accessor.Remove(user.ProfilePhoto, UploadType.ProfilePhoto);
            var result = await _accessor.Upload(model.File, UploadType.ProfilePhoto);
            user.ProfilePhoto = Path.GetFileName(result);
            _repository.Update(user);
            await _repository.SaveChangesAsync();
            return new ImageOptimizer
            {
                Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                    UploadType.ProfilePhoto, result, 200, 200)
            };
        }

        public async Task<bool> EditSubscription(UpdateSubscriptionModel model, int userId)
        {
            var user = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            var saveFilters = await _repository.Filter<SaveFilter>(sf => sf.UserId == user.Id).ToListAsync();
            var otherSetting = await _repository.Filter<PersonOtherSetting>(x => x.UserId == user.Id).FirstOrDefaultAsync();
            var settings = await _repository.Filter<PersonSetting>(ps => ps.UserId == user.Id).ToListAsync();
            string areaType = null;
            if (otherSetting == null)
                areaType = "m²";
            else
            {
                if (otherSetting.AreaUnit == AreaUnit.SquareFut)
                    areaType = "ft²";
                else
                    areaType = "m²";
            }
            _repository.HardDeleteRange(settings);
            foreach (var variable in model.Subscriptions)
            {
                _repository.Create(new PersonSetting
                {
                    UserId = user.Id,
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
                    UserId = user.Id,
                    Language = model.Language
                });
            }
            //foreach (var saveFilter in saveFilters)
            //{
            //    if (saveFilter.Description != null)
            //    {
            //        if (model.Language == Language.English)
            //        {
            //            if (saveFilter.AnnouncementType != null)
            //            {
            //                switch (saveFilter.AnnouncementType)
            //                {
            //                    case AnnouncementType.Rent:
            //                        saveFilter.Description = "Rent";
            //                        break;
            //                    case AnnouncementType.Sale:
            //                        saveFilter.Description = "Sale";
            //                        break;
            //                }
            //            }
            //            else if (saveFilter.AnnouncementEstateType != null)
            //            {
            //                switch (saveFilter.AnnouncementEstateType)
            //                {
            //                    case AnnouncementEstateType.Residential:
            //                        saveFilter.Description = "Residential";
            //                        break;
            //                    case AnnouncementEstateType.Commercial:
            //                        saveFilter.Description = "Commercial";
            //                        break;
            //                    case AnnouncementEstateType.Land:
            //                        saveFilter.Description = "Land";
            //                        break;
            //                }
            //            }
            //            _repository.Update(saveFilter);
            //        }
            //        else
            //        {
            //            if (saveFilter.AnnouncementType != null)
            //            {
            //                switch (saveFilter.AnnouncementType)
            //                {
            //                    case AnnouncementType.Rent:
            //                        saveFilter.Description = "تأجير";
            //                        break;
            //                    case AnnouncementType.Sale:
            //                        saveFilter.Description = "البيع";
            //                        break;
            //                }
            //            }
            //            else if (saveFilter.AnnouncementEstateType != null)
            //            {
            //                switch (saveFilter.AnnouncementEstateType)
            //                {
            //                    case AnnouncementEstateType.Residential:
            //                        saveFilter.Description = "سكني";
            //                        break;
            //                    case AnnouncementEstateType.Commercial:
            //                        saveFilter.Description = "تجاري";
            //                        break;
            //                    case AnnouncementEstateType.Land:
            //                        saveFilter.Description = "الأرض";
            //                        break;
            //                }
            //            }
            //            _repository.Update(saveFilter);
            //        }
            //        if ((saveFilter.Description.Contains("ft²")) && model.AreaUnit == AreaUnit.SquareMeter)
            //        {
            //            var line = saveFilter.Description.IndexOf("-");
            //            var firstAreaUnit = saveFilter.Description.IndexOf(areaType);
            //            var lastAreaUnit = saveFilter.Description.IndexOf(areaType, line);

            //            var firstNumberString = saveFilter.Description.Remove(firstAreaUnit);
            //            var Number = saveFilter.Description.Remove(lastAreaUnit);
            //            var secondNumberString = Number.Substring(line + 1);
            //            var firstNumber = Int32.Parse(firstNumberString);
            //            var secondNumber = Int32.Parse(secondNumberString);

            //            var firstNumberFutToMeter = (int)firstNumber * 0.3048;
            //            var secondNumberFutToMeter = (int)secondNumber * 0.3048;
            //            saveFilter.Description = ($"{Convert.ToInt32(firstNumberFutToMeter)}{"m²"} - {Convert.ToInt32(secondNumberFutToMeter)}{"m²"}");
            //        }
            //        ////else if (saveFilter.Description.Contains("m²") && model.AreaUnit == AreaUnit.SquareFut)
            //        ////{
            //        ////    var line = saveFilter.Description.IndexOf("-");
            //        ////    var firstAreaUnit = saveFilter.Description.IndexOf(areaType);
            //        ////    //var lastAreaUnit = saveFilter.Description.IndexOf(areaType, line);

            //        ////    var firstNumberString = saveFilter.Description.Remove(firstAreaUnit);
            //        ////    var Number = saveFilter.Description.Remove(/*lastAreaUnit*/0);
            //        ////    var secondNumberString = Number.Substring(line + 1);
            //        ////    var firstNumber = Int32.Parse(firstNumberString);
            //        ////    var secondNumber = Int32.Parse(secondNumberString);

            //        ////    var firstNumberMeterToFut = firstNumber / 0.3048;
            //        ////    var secondNumberMeterToFut = secondNumber / 0.3048;

            //        ////    propertySaveFilters.Description = ($"{Convert.ToInt32(firstNumberMeterToFut)}{"ft²"} - {Convert.ToInt32(secondNumberMeterToFut)}{"ft²"}");
            //        ////}
            //        _repository.Update(saveFilter);
            //    }
            //}

            if (model.CurrencyId > 0)
            {
                user.CurrencyId = model.CurrencyId;
                _repository.Update(user);
            }
            if (user.CurrencyId == 0)
                throw new Exception("Currency id can not be 0!");

            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<UpdateSubscriptionModel> GetSubscription(int userId)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            var otherSetting = await _repository.FilterAsNoTracking<PersonOtherSetting>(pos => pos.UserId == user.Id).FirstOrDefaultAsync();
            var subscriptions = await _repository.FilterAsNoTracking<PersonSetting>(ps => ps.UserId == user.Id)
                .Select(x => x.SubscriptionsType).ToListAsync();
            var currency = _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefault();
            return new UpdateSubscriptionModel
            {
                Subscriptions = subscriptions,
                AreaUnit = otherSetting == null ? AreaUnit.SquareMeter : otherSetting.AreaUnit,
                Language = otherSetting == null ? Language.English : otherSetting.Language,
                CurrencyId = currency.Id,
                CurrencyCode = currency.Code,
                CurrencySymbol = currency.Symbol
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordModel model, int userId, Language language)
        {
            if (model.NewPassword != model.ConfirmPassword)
                throw new Exception(_optionsBinder.Error().ConfirmPassword);
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            var oldHashed = await _repository.Filter<Password>(p => p.UserId == user.Id
                && p.LoginProvider == SocialLoginProvider.Local).FirstOrDefaultAsync();
            if (oldHashed == null)
                throw new Exception("oldhashed not found");
            var isSame = Utilities.VerifyHashedPassword(oldHashed.PasswordHash, model.OldPassword);
            if (!isSame)
                throw new Exception(_optionsBinder.Error().OldPassword);
            _repository.HardDelete(oldHashed);
            _repository.Create(new Password
            {
                LoginProvider = SocialLoginProvider.Local,
                PasswordHash = Utilities.HashPassword(model.NewPassword),
                UserId = user.Id
            });
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForgotPasswordCheckVerified(SendKeyForgotPasswordModel model, Language language)
        {
            string bodyText;
            string subject;
            if (language == Language.English)
            {
                bodyText = "Your verification key is";
                subject = "Baitkm Password Recovery code";
            }
            else
            {
                subject = "بيتكم استعادة كلمة السر";
                bodyText = "مفتاح التحقق الخاص بك هو";
            }
            model.VerificationTerm = model.VerificationTerm.ToLower();
            var user = await _repository.FilterAsNoTracking<User>(u => (u.Email == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Email)
                 || (u.PhoneCode == model.PhoneCode && u.Phone == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Phone)).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            var key = Utilities.KeyGenerator(4);
            user.ForgotKey = key;
            _repository.Update(user);
            if (model.VerificationTerm.Contains("@"))
                Utilities.SendKeyByEmail(model.VerificationTerm, key, subject, bodyText);
            else
                Utilities.SendKeyByTwilio(model.PhoneCode + model.VerificationTerm, key, bodyText);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForgotPasswordChangeVerified(CheckForgotKeyModel model, Language language)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => (u.Email == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Email)
                 || (u.PhoneCode == model.PhoneCode && u.Phone == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Phone)).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            if (model.Code != user.ForgotKey)
                throw new Exception(_optionsBinder.Error().VerificationCode);
            user.ForgotPasswordVerified = VerifiedType.Verify;
            _repository.Update(user);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForgotPasswordChangePassword(ForgotPasswordChangePasswordModel model, Language language)
        {
            if (model.Password != model.ConfirmPassword)
                throw new Exception(_optionsBinder.Error().PasswordDoNotMatch);
            var user = await _repository.FilterAsNoTracking<User>(u => (u.Email == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Email)
                 || (u.PhoneCode == model.PhoneCode && u.Phone == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Phone)).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            var passwords = await _repository.Filter<Password>(x => x.UserId == user.Id).ToListAsync();
            _repository.HardDeleteRange(passwords);
            var passwordHash = Utilities.HashPassword(model.Password);
            _repository.Create(new Password
            {
                PasswordHash = passwordHash,
                UserId = user.Id,
                LoginProvider = SocialLoginProvider.Local
            });
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Logout(string deviceId, OsType osType)
        {
            var deviceTokens = await _repository.Filter<DeviceToken>(dt => dt.DeviceId == deviceId && dt.OsType == osType).ToListAsync();
            _repository.HardDeleteRange(deviceTokens);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<string> Block(int userId, int day)
        {
            var user = _repository.GetById<User>(userId);
            if (user == null)
                throw new Exception("User not found");
            var language = await _repository.Filter<PersonOtherSetting>(pos => pos.UserId == user.Id)
                .Select(pos => pos.Language).FirstOrDefaultAsync();
            string unblockDate = null;
            if (language == Language.English)
                unblockDate = "This user will be unblocked on ";
            else
                unblockDate = "سيتم إلغاء حظر هذا المستخدم على ";
            var date = DateTime.Now.AddDays(day);
            date = new DateTime(date.Year, date.Month, date.Day);
            user.IsBlocked = true;
            user.UserStatusType = UserStatusType.Inactive;
            user.UnBlockDate = date;
            _repository.Update(user);
            await _repository.SaveChangesAsync();
            var unBlockDayTriggerBuilder = TriggerBuilder.Create()
                    .WithIdentity($"{nameof(UserUnBlockDayJob)}Trigger#{UserUnBlockDayJob.Name}{user.Id}")
                    .StartAt(date);
            dynamic unBlockDay = new ExpandoObject();
            unBlockDay.userId = user.Id;
            await SchedulerHelper.Schedule<UserUnBlockDayJob, IJobListener>(new QuartzScheduleModel
            {
                Name = $"{UserUnBlockDayJob.Name}{user.Id}",
                IsListenerRequested = false,
                DataMap = unBlockDay,
                TriggerBuilder = unBlockDayTriggerBuilder
            });
            return $"{unblockDate} {date}.";
        }

        public async Task<bool> UnBlock(int userId)
        {
            var user = _repository.GetById<User>(userId);
            if (user == null)
                throw new Exception("User not found");
            var name = ($"{UserUnBlockDayJob.Name}{user.Id}").ToString();
            await SchedulerHelper.UnSchedule<UserUnBlockDayJob>(name);
            user.IsBlocked = false;
            user.UserStatusType = UserStatusType.Active;
            _repository.Update(user);
            await _repository.SaveChangesAsync();
            return true;
        }
        //TO DO test
        //public async Task<bool> Delete(int userId, string userName, Language language)
        //{
        //    var caller = await _repository.Filter<User>(x => x.Email == userName && x.RoleEnum == Role.Admin).FirstOrDefaultAsync();
        //    if (caller == null)
        //        throw new Exception(_optionsBinder.Error().DeleteUsers);
        //    var user = _repository.GetById<User>(userId);
        //    user.CheckIfExist();
        //    var announcements = await _repository.Filter<Announcement>(x => x.UserId == user.Id).ToListAsync();
        //    var photoIds = await _repository.Filter<AnnouncementAttachment>(x =>
        //        announcements.Select(a => a.Id).Contains(x.AnnouncementId)).Select(s => s.Id).ToListAsync();
        //    var favoriteIds = await _repository.Filter<Favourite>(x =>
        //        announcements.Select(a => a.Id).Contains(x.AnnouncementId)).Select(s => s.Id).ToListAsync();
        //    var guestFavoriteIds = await _repository.Filter<GuestFavourite>(x =>
        //        announcements.Select(a => a.Id).Contains(x.AnnouncementId)).Select(s => s.Id).ToListAsync();
        //    var announcementFeatureIds = await _repository.Filter<AnnouncementFeature>(x =>
        //        announcements.Select(a => a.Id).Contains(x.AnnouncementId)).Select(s => s.Id).ToListAsync();
        //    var conversationsAnnouncementIds = await _repository.Filter<Conversation>(x =>
        //        announcements.Select(a => a.Id).Contains(x.AnnouncementId)).Select(x => x.Id).ToListAsync();
        //    var announcementReportIds = await _repository.Filter<AnnouncementReport>(x =>
        //        announcements.Select(a => a.Id).Contains(x.AnnouncementId)).Select(x => x.Id).ToListAsync();
        //    await _repository.RemoveRange<AnnouncementAttachment>(photoIds);
        //    await _repository.RemoveRange<Favourite>(favoriteIds);
        //    await _repository.RemoveRange<GuestFavourite>(guestFavoriteIds);
        //    await _repository.RemoveRange<AnnouncementFeature>(announcementFeatureIds);
        //    await _repository.RemoveRange<Conversation>(conversationsAnnouncementIds);
        //    await _repository.RemoveRange<AnnouncementReport>(announcementReportIds);
        //    var verified = await _repository.Filter<Verified>(x => x.Email == user.Email
        //        || (x.PhoneCode + x.Phone) == x.Phone && x.VerifiedBy == user.VerifiedBy)
        //        .Select(x => x.Id).FirstOrDefaultAsync();
        //    var deviceTokens = await _repository.Filter<DeviceToken>(x => x.UserId == user.Id).Select(x => x.Id).ToListAsync();
        //    var favourites = await _repository.Filter<Favourite>(x => x.UserId == user.Id).Select(x => x.Id).ToListAsync();
        //    //var conversations = await _repository.Filter<Conversation>(x => x.AnnouncementCreatorId == user.Id
        //    //    || x.QuestionerId == user.Id).Select(x => x.Id).ToListAsync();
        //    //var supportConversations = await _repository.Filter<SupportConversation>(x => x.UserId == user.Id).Select(x => x.Id).ToListAsync();
        //    //var supportMessages = await _repository.Filter<SupportMessage>(x => x.UserSenderId == user.Id).Select(x => x.Id).ToListAsync();
        //    //var messages = await _repository.Filter<Message>(x => x.SenderId == user.Id).Select(x => x.Id).ToListAsync();
        //    var password = await _repository.Filter<Password>(x => x.UserId == user.Id).Select(x => x.Id).FirstOrDefaultAsync();
        //    var subscriptions = await _repository.Filter<Subscription>(x => x.UserId == user.Id).Select(x => x.Id).ToListAsync();
        //    var userSubscriptionAreaAndLanguages = await _repository.Filter<UserSubscriptionAreaAndLanguage>
        //        (x => x.UserId == user.Id).Select(x => x.Id).ToListAsync();
        //    var saveFilters = await _repository.Filter<SaveFilter>(x => x.UserId == user.Id).ToListAsync();
        //    var sav = await _repository.Filter<SaveFilterAnnouncementFeature>(x =>
        //        saveFilters.Select(s => s.Id).Contains(x.Id)).ToListAsync();
        //    dynamic property = new ExpandoObject();
        //    property.UserStatusType = UserStatusType.Deleted;
        //    _repository.Update(user, (ExpandoObject)property);
        //    await _repository.RemoveRange<Announcement>(announcements.Select(y => y.Id).ToList());
        //    await _repository.RemoveRange<DeviceToken>(deviceTokens);
        //    await _repository.RemoveRange<Subscription>(subscriptions);
        //    await _repository.RemoveRange<UserSubscriptionAreaAndLanguage>(userSubscriptionAreaAndLanguages);
        //    await _repository.RemoveRange<Favourite>(favourites);
        //    await _repository.RemoveRange<SaveFilter>(saveFilters.Select(x => x.Id).ToList());
        //    //await _repository.RemoveRange<Conversation>(conversations);
        //    //await _repository.RemoveRange<SupportConversation>(supportConversations);
        //    //await _repository.RemoveRange<SupportConversation>(supportMessages);
        //    //await _repository.RemoveRange<Message>(messages);
        //    await _repository.Remove<SupportConversation>(user.Id);
        //    await _repository.Remove<Password>(password);
        //    await _repository.Remove<Verified>(verified);
        //    await _repository.Remove<User>(user.Id);
        //    await _repository.SaveChangesAsync();
        //    return true;
        //}

        public async Task<PagingResponseModel<UserViewModel>> UserFilter(UserFilterModel model, Language language, int userId)
        {
            var caller = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            IQueryable<User> query;
            switch (model.UserStatusType)
            {
                case UserStatusType.Deleted:
                    query = _repository.FilterAsNoTracking<User>(x => x.UserStatusType == UserStatusType.Deleted).Distinct();
                    break;
                case UserStatusType.Inactive:
                    query = _repository.Filter<User>(x => x.UserStatusType == UserStatusType.Inactive).Distinct();
                    break;
                case UserStatusType.Active:
                    query = _repository.Filter<User>(x => x.UserStatusType == UserStatusType.Active).Distinct();
                    break;
                default:
                    query = _repository.GetAll<User>().Distinct();
                    break;
            }
            if (!string.IsNullOrEmpty(model.FullName))
            {
                var search = model.FullName.ToLower();
                var searchParams = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in searchParams)
                {
                    query = query.Where(x => x.FullName.ToLower().Contains(item));
                }
            }
            if (!string.IsNullOrEmpty(model.Phone))
            {
                var searchParams = model.Phone.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in searchParams)
                {
                    query = query.Where(x => (x.PhoneCode + x.Phone).Contains(item));
                }
            }
            if (!string.IsNullOrEmpty(model.Email))
            {
                var search = model.Email.ToLower();
                var searchParams = model.Email.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in searchParams)
                {
                    query = query.Where(x => x.Email.Contains(item));
                }
            }
            if (!string.IsNullOrEmpty(model.City))
            {
                var search = model.City.ToLower();
                var searchParams = model.City.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in searchParams)
                {
                    query = query.Where(x => x.City.Name.Contains(item));
                }
            }
            if (!string.IsNullOrEmpty(model.IpLocation))
            {
                var search = model.City.ToLower();
                var searchParams = model.City.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in searchParams)
                {
                    query = query.Where(x => x.UserLocation.Country.Contains(item));
                }
            }
            if (model.AnnouncementCount != null)
            {
                query = query.Where(x => x.Announcements.Count(y => !y.IsDeleted && !y.IsDraft) == model.AnnouncementCount);
            }
            var result = await query.OrderByDescending(x => x.CreatedDt).Skip((model.Page - 1) * model.Count).Distinct()
                .Take(model.Count).Select(x =>
                new UserViewModel
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    AnnouncementCount = x.Announcements.Count(y => !y.IsDeleted && !y.IsDraft),
                    Email = x.Email,
                    PhoneCode = x.PhoneCode,
                    Phone = x.Phone,
                    UserStatusType = x.UserStatusType,
                    IsBlocked = x.IsBlocked,
                    CityId = x.CityId ?? 0,
                    City = x.City.Name,
                    IpLocation = x.UserLocation.UserId == x.Id && !x.UserLocation.IsDeleted ? x.UserLocation.Country : null,
                    ProfilePhoto = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                                UploadType.ProfilePhoto, x.ProfilePhoto, ConstValues.Width, ConstValues.Height, false, 0)
                    },
                    CreatedDate = x.CreatedDt
                }).ToListAsync();
            var count = await query.CountAsync();
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(count, model.Count)));
            return new PagingResponseModel<UserViewModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? result.FirstOrDefault()?.CreatedDate : model.DateFrom,
                ItemCount = count,
                PageCount = page
            };
        }

        public async Task<bool> ResetPassword(UserResetPasswordModel model, int userId, Language language, int callerId)
        {
            var admin = await _repository.Filter<User>(u => u.Id == callerId).FirstOrDefaultAsync();
            if (admin == null)
                throw new Exception(_optionsBinder.Error().MakeFilter);
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            Verified verified = null;
            if (Regex.Replace(model.VerificationTerm, @"\s+", "").All(c => Char.IsDigit(c)))
            {
                verified = await _repository.Filter<Verified>(v => v.PhoneCode == user.PhoneCode && v.Phone == user.Phone
                    && v.VerifiedBy == VerifiedBy.Phone).FirstOrDefaultAsync();
                if (verified == null)
                    throw new Exception(_optionsBinder.Error().InvalidEmailorPhone);
                var checkEmail = await _repository.FilterAsNoTracking<User>(u => u.VerifiedBy == VerifiedBy.Phone
                    && u.PhoneCode == model.PhoneCode && u.Phone == model.VerificationTerm).FirstOrDefaultAsync();
                if (checkEmail != null && checkEmail.Id != user.Id)
                    throw new Exception("Phone already used");
                verified.VerifiedBy = VerifiedBy.Phone;
                verified.Email = null;
                verified.PhoneCode = model.PhoneCode;
                verified.Phone = model.VerificationTerm;

                user.PhoneCode = model.PhoneCode;
                user.Phone = model.VerificationTerm;
                user.VerifiedBy = VerifiedBy.Phone;
            }
            else
            {
                verified = await _repository.Filter<Verified>(v => v.Email == user.Email && v.VerifiedBy == VerifiedBy.Email).FirstOrDefaultAsync();
                if (verified == null)
                    throw new Exception(_optionsBinder.Error().InvalidEmailorPhone);
                var checkEmail = await _repository.FilterAsNoTracking<User>(u => u.VerifiedBy == VerifiedBy.Email
                    && u.Email == model.VerificationTerm && u.Id == user.Id).FirstOrDefaultAsync();
                if (checkEmail != null && checkEmail.Id != user.Id)
                    throw new Exception("Email already used");
                verified.VerifiedBy = VerifiedBy.Email;
                verified.Email = model.VerificationTerm;
                verified.PhoneCode = null;
                verified.Phone = null;

                user.Email = model.VerificationTerm;
                user.VerifiedBy = VerifiedBy.Email;
            }

            _repository.Update(verified);
            _repository.Update(user);
            var passwords = await _repository.Filter<Password>(p => p.UserId == user.Id).ToListAsync();
            var newPassword = Utilities.KeyGenerator(8);
            _repository.Create(new Password
            {
                UserId = user.Id,
                LoginProvider = SocialLoginProvider.Local,
                PasswordHash = Utilities.HashPassword(newPassword)
            });
            _repository.HardDeleteRange(passwords);

            var text = $"{_optionsBinder.Error().NewPassword} {newPassword}";
            var bodyText = $"{_optionsBinder.Error().NewPassword}";

            if (model.VerificationTerm.Contains("@"))
                Utilities.SendEmail(model.VerificationTerm, "Account is reset", text);
            else
                Utilities.SendKeyByTwilio(model.PhoneCode + model.VerificationTerm, newPassword, bodyText);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<PagingResponseModel<PersonNotificationModel>> NotificationList(PagingRequestModel model, int userId, Language language)
        {
            var user = await _repository.FilterAsNoTracking<User>(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception(_optionsBinder.Error().UserNotFound);
            var personNotifications = _repository.FilterAsNoTracking<PersonNotification>(x => x.UserId == user.Id);
            var page = Convert.ToInt32(Math.Ceiling(decimal.Divide(personNotifications.Count(), model.Count)));
            var result = await personNotifications.OrderByDescending(x => x.CreatedDt)
                .Skip((model.Page - 1) * model.Count).Take(model.Count)
                .Select(x => new PersonNotificationModel
                {
                    Title = language == Language.English ? x.Notification.Title ?? x.PushNotification.Title
                            : x.Notification.NotificationTranslate.Select(n => n.Title).FirstOrDefault()
                            ?? x.PushNotification.PushNotificationTranslates.Select(n => n.Title).FirstOrDefault(),
                    Description = language == Language.English ? x.Notification.Description ?? x.PushNotification.Description
                            : x.Notification.NotificationTranslate.Select(n => n.Description).FirstOrDefault()
                            ?? x.PushNotification.PushNotificationTranslates.Select(n => n.Description).FirstOrDefault(),
                    AnnouncementId = x.AnnouncementId,
                    NotificationSendDate = x.CreatedDt,
                    Photo = new ImageOptimizer
                    {
                        Photo = Utilities.ReturnFilePath(ConstValues.MediaBaseUrl, ConstValues.MediaResize,
                        UploadType.AnnouncementBasePhoto, x.Announcement.BasePhoto, 300, 300, false, 0)
                    },
                }).ToListAsync();
            var unSeen = await _repository.Filter<PersonNotification>(x => !x.IsSeen && x.UserId == user.Id).ToListAsync();
            foreach (var not in unSeen)
            {
                not.IsSeen = true;
                _repository.Update(not);
            }
            await _repository.SaveChangesAsync();
            return new PagingResponseModel<PersonNotificationModel>
            {
                Data = result,
                DateFrom = model.Count == 1 ? personNotifications.FirstOrDefault()?.CreatedDt : model.DateFrom,
                ItemCount = personNotifications.Count(),
                PageCount = page
            };
        }

        public async Task<UserCurrencyResponseModel> GetUserCurrency(int userId)
        {
            User user = await _repository.Filter<User>(u => u.Id == userId).FirstOrDefaultAsync();
            Currency currency = await _repository.Filter<Currency>(c => c.Id == user.CurrencyId).FirstOrDefaultAsync();
            return new UserCurrencyResponseModel
            {
                Symbol = currency.Symbol,
                Code = currency.Code,
                CurrencyId = currency.Id
            };
        }

        public User CheckExistUser(int userId)
        {
            return _repository.FilterAsNoTracking<User>(u => u.Id == userId && !u.IsBlocked).FirstOrDefault();
        }

        #region
        private async Task<List<UserActivityModel>> UserActivityCounter(int userId)
        {
            var list = new List<UserActivityModel>();
            for (int i = 6; i >= 0; i--)
            {
                var activity = await _repository.Filter<Statistic>(x => x.UserId == userId
                    && x.ActivityDate.Date == DateTime.Today.AddDays(-1 * i)).ToListAsync();
                var count = activity.Count;
                var model = new UserActivityModel
                {
                    Day = DateTime.Today.AddDays(-1 * i),
                    Duration = 0.0
                };
                var dur = 0.0;
                for (int j = 0; j < count; j++)
                {
                    var first = activity[j];
                    if (j == count - 1)
                    {
                        dur += 5;
                        model.Duration = dur;
                        break;
                    }
                    else
                    {
                        var second = activity[j + 1];
                        var diff = (second.ActivityDate - first.ActivityDate).TotalMinutes;
                        if (diff < 10)
                            dur += diff;
                    }
                }
                list.Add(model);
            }
            return list;
        }
        #endregion UserActivityCounter
    }
}