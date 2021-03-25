using Baitkm.DAL.Context;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DAL.Services;
using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.PushNotifications;
using Baitkm.Entities;
using Baitkm.Enums.Notifications;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Validation.Attributes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Baitkm.DAL.Repository.Firebase
{
    public class FirebaseRepository : EntityRepository, IFirebaseRepository
    {
        public FirebaseRepository(BaitkmDbContext context) : base(context)
        {
        }

        public async Task<bool> SendIndividualNotification<TModel>(TModel model, bool isGuest) where TModel : IIndividualNotificationBase
        {
            List<DeviceTokenHelper> deviceToken;
            if (!isGuest)
                deviceToken = await Filter<DeviceToken>(x => x.UserId == model.ReceiverId).Select(x => new DeviceTokenHelper
                {
                    OsType = x.OsType,
                    Token = x.Token
                }).ToListAsync();
            else
                deviceToken = await Filter<Guest>(x => x.Id == model.ReceiverId).Select(x => new DeviceTokenHelper
                {
                    OsType = x.OsType,
                    Token = x.Token
                }).ToListAsync();
            if (deviceToken.Count == 0)
                return true;
            deviceToken = deviceToken.GroupBy(x => x.Token).Select(x => x.First()).ToList();
            foreach (var item in deviceToken)
            {
                if (item.Token == null)
                    continue;
                dynamic objNotification;
                if (item.OsType == OsType.Android)
                {
                    objNotification = new
                    {
                        to = item.Token,
                        data = new
                        {
                            title = model.Title,
                            description = model.Description,
                            notificationType = model.NotificationType,
                            personId = model.SenderId,
                            genericId = model.GenericId,
                            userId = model.ReceiverId,
                            unreadConversationCount = model.UnreadConversationCount,
                            fromAdmin = model.FromAdmin
                        }
                    };
                }
                else
                {
                    objNotification = new
                    {
                        to = item.Token,
                        notification = new
                        {
                            title = model.Title,
                            body = model.Description,
                            badge = 1,
                            sound = "default"
                        },
                        data = new
                        {
                            message = new
                            {
                                title = model.Title,
                                body = model.Description,
                                notificationType = model.NotificationType,
                                senderId = model.SenderId,
                                genericId = model.GenericId,
                                userId = model.ReceiverId,
                                unreadConversationCount = model.UnreadConversationCount,
                                fromAdmin = model.FromAdmin
                            }
                        }
                    };
                }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(() => CreatePostRequest(objNotification), TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            return true;
        }

        public async Task<bool> SendGroupNotification<TModel>(TModel model) where TModel : IGroupNotificationBase
        {
            var (androidList, iosList) = await GetSortedDeviceTokens(model.UserIds);
            SendAndroidGroupNotification(androidList, model);
            SendIosGroupNotification(iosList, model);
            return true;
        }

        public async Task<bool> SendCampaignNotification<TModel>(TModel model) where TModel : IGroupNotificationBase
        {
            var (androidList, iosList) = await GetSortedDeviceTokens(model.UserIds);
            var (guestAndroid, guestIos) = await GetSortedGuestDeviceTokens(model.GuestIds);
            SendAndroidGroup(androidList, model);
            SendIosGroup(iosList, model);
            SendAndroidGuestGroup(guestAndroid, model);
            SendIosGuestGroup(guestIos, model);
            return true;
        }

        public void SendAndroidGroupNotification<TModel>(List<List<DeviceToken>> androidList, TModel model) where TModel : IGroupNotificationBase
        {
            if (androidList.Count == 0)
                return;
            foreach (var variable in androidList)
            {
                var newList = variable.GroupBy(x => x.DeviceId).Select(x => x.First()).ToList();
                foreach (var deviceToken in newList)
                {
                    IDictionary<string, object> property = new ExpandoObject();
                    property.Add("to", deviceToken.Token);
                    var props = model.GetType().GetProperties().Where(x =>
                            x.CustomAttributes.Count(s => s.AttributeType == typeof(PropertyNotMappedAttribute)) == 0);
                    IDictionary<string, object> data = new ExpandoObject();
                    foreach (var prop in props)
                    {
                        data.Add(prop.Name, prop.GetValue(model));
                    }
                    data.Add("userId", deviceToken.UserId);
                    property.Add("data", data);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Factory.StartNew(() => CreatePostRequest(property), TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        public void SendIosGroupNotification<TModel>(List<List<DeviceToken>> iosList, TModel model) where TModel : IGroupNotificationBase
        {
            if (iosList.Count == 0)
                return;
            foreach (var variable in iosList)
            {
                var newList = variable.GroupBy(x => x.DeviceId).Select(x => x.First()).ToList();
                foreach (var deviceToken in newList)
                {
                    IDictionary<string, object> property = new ExpandoObject();
                    property.Add("to", deviceToken.Token);
                    var props = model.GetType().GetProperties().Where(x =>
                        x.CustomAttributes.Count(s => s.AttributeType == typeof(PropertyNotMappedAttribute)) == 0);
                    IDictionary<string, object> message = new ExpandoObject();
                    foreach (var prop in props)
                    {
                        message.Add(prop.Name, prop.GetValue(model));
                    }
                    message.Add("userId", deviceToken.UserId);
                    property.Add("data", new { message = new { message } });
                    var notification = new
                    {
                        title = model.Title,
                        body = model.Description,
                        badge = 1,
                        sound = "default"
                    };
                    property.Add("notification", notification);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Factory.StartNew(() => CreatePostRequest(property), TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        #region Private

        private async Task<(List<List<DeviceToken>> AndroidList, List<List<DeviceToken>> IosList)> GetSortedDeviceTokens(IEnumerable<int> personIds)
        {
            var persons = await FilterAsNoTracking<User>(x => !x.IsDeleted && personIds.Contains(x.Id)).ToListAsync();
            if (persons.Count == 0)
                return (new List<List<DeviceToken>>(), new List<List<DeviceToken>>());
            var iosDevToken = new List<DeviceToken>();
            var androidDevToken = new List<DeviceToken>();
            foreach (var variable in persons)
            {
                var personDevToken = await FilterAsNoTracking<DeviceToken>(x => x.UserId == variable.Id).ToListAsync();
                if (personDevToken.Count == 0)
                    continue;
                foreach (var item in personDevToken)
                {
                    if (item.Token == null)
                        continue;
                    if (item.OsType == OsType.Android)
                        androidDevToken.Add(item);
                    else
                        iosDevToken.Add(item);
                }
            }
            return (androidDevToken.SplitList(1000), iosDevToken.SplitList(1000));
        }

        private async Task<(List<List<Guest>> AndroidList, List<List<Guest>> IosList)> GetSortedGuestDeviceTokens(IEnumerable<int> personIds)
        {
            var persons = await FilterAsNoTracking<Guest>(x => !x.IsDeleted && personIds.Contains(x.Id)).ToListAsync();
            if (persons.Count == 0)
                return (new List<List<Guest>>(), new List<List<Guest>>());
            var iosDevToken = new List<Guest>();
            var androidDevToken = new List<Guest>();
            foreach (var variable in persons)
            {
                if (variable.Token == null)
                    continue;
                if (variable.OsType == OsType.Android)
                    androidDevToken.Add(variable);
                else
                    iosDevToken.Add(variable);
            }
            return (androidDevToken.SplitList(1000), iosDevToken.SplitList(1000));
        }

        private async Task CreatePostRequest(object notification)
        {
            using (var client = new HttpClient())
            {
                var json = Utilities.SerializeObject(notification).Replace("\\", string.Empty);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={ConstValues.FirebaseServerKey}");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sender", $"id={ConstValues.FirebaseSenderId}");
                var t = await client.PostAsync(ConstValues.FirebaseUrl, content);
                var smth = t.Content.ReadAsStringAsync();
            }
        }

        public void SendAndroidGroup<TModel>(List<List<DeviceToken>> androidList, TModel model) where TModel : INotificationBase
        {
            if (androidList.Count == 0) return;
            foreach (var variable in androidList)
            {
                var devTokenList = variable.GroupBy(x => x.Token).Select(x => x.First()).ToList();
                IDictionary<string, object> property = new ExpandoObject();
                property.Add("registration_ids", devTokenList.Select(x => x.Token).ToList());
                var props = model.GetType().GetProperties().Where(x =>
                    x.CustomAttributes.Count(s => s.AttributeType == typeof(PropertyNotMappedAttribute)) == 0);
                IDictionary<string, object> data = new ExpandoObject();
                foreach (var prop in props)
                {
                    data.Add(prop.Name, prop.GetValue(model));
                }
                property.Add("data", data);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(() => CreatePostRequest(property), TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public void SendIosGroup<TModel>(List<List<DeviceToken>> iosList, TModel model) where TModel : INotificationBase
        {
            if (iosList.Count == 0) return;
            foreach (var variable in iosList)
            {
                var devTokenList = variable.GroupBy(x => x.Token).Select(x => x.First()).ToList();
                IDictionary<string, object> property = new ExpandoObject();
                property.Add("registration_ids", devTokenList.Select(x => x.Token).ToList());
                var props = model.GetType().GetProperties().Where(x =>
                    x.CustomAttributes.Count(s => s.AttributeType == typeof(PropertyNotMappedAttribute)) == 0);
                IDictionary<string, object> message = new ExpandoObject();
                foreach (var prop in props)
                {
                    message.Add(prop.Name, prop.GetValue(model));
                }
                property.Add("data", new { message = new { message } });
                var notification = new
                {
                    title = model.Title,
                    body = model.Description,
                    badge = 1,
                    sound = "default"
                };
                property.Add("notification", notification);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(() => CreatePostRequest(property), TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public void SendAndroidGuestGroup<TModel>(List<List<Guest>> androidList, TModel model) where TModel : INotificationBase
        {
            if (androidList.Count == 0) return;
            foreach (var variable in androidList)
            {
                var devTokenList = variable.GroupBy(x => x.Token).Select(x => x.First()).ToList();
                IDictionary<string, object> property = new ExpandoObject();
                property.Add("registration_ids", devTokenList.Select(x => x.Token).ToList());
                var props = model.GetType().GetProperties().Where(x =>
                    x.CustomAttributes.Count(s => s.AttributeType == typeof(PropertyNotMappedAttribute)) == 0);
                IDictionary<string, object> data = new ExpandoObject();
                foreach (var prop in props)
                {
                    data.Add(prop.Name, prop.GetValue(model));
                }
                property.Add("data", data);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(() => CreatePostRequest(property), TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public void SendIosGuestGroup<TModel>(List<List<Guest>> iosList, TModel model) where TModel : INotificationBase
        {
            if (iosList.Count == 0) return;
            foreach (var variable in iosList)
            {
                var devTokenList = variable.GroupBy(x => x.Token).Select(x => x.First()).ToList();
                IDictionary<string, object> property = new ExpandoObject();
                property.Add("registration_ids", devTokenList.Select(x => x.Token).ToList());
                var props = model.GetType().GetProperties().Where(x =>
                    x.CustomAttributes.Count(s => s.AttributeType == typeof(PropertyNotMappedAttribute)) == 0);
                IDictionary<string, object> message = new ExpandoObject();
                foreach (var prop in props)
                {
                    message.Add(prop.Name, prop.GetValue(model));
                }
                property.Add("data", new { message = new { message } });
                var notification = new
                {
                    title = model.Title,
                    body = model.Description,
                    badge = 1,
                    sound = "default"
                };
                property.Add("notification", notification);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(() => CreatePostRequest(property), TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        #endregion
    }
}
