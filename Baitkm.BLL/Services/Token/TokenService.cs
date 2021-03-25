using Baitkm.BLL.Services.Configurations;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Persons.Authentication;
using Baitkm.DTO.ViewModels.Persons.Users.Verification;
using Baitkm.DTO.ViewModels.Social;
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
using Baitkm.Infrastructure.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly IEntityRepository _repository;
        private readonly TokenProviderOptions _options;
        private readonly MediaAccessor _mediaAccessor;
        private readonly IOptionsBinder _optionsBinder;
        private readonly IConfigurationService configurationService;

        public TokenService(IEntityRepository repository,
            IOptions<TokenProviderOptions> options,
            IOptions<AppSettings> settings,
            IHostingEnvironment environment,
            IOptionsBinder optionsBinder,
            IConfigurationService configurationService)
        {
            _repository = repository;
            _options = options.Value;
            var appSettings = settings.Value;
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.SecretKey));
            _options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            _options.Audience = "BaitkmReactJsWebApp";
            _options.Issuer = "Baitkm" + environment.EnvironmentName;
            _mediaAccessor = new MediaAccessor();
            _optionsBinder = optionsBinder;
            this.configurationService = configurationService;
        }

        public async Task<bool> SendKeyAsync(SendKeyModel model, Language language)
        {
            var key = Utilities.KeyGenerator(4);
            string bodyText;
            string subject;
            if (language == Language.English)
            {
                bodyText = "Your verification key is";
                subject = "Baitkm Registration code";
            }
            else
            {
                subject = "رمز تسجيل منزلك";
                bodyText = "مفتاح التحقق الخاص بك هو";
            }
            var user = await _repository.FilterAsNoTracking<User>(u => (u.Email == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Email)
                 || (u.PhoneCode == model.PhoneCode && u.Phone == model.VerificationTerm && u.VerifiedBy == VerifiedBy.Phone)).FirstOrDefaultAsync();
            if (user != null)
                throw new Exception(_optionsBinder.Error().AccountExist);
            var verified = await _repository.Filter<Verified>(v => v.Email == model.VerificationTerm && v.VerifiedBy == VerifiedBy.Email
                || (v.PhoneCode == model.PhoneCode && v.Phone == model.VerificationTerm && v.VerifiedBy == VerifiedBy.Phone)).FirstOrDefaultAsync();
            if (verified != null)
                if (verified.IsRegistered)
                    throw new Exception(_optionsBinder.Error().AccountExist);
            if (Regex.Replace(model.VerificationTerm, @"\s+", "").All(c => Char.IsDigit(c)))
            {
                if (verified != null)
                {
                    verified.Code = key;
                    _repository.Update(verified);
                }
                else
                {
                    _repository.Create(new Verified
                    {
                        PhoneCode = model.PhoneCode,
                        Phone = model.VerificationTerm,
                        Code = key,
                        VerifiedBy = VerifiedBy.Phone
                    });
                }
                Utilities.SendKeyByTwilio(model.PhoneCode + model.VerificationTerm, key, bodyText);
            }
            else
            {
                if (verified != null)
                {
                    verified.Code = key;
                    _repository.Update(verified);
                }
                else
                {
                    _repository.Create(new Verified
                    {
                        Email = model.VerificationTerm,
                        Code = key,
                        VerifiedBy = VerifiedBy.Email
                    });
                }
                Utilities.SendKeyByEmail(model.VerificationTerm, key, subject, bodyText);
            }
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyAsync(TokenVerifyViewModel model, string deviceToken, OsType osType, Language language, string deviceId)
        {
            Verified verified;
            if (Regex.Replace(model.VerificationTerm, @"\s+", "").All(c => Char.IsDigit(c)))
                verified = await _repository.Filter<Verified>(v => v.PhoneCode == model.PhoneCode 
                    && v.Phone == model.VerificationTerm && v.VerifiedBy == VerifiedBy.Phone).FirstOrDefaultAsync();
            else
                verified = await _repository.Filter<Verified>(v => 
                    v.Email == model.VerificationTerm && v.VerifiedBy == VerifiedBy.Email).FirstOrDefaultAsync();
            if (verified == null || verified.Code != model.Code)
                throw new Exception(_optionsBinder.Error().VerificationCode);
            verified.IsVerified = true;
            verified.VerifiedType = VerifiedType.Verify;
            _repository.Update(verified);
            await _repository.SaveChangesAsync();
            return true;
        }

        public bool TokenTest()
        {
            //return true; // cherevalu hamar
            return false; // erevalu hamar
        }

        public string ReturnString() => configurationService.GetNgrokSettings().NgrokUrl;

        public async Task<TokenResponse> TokenAsync(TokenViewModel model, string deviceToken, OsType osType,
            Language language, string deviceId)
        {
            User user;
            if (Regex.Replace(model.VerificationTerm, @"\s+", "").Substring(1).All(c => Char.IsDigit(c)))
                user = await _repository.Filter<User>(u => (u.PhoneCode + u.Phone) == model.VerificationTerm
                    && u.VerifiedBy == VerifiedBy.Phone).FirstOrDefaultAsync();
            else
                user = await _repository.Filter<User>(u => u.Email == model.VerificationTerm
                    && u.VerifiedBy == VerifiedBy.Email).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception(_optionsBinder.Error().Incorrect);
            if (user.IsBlocked)
                throw new Exception($"{_optionsBinder.Error().UnLockDate} {user.UnBlockDate.ToShortDateString()}");
            if (user.IsDeleted)
                throw new Exception(_optionsBinder.Error().AccountWasDeleted);

            var password = await _repository.FilterAsNoTracking<Password>(p => p.UserId == user.Id &&
                p.LoginProvider == SocialLoginProvider.Local).FirstOrDefaultAsync();
            var isSame = Utilities.VerifyHashedPassword(password.PasswordHash, model.Password);
            if (!isSame)
                throw new Exception(_optionsBinder.Error().Incorrect);
            var token = await Token(new SocialTokenViewModel
            {
                DeviceToken = deviceToken,
                DeviceId = deviceId,
                OsType = osType,
                Password = password.PasswordHash,
                Provider = password.LoginProvider,
                Email = user.Email,
                Id = user.Id
            });
            await _repository.SaveChangesAsync();
            return token;
        }

        public async Task<TokenResponse> SocialLogin(SocialLoginModel model,
            string deviceToken, Language language, OsType osType, string deviceId, string currencyCode)
        {
            switch (model.Provider)
            {
                case SocialLoginProvider.Facebook:
                    return await ExternalLogin(await FaceBookRequest(model.Token), deviceToken, osType, deviceId, currencyCode);
                case SocialLoginProvider.Google:
                    return await ExternalLogin(await GoogleRequest(model.Token), deviceToken, osType, deviceId, currencyCode);
                default:
                    throw new SmartException();
            }
        }

        public async Task<TokenResponse> AppleLogin(AppleSignInRequestModel model,
            string email, string appleId, string deviceToken, Language language, string deviceId, string currencyCode)
        {
            User user = new User();
            Password password;
            string generatedPassword = email.ToCharArray().Reverse().ToString();
            var firstCasePerson = await _repository
                .Filter<User>(x => x.Email == email && x.VerifiedBy == VerifiedBy.Email).FirstOrDefaultAsync();
            var secondCasePerson = await _repository.Filter<User>(x =>
                x.Email == email && x.VerifiedBy == VerifiedBy.Email).Include(p => p.Passwords).FirstOrDefaultAsync();
            if (firstCasePerson != null && firstCasePerson.IsBlocked || secondCasePerson != null && secondCasePerson.IsBlocked)
                throw new Exception(_optionsBinder.Error().BlockedAccount);
            int currencyId = 1;
            Currency currency = _repository.Filter<Currency>(c => c.Code == currencyCode && !c.IsDeleted).FirstOrDefault();
            if (currency != null)
                currencyId = currency.Id;
            if (firstCasePerson == null)
            {
                generatedPassword = email.ToCharArray().Reverse().ToString();
                user = new User
                {
                    Email = email,
                    FullName = model.FullName,
                    ProfilePhoto = null,
                    RoleEnum = Role.User,
                    VerifiedBy = VerifiedBy.Email,
                    OsType = OsType.Ios,
                    UserStatusType = UserStatusType.Active,
                    DateOfBirth = null,
                    IsLocal = false,
                    CurrencyId = currencyId
                };
                _repository.Create(user);
                password = new Password
                {
                    UserId = user.Id,
                    LoginProvider = SocialLoginProvider.Apple,
                    UniqueIdentifier = appleId,
                    PasswordHash = Utilities.HashPassword(generatedPassword)
                };
                _repository.Create(password);
                var verified = await _repository.Filter<Verified>(x => x.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
                if (verified == null)
                {
                    verified = new Verified
                    {
                        Code = "0000",
                        Email = email,
                        IsRegistered = true,
                        VerifiedType = VerifiedType.Verify,
                        VerifiedBy = VerifiedBy.Email,
                        IsVerified = true
                    };
                }
                _repository.Create(verified);
            }
            else if (secondCasePerson != null)
            {
                user = secondCasePerson;
            }
            if ((firstCasePerson == null) && (secondCasePerson == null))
            {
                foreach (SubscriptionsType variable in Enum.GetValues(typeof(SubscriptionsType)))
                {
                    _repository.Create(new PersonSetting
                    {
                        UserId = user.Id,
                        SubscriptionsType = variable
                    });
                }
                _repository.Create(new PersonOtherSetting
                {
                    UserId = user.Id,
                    AreaUnit = AreaUnit.SquareMeter,
                    Language = Language.English
                });
            }
            await _repository.SaveChangesAsync();
            return await Token(new SocialTokenViewModel
            {
                DeviceToken = deviceToken,
                DeviceId = deviceId,
                OsType = OsType.Ios,
                Password = generatedPassword,
                Provider = SocialLoginProvider.Apple,
                SocialId = appleId,
                Email = user.Email,
                Id = user.Id
            });
        }

        #region Private

        private async Task<FacebookResponse> FaceBookRequest(string fbToken)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{ConstValues.FacebookGraphBaseUrl}{ConstValues.FacebookPersonInfoUrl}{fbToken}");
                var parsed = await response.Content.ReadAsStringAsync();
                var token = JToken.Parse(parsed);
                var fbResponse = JsonConvert.DeserializeObject<FacebookResponse>(parsed);
                fbResponse.Url = (string)token.SelectToken("picture.data.url");
                if (fbResponse.Email == null)
                    throw new Exception(_optionsBinder.Error().EmptyEmail);
                fbResponse.Email = fbResponse.Email.ToLower();
                return fbResponse;
            }
        }

        private async Task<GoogleResponse> GoogleRequest(string googleToken)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{ConstValues.GoogleSocialLoginUrl}{googleToken}");
                var parsed = await response.Content.ReadAsStringAsync();
                var googleResponse = JsonConvert.DeserializeObject<GoogleResponse>(parsed);
                googleResponse.Email = googleResponse.Email.ToLower();
                return googleResponse;
            }
        }

        private async Task<TokenResponse> ExternalLogin(FacebookResponse fbResult, string deviceToken,
            OsType osType, string deviceId, string currencyCode)
        {
            User user = new User();
            Password password;
            string generatedPassword = fbResult.Id.ToCharArray().Reverse().ToString();
            var firstCasePerson = await _repository
                .Filter<User>(x => x.Email == fbResult.Email && x.VerifiedBy == VerifiedBy.Email).FirstOrDefaultAsync();
            var secondCasePerson = await _repository.Filter<User>(x =>
                x.Email == fbResult.Email && x.VerifiedBy == VerifiedBy.Email).Include(p => p.Passwords).FirstOrDefaultAsync();
            if (firstCasePerson != null && firstCasePerson.IsBlocked || secondCasePerson != null && secondCasePerson.IsBlocked)
                throw new Exception(_optionsBinder.Error().BlockedAccount);
            int currencyId = 1;
            Currency currency = _repository.FilterAsNoTracking<Currency>(c => c.Code == currencyCode).FirstOrDefault();
            if (currency != null)
                currencyId = currency.Id;

            if (firstCasePerson == null)
            {
                var photo = await _mediaAccessor.GetPhotoFromSocial(fbResult.Url);
                generatedPassword = fbResult.Id.ToCharArray().Reverse().ToString();
                user = new User
                {
                    Email = fbResult.Email,
                    FullName = $"{fbResult.First_name} {fbResult.Last_name}",
                    ProfilePhoto = photo,
                    RoleEnum = Role.User,
                    VerifiedBy = VerifiedBy.Email,
                    OsType = osType,
                    UserStatusType = UserStatusType.Active,
                    DateOfBirth = null,
                    IsLocal = false,
                    CurrencyId = currencyId
                };
                _repository.Create(user);
                password = new Password
                {
                    UserId = user.Id,
                    LoginProvider = SocialLoginProvider.Facebook,
                    UniqueIdentifier = fbResult.Id,
                    PasswordHash = Utilities.HashPassword(generatedPassword)
                };
                _repository.Create(password);
                var verified = await _repository.Filter<Verified>(x => x.Email.ToLower() == fbResult.Email.ToLower()).FirstOrDefaultAsync();
                if (verified == null)
                {
                    verified = new Verified
                    {
                        Code = null,
                        Email = fbResult.Email,
                        IsRegistered = true,
                        VerifiedType = VerifiedType.Verify,
                        VerifiedBy = VerifiedBy.Email,
                        IsVerified = true
                    };
                }
                _repository.Create(verified);
            }
            else if (secondCasePerson != null)
            {
                user = secondCasePerson;
            }
            if ((firstCasePerson == null) && (secondCasePerson == null))
            {
                foreach (SubscriptionsType variable in Enum.GetValues(typeof(SubscriptionsType)))
                {
                    _repository.Create(new PersonSetting
                    {
                        UserId = user.Id,
                        SubscriptionsType = variable
                    });
                }
                _repository.Create(new PersonOtherSetting
                {
                    UserId = user.Id,
                    AreaUnit = AreaUnit.SquareMeter,
                    Language = Language.English
                });
            }
            await _repository.SaveChangesAsync();
            if (user.VerifiedBy == VerifiedBy.Email)
            {
                var html = Utilities.NewUserEmailStyle(user.Email);
                Utilities.SendEmail(user.Email, "Baitkm", html);
            }
            return await Token(new SocialTokenViewModel
            {
                DeviceToken = deviceToken,
                DeviceId = deviceId,
                OsType = osType,
                Password = generatedPassword,
                Provider = SocialLoginProvider.Facebook,
                SocialId = fbResult.Id,
                Email = user.Email,
                Id = user.Id
            });
        }

        private async Task<TokenResponse> ExternalLogin(GoogleResponse googleResult, string deviceToken,
            OsType osType, string deviceId, string currencyCode)
        {
            User user = new User();
            Password password;
            var generatedPassword = googleResult.Sub.ToCharArray().Reverse().ToString();
            var firstCasePerson = await _repository
                .Filter<User>(x => x.Email == googleResult.Email && x.VerifiedBy == VerifiedBy.Email).FirstOrDefaultAsync();
            var secondCasePerson = await _repository.Filter<User>(x =>
                x.Email == googleResult.Email && x.VerifiedBy == VerifiedBy.Email).Include(p => p.Passwords).FirstOrDefaultAsync();
            if (firstCasePerson != null && firstCasePerson.IsBlocked || secondCasePerson != null && secondCasePerson.IsBlocked)
                throw new Exception(_optionsBinder.Error().BlockedAccount);
            int currencyId = 1;
            Currency currency = _repository.Filter<Currency>(c => c.Code == currencyCode && !c.IsDeleted).FirstOrDefault();
            if (currency != null)
                currencyId = currency.Id;
            if (firstCasePerson == null)
            {
                var photo = await _mediaAccessor.GetPhotoFromSocial(googleResult.Picture);
                user = new User
                {
                    Email = googleResult.Email,
                    FullName = $"{googleResult.Given_name} {googleResult.Family_name}",
                    ProfilePhoto = photo,
                    RoleEnum = Role.User,
                    OsType = osType,
                    UserStatusType = UserStatusType.Active,
                    VerifiedBy = VerifiedBy.Email,
                    DateOfBirth = null,
                    IsLocal = false,
                    CurrencyId = currencyId
                };
                _repository.Create(user);
                password = new Password
                {
                    UserId = user.Id,
                    LoginProvider = SocialLoginProvider.Google,
                    UniqueIdentifier = googleResult.Sub,
                    PasswordHash = Utilities.HashPassword(generatedPassword)
                };
                _repository.Create(password);
                var verified = await _repository.Filter<Verified>(x => x.Email.ToLower() == googleResult.Email.ToLower()).FirstOrDefaultAsync();
                if (verified == null)
                {
                    verified = new Verified
                    {
                        Code = "0000",
                        Email = googleResult.Email,
                        IsRegistered = true,
                        VerifiedType = VerifiedType.Verify,
                        VerifiedBy = VerifiedBy.Email,
                        IsVerified = true
                    };
                    _repository.Create(verified);
                }
            }
            else if (secondCasePerson != null)
            {
                user = secondCasePerson;
            }
            if (firstCasePerson == null && secondCasePerson == null)
            {
                foreach (SubscriptionsType variable in Enum.GetValues(typeof(SubscriptionsType)))
                {
                    _repository.Create(new PersonSetting
                    {
                        UserId = user.Id,
                        SubscriptionsType = variable
                    });
                }
                _repository.Create(new PersonOtherSetting
                {
                    UserId = user.Id,
                    AreaUnit = AreaUnit.SquareMeter,
                    Language = Language.English
                });
            }
            await _repository.SaveChangesAsync();
            if (user.VerifiedBy == VerifiedBy.Email)
            {
                var html = Utilities.NewUserEmailStyle(user.Email);
                Utilities.SendEmail(user.Email, "Baitkm", html);
            }
            return await Token(new SocialTokenViewModel
            {
                DeviceToken = deviceToken,
                DeviceId = deviceId,
                OsType = osType,
                Password = generatedPassword,
                Provider = SocialLoginProvider.Google,
                SocialId = googleResult.Sub,
                Email = user.Email,
                Id = user.Id
            });
        }

        private async Task<TokenResponse> Token(SocialTokenViewModel model)
        {
            //var user = await _repository.Filter<User>(x => x.Email == model.Email && x.Id == model.Id).FirstOrDefaultAsync();
            var user = await _repository.Filter<User>(u => u.Id == model.Id).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception("user not found");
            var password = await _repository.Filter<Password>(x => x.UserId == user.Id).FirstOrDefaultAsync();
            if (password.LoginProvider != SocialLoginProvider.Local)
            {
                var isVerified = Utilities.VerifyHashedPassword(password.PasswordHash, model.Password);
                if (!isVerified)
                    throw new Exception(_optionsBinder.Error().Incorrect);
            }
            if (user.IsBlocked)
                throw new Exception(_optionsBinder.Error().BlockedAccount);
            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, model.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.Second.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Iat, now.Second.ToString(), ClaimValueTypes.Integer64),
                new Claim("userId", user.Id.ToString()),
                new Claim("roles", user.RoleEnum.ToString()),
                new Claim("ExpireDate", now.Add(_options.Expiration).ToString(CultureInfo.InvariantCulture)),
                new Claim("verifiedBy", user.VerifiedBy.ToString())
            };
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: _options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new TokenResponse
            {
                AccessToken = encodedJwt,
                DateTime = now.Add(_options.Expiration),
                Provider = model.Provider,
                Id = user.Id
            };
            if (string.IsNullOrEmpty(model.DeviceId))
                return response;
            var device = await _repository.Filter<DeviceToken>(dt => dt.DeviceId == model.DeviceId).FirstOrDefaultAsync();
            if (device == null)
            {
                _repository.Create(new DeviceToken
                {
                    DeviceId = model.DeviceId,
                    Token = model.DeviceToken,
                    OsType = model.OsType,
                    //Language = language,
                    //Currency = currency,
                    User = user
                });
            }
            else
            {
                device.Token = model.DeviceToken;
                device.OsType = model.OsType;
                //device.Language = language;
                //device.Currency = currency;
                device.UserId = device.UserId;
                _repository.Update(device);
            }
            //_repository.Create(new DeviceToken
            //{
            //    Token = model.DeviceToken,
            //    OsType = model.OsType,
            //    UserId = user.Id,
            //    DeviceId = model.DeviceId
            //});
            await _repository.SaveChangesAsync();
            return response;
        }
        #endregion
    }
}