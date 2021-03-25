using Baitkm.DTO.ViewModels;
using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Location;
using Baitkm.DTO.ViewModels.Slack;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.UserRelated;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.ResponseModels;
using Baitkm.Infrastructure.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Baitkm.Controllers
{
    //[Authorize]
    [Route("api/[controller]/[action]/")]
    [Authorize(Policy = "CheckUnauthorized")]
    public class BaseController : Controller
    {
        protected string UserCurrency
        {
            get
            {
                string ip = HttpContext.Connection.RemoteIpAddress.ToString();
                var result = new LocateModel();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ConstValues.IpAddressUrl);
                    var response = client.GetAsync(ip).Result;
                    var serialized = response.Content.ReadAsStringAsync().Result;
                    var token = JToken.Parse(serialized);
                    var resultJson = token.SelectToken("data").ToString();
                    result = JsonConvert.DeserializeObject<LocateModel>(resultJson);
                }
                return result.Currency;

            }
        }

        protected (decimal, decimal) GetRequesterLatLng()
        {
            string ip = HttpContext.Connection.RemoteIpAddress.ToString();
            var result = new LocateModel();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConstValues.IpAddressUrl);
                var response = client.GetAsync(ip).Result;
                var serialized = response.Content.ReadAsStringAsync().Result;
                var token = JToken.Parse(serialized);
                var resultJson = token.SelectToken("data").ToString();
                result = JsonConvert.DeserializeObject<LocateModel>(resultJson);
            }
            return (result.Lat, result.Lng);
        }

        protected decimal CurrencyRate
        {
            get
            {
                var tests = UserCurrency;
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync("https://itfllc.am/api/rate/currency-list").Result;
                    var test = JsonConvert.DeserializeObject<RateBaseResult>(response.Content.ReadAsStringAsync().Result);
                }

                return 0;
            }
        }

        protected string GetModelStateErrors()
        {
            return string.Join("; ", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));
        }

        protected void CreateErrorResult(ServiceResult serviceResult, Exception ex)
        {
            serviceResult.Success = false;
            serviceResult.Messages.AddMessage(MessageType.Error, ex.Message);
            serviceResult.Messages.AddMessage(MessageType.Error, ex.InnerException?.Message);
        }

        protected void CreateSuccessResult(ServiceResult serviceResult, object data, string message)
        {
            serviceResult.Success = true;
            serviceResult.Data = data;
            serviceResult.Messages.AddMessage(MessageType.Info, message);
        }

        protected string GetPerson()
        {
            var res = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return res;
        }

        protected int GetPersonId()
        {
            var personId = User.Claims.Where(c => c.Type == "userId").Select(c => c.Value).FirstOrDefault();
            return Convert.ToInt32(personId);
        }

        protected string GetDeviceToken()
        {
            Request.Headers.TryGetValue("DeviceToken", out var token);
            return token.FirstOrDefault();
        }

        protected OsType GetOsType()
        {
            Request.Headers.TryGetValue("OsType", out var os);
            Enum.TryParse(os.FirstOrDefault(), out OsType osType);
            return osType;
        }

        protected VerifiedBy GetVerifiedBy()
        {
            var verified = User.Claims.Where(c => c.Type == "verifiedBy").Select(c => c.Value).FirstOrDefault();
            Enum.TryParse(verified, out VerifiedBy verifiedBy);
            return verifiedBy;
        }

        protected string GetDeviceId()
        {
            Request.Headers.TryGetValue("DeviceId", out var deviceId);
            return deviceId.FirstOrDefault();
        }

        protected Language GetLanguage()
        {
            Request.Headers.TryGetValue("Language", out var lang);
            Enum.TryParse(lang.FirstOrDefault(), out Language language);
            return language;
        }

        protected int GetUserId()
        {
            int id = -1;
            var userId = User.Claims.Where(u => u.Type == "userId").Select(u => u.Value).FirstOrDefault();
            int.TryParse(userId, out id);
            if (id == -1) throw new Exception("Can not get current user id");
            return id;
        }

        protected async Task<IActionResult> MakeActionCallAsync<TResult>(Func<Task<TResult>> action)
        {
            var serviceResult = new ServiceResult();
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception(GetModelStateErrors());
                var result = await action();
                CreateSuccessResult(serviceResult, result, "OK");
            }
            catch (SmartException e)
            {
                CreateErrorResult(serviceResult, e);
            }
            catch (Exception e)
            {
                //await SendToSlackAsync(e);
                CreateErrorResult(serviceResult, e);
            }
            return Json(serviceResult);
        }

        protected IActionResult MakeActionCall<TResult>(Func<TResult> action)
        {
            var serviceResult = new ServiceResult();
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception(GetModelStateErrors());
                var result = action();
                CreateSuccessResult(serviceResult, result, "OK");
            }
            catch (SmartException e)
            {
                CreateErrorResult(serviceResult, e);
            }
            catch (Exception e)
            {
                //SendToSlack(e);
                CreateErrorResult(serviceResult, e);
            }
            return Json(serviceResult);
        }

        //private async Task SendToSlackAsync(Exception ex, IViewModel model = null)
        //{
        //    var controller = string.Empty;
        //    var action = string.Empty;
        //    var routeData = ControllerContext?.HttpContext.GetRouteData();
        //    if (routeData != null)
        //    {
        //        routeData.Values.TryGetValue("action", out var actionType);
        //        routeData.Values.TryGetValue("controller", out var controllerName);
        //        action = actionType?.ToString();
        //        controller = controllerName?.ToString();
        //    }
        //    var error = new SlackBaseModel
        //    {
        //        Channel = "#ellevate-bot",
        //        Username = "ellevateBot",
        //        Attachments = new List<SlackAttachmentModel> { new SlackAttachmentModel
        //            {
        //                Color = "#ff0000",
        //                Title = $"Error while calling {controller} ({DateTime.UtcNow})",
        //                Text = $"BackEnd Hosted In : {ConstValues.BackEndBaseUrl}, (Baitkm)\n" +
        //                       $" HttpMethod : {action},\n " +
        //                       $"Action name : {ControllerContext?.HttpContext?.Request?.Path},\n" +
        //                       $" User : {ControllerContext?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value},\n" +
        //                       $" Request Body : {JsonConvert.SerializeObject(model)},\n" +
        //                       $" Exception Message : {ex.Message},\n" +
        //                       $" Exception Stact Trace : {ex.StackTrace},\n" +
        //                       $" Inner Exception Message : {ex.InnerException?.Message},\n" +
        //                       $" Inner Exception Stack Trace : {ex.InnerException?.StackTrace},\n" +
        //                       $" Status Code Of Result : {HttpContext.Response.StatusCode}"
        //            }
        //        }
        //    };
        //    using (var client = new HttpClient())
        //    {
        //        var content = new StringContent(Utilities.SerializeObject(error), Encoding.UTF8, "application/json");
        //        await client.PostAsync(ConstValues.SlackUrl, content);
        //    }
        //}

        private void SendToSlack(Exception ex, IViewModel model = null)
        {
            var controller = string.Empty;
            var action = string.Empty;
            var routeData = ControllerContext?.HttpContext.GetRouteData();
            if (routeData != null)
            {
                routeData.Values.TryGetValue("action", out var actionType);
                routeData.Values.TryGetValue("controller", out var controllerName);
                action = actionType?.ToString();
                controller = controllerName?.ToString();
            }
            var error = new SlackBaseModel
            {
                Channel = "#ellevate-bot",
                Username = "ellevateBot",
                Attachments = new List<SlackAttachmentModel> {
                    new SlackAttachmentModel
                    {
                        Color = "#ff0000",
                        Title = $"Error while calling {controller} ({DateTime.UtcNow})",
                        Text = $"BackEnd Hosted In : {ConstValues.BackEndBaseUrl}, (Baitkm)\n" +
                               $"HttpMethod : {action},\n " +
                               $"Action name : {ControllerContext?.HttpContext?.Request?.Path},\n" +
                               $"User : {ControllerContext?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value},\n" +
                               $"Request Body : {JsonConvert.SerializeObject(model)},\n" +
                               $"Exception Message : {ex.Message},\n" +
                               $"Exception Stact Trace : {ex.StackTrace},\n" +
                               $"Inner Exception Message : {ex.InnerException?.Message},\n" +
                               $"Inner Exception Stack Trace : {ex.InnerException?.StackTrace},\n" +
                               $"Status Code Of Result : {HttpContext.Response.StatusCode}"
                    }
                }
            };
            using (var client = new HttpClient())
            {
                var content = new StringContent(Utilities.SerializeObject(error), Encoding.UTF8, "application/json");
                client.PostAsync(ConstValues.SlackUrl, content).Wait();
            }
        }

        protected string GetIpAddress()
        {
            if (ConstValues.IsDevelopmentEnvironment)
            {
                HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var values);
                var stringValue = values.ToString();
                var array = stringValue.Split(",", StringSplitOptions.RemoveEmptyEntries);
                return array.LastOrDefault();
            }
            else
            {
                var ip = HttpContext.Connection?.RemoteIpAddress;
                var stringValue = ip?.ToString();
                var array = stringValue?.Split(",", StringSplitOptions.RemoveEmptyEntries);
                return array?.LastOrDefault();
            }
        }
    }
}