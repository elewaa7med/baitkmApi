using Baitkm.Enums;
using Baitkm.Enums.Attachments;
using Baitkm.Enums.Subscriptions;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers.AnnouncementLocation;
using Baitkm.Infrastructure.Helpers.ParserHelpers;
using Baitkm.Infrastructure.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Baitkm.Infrastructure.Helpers
{
    public static class Utilities
    {
        public static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;
            return !a1.Where((t, i) => t != a2[i]).Any();
        }

        public static string KeyGenerator(int count)
        {
            var sb = new StringBuilder();
            for (int i = 1; i <= count; i++)
            {
                sb.Append("9");
            }
            var limitString = sb.ToString();
            int.TryParse(limitString, out int limit);
            if (limit == 0)
                return null;
            var random = new Random();
            return random.Next(0, limit).ToString($"D{count.ToString()}");
        }

        public static void SendKeyByEmail(string emailTo, string Key, string subject, string bodyText)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(ConstValues.EmailAccount, "Baitkm"),
                Body = string.Format(bodyText + " " + Key),
                IsBodyHtml = true,
                Subject = subject,
                To = { new MailAddress(emailTo) }
            };
            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(ConstValues.EmailAccount, ConstValues.EmailAccountPassword)
            };
            smtpClient.Send(mail);
        }

        //Send Email to user or guest when add similar announcement like subscribe announcement
        public static void SendEmail(string emailTo, string subject, string bodyText)
        {
            var mail = new MailMessage
            {
                IsBodyHtml = true,
                From = new MailAddress(ConstValues.EmailAccount, "Baitkm"),
                Body = bodyText,
                Subject = subject,
                To = { new MailAddress(emailTo) }
            };
            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(ConstValues.EmailAccount, ConstValues.EmailAccountPassword)
            };
            smtpClient.Send(mail);
            smtpClient.Dispose();
            mail.Dispose();
        }

        public static void SendKeyByTwilio(string phoneNumber, string Key, string bodyText)
        {
            TwilioClient.Init(ConstValues.TwilioAccountSid, ConstValues.TwilioAuthToken);
            MessageResource.Create(to: new PhoneNumber(phoneNumber),
                 from: new PhoneNumber(ConstValues.TwilioPhone),
                 body: $"{bodyText} {Key}");
        }

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
                throw new SmartException(nameof(password));
            using (var bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            var dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
                return false;
            if (password == null)
                throw new SmartException(nameof(password));
            var src = Convert.FromBase64String(hashedPassword);
            if (src.Length != 0x31 || src[0] != 0)
                return false;
            var dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            var buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (var bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArrayCompare(buffer3, buffer4);
        }

        public static string StringBuilderModel(AnnouncementLocateModel model)
        {
            StringBuilder sb = new StringBuilder();
            if (model.BuildingNumber != null)
                sb.Append(model.BuildingNumber + " ");
            if (model.StreetName != null)
                sb.Append(model.StreetName + ", ");
            if (model.Borough != null)
                sb.Append(model.Borough + ", ");
            if (model.City != null)
                sb.Append(model.City + ", ");
            if (model.Country != null)
                sb.Append(model.Country);
            return sb.ToString();
        }

        public static async Task<AnnouncementLocateModel> GetAddressFromGoogle(decimal lat, decimal lng, Language language)
        {
            var result = new AnnouncementLocateModel();

            Uri addressUri = null;
            if (language == Language.English)
                addressUri = new Uri(
                    $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}{ConstValues.GoogleLocateKey}&language=en");
            else
                addressUri = new Uri(
                    $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}{ConstValues.GoogleLocateKey}&language=ar");
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(addressUri);
                var json = await resp.Content.ReadAsStringAsync();
                var token = JToken.Parse(json);
                var array = token.SelectToken("results");
                var deserialized = JsonConvert.DeserializeObject<List<UserLocationParsingBase>>(array.ToString());
                //var item = deserialized.FirstOrDefault();

                result.Lat = lat;
                result.Lng = lng;
                result.BuildingNumber = deserialized.SelectMany(s => s.AddressComponents)
                    .Where(x => x.Types.Contains("street_number")).Select(ac => ac.Name).FirstOrDefault();
                result.StreetName = deserialized.SelectMany(s => s.AddressComponents)
                    .Where(x => x.Types.Contains("route")).Select(ac => ac.Name).FirstOrDefault();
                result.City = deserialized.SelectMany(s => s.AddressComponents)
                    .Where(x => x.Types.Contains("locality")).Select(ac => ac.Name).FirstOrDefault();
                result.Country = deserialized.SelectMany(s => s.AddressComponents)
                    .Where(x => x.Types.Contains("country")).Select(ac => ac.Name).FirstOrDefault();
                result.PostalCode = deserialized.SelectMany(s => s.AddressComponents)
                    .Where(x => x.Types.Contains("postal_code")).Select(ac => ac.Name).FirstOrDefault();
                //Test
                //var BuildingNumber = deserialized.SelectMany(s => s.AddressComponents)
                //    .Where(x => x.Types.Contains("street_number")).Select(ac => ac.Name).FirstOrDefault();
                //var StreetName = deserialized.SelectMany(s => s.AddressComponents)
                //    .Where(x => x.Types.Contains("route")).Select(ac => ac.Name).FirstOrDefault();
                //var City = deserialized.SelectMany(s => s.AddressComponents)
                //    .Where(x => x.Types.Contains("locality")).Select(ac => ac.Name).FirstOrDefault();
                //var Country = deserialized.SelectMany(s => s.AddressComponents)
                //    .Where(x => x.Types.Contains("country")).Select(ac => ac.Name).FirstOrDefault();
                //var PostalCode = deserialized.SelectMany(s => s.AddressComponents)
                //    .Where(x => x.Types.Contains("postal_code")).Select(ac => ac.Name).FirstOrDefault();
                //Test
            }
            return result;
        }

        public static SubscriptionsType SubscriptionsTypeDetector(SubscriptionsType type)
        {
            switch (type)
            {
                case SubscriptionsType.NewMessagesNotifications:
                    return SubscriptionsType.NewMessagesNotifications;
                case SubscriptionsType.NewSavedFilterSuggestionNotifications:
                    return SubscriptionsType.NewSavedFilterSuggestionNotifications;
                case SubscriptionsType.PhoneNumberVisibility:
                    return SubscriptionsType.PhoneNumberVisibility;
                default:
                    throw new Exception(nameof(type));
            }
        }

        public static bool IsNullOrEmpty(params string[] param)
        {
            foreach (var variable in param)
            {
                if (string.IsNullOrEmpty(variable))
                    return false;
            }
            return true;
        }

        public static string SerializeObject(object model)
        {
            return JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        private const int ExifOrientationId = 0x112; //274

        public static void ExifRotate(this Bitmap img)
        {
            if (!img.PropertyIdList.Contains(ExifOrientationId))
                return;

            var prop = img.GetPropertyItem(ExifOrientationId);
            int val = BitConverter.ToUInt16(prop.Value, 0);
            var rot = RotateFlipType.RotateNoneFlipNone;

            if (val == 3 || val == 4)
                rot = RotateFlipType.Rotate180FlipNone;
            else if (val == 5 || val == 6)
                rot = RotateFlipType.Rotate90FlipNone;
            else if (val == 7 || val == 8)
                rot = RotateFlipType.Rotate270FlipNone;

            if (val == 2 || val == 4 || val == 5 || val == 7)
                rot |= RotateFlipType.RotateNoneFlipX;

            if (rot != RotateFlipType.RotateNoneFlipNone)
                img.RotateFlip(rot);
        }

        public static string ReturnFilePath(string baseUrl, string action, UploadType uploadType, string fileName, bool isBlur = false, int id = 0)
        {
            return string.IsNullOrEmpty(fileName) ? fileName : $"{baseUrl}{action}{uploadType}/{Path.GetFileName(fileName)}/{isBlur.ToString()}/{id}";
        }

        public static string ReturnFilePath(string baseUrl, string action, UploadType uploadType, string fileName, int width, int height, bool isBlur = false, int id = 0)
        {
            return string.IsNullOrEmpty(fileName) ? fileName : $"{baseUrl}{action}{uploadType}/{Path.GetFileName(fileName)}/{width}/{height}/{isBlur.ToString()}/{id}";
        }

        public static string ReturnFilePath(string baseUrl, string action, UploadType uploadType, string fileName)
        {
            return string.IsNullOrEmpty(fileName) ? fileName : $"{baseUrl}{action}{uploadType}/{Path.GetFileName(fileName)}";
        }

        public static string ReturnFilePath(UploadType uploadType, string fileName, bool isBlur = false, int id = 0)
        {
            return string.IsNullOrEmpty(fileName) ? fileName : $"{ConstValues.MediaBaseUrl}{ConstValues.MediaDownload}{uploadType}/{Path.GetFileName(fileName)}/{isBlur.ToString()}/{id}";
        }

        public static async Task<string> MakeCall(this string input)
        {
            var uri = new Uri($"{ConstValues.GoogleLocateBase}{input}&types=(cities){ConstValues.GoogleLocateKey}");
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(uri);
                return await resp.Content.ReadAsStringAsync();
            }
        }

        public static async Task<string> CountryNameCheck(this string country, string city)
        {
            string complete;
            string jsonString;
            if (string.IsNullOrEmpty(country))
                return country;
            if (!string.IsNullOrEmpty(city))
            {
                complete = $"{city},{country}";
                jsonString = await complete.MakeCall(LocateType.City);
            }
            else
            {
                complete = country;
                jsonString = await complete.MakeCall(LocateType.Country);
            }
            var token = JToken.Parse(jsonString);
            var jsonList = token.SelectToken("predictions").ToString();
            var parsed = JsonConvert.DeserializeObject<List<ParsingBaseModel>>(jsonList);
            parsed.RemoveAll(x => !(x.Types.Contains("country") || x.Types.Contains("locality")));
            var item = parsed.FirstOrDefault();
            if (item == null)
                return null;
            return await item.PlaceId.Details();
        }

        public static async Task<string> MakeCall(this string input, LocateType type)
        {
            string googleType = "";
            switch (type)
            {
                case LocateType.Country:
                    googleType = "(regions)";
                    break;
                case LocateType.City:
                    googleType = "(cities)";
                    break;
                case LocateType.Address:
                    googleType = "address";
                    break;
            }
            //        new Uri(
            //$"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}{ConstValues.GoogleLocateKey}&language=en");

            var uri = new Uri($"{ConstValues.GoogleLocateBase}{input}&types={googleType}{ConstValues.GoogleLocateKey}&language=ar");
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(uri);
                return await resp.Content.ReadAsStringAsync();
            }
        }

        public static async Task<string> Details(this string placeId)
        {
            if (placeId == null)
                return placeId;
            string name;
            var uri = new Uri(
                $"https://maps.googleapis.com/maps/api/place/details/json?placeid={placeId}{ConstValues.GoogleLocateKey}");
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(uri);
                var json = await resp.Content.ReadAsStringAsync();
                var parsed = JsonConvert.DeserializeObject<DetailsParsingHelper>(json);
                var country = parsed.DetailsParisngResultModel.AdressComponents.Find(x => x.Types.Contains("country"));
                name = country.Name;
            }
            return name;
        }

        public static string SubscribersEmailStyle(int id, string address, DateTime createdDate, int bedroom,
            int bathroom, double rating, decimal area, decimal price)
        {
            var style1 =
                @"* {
                    box - sizing: border - box;
                    margin: 0;
                    padding: 0;
                    font - family: Arial;
                    }";
            var style2 =
                @"* .G-btn {
                     
                    }";
            var style3 =
                @"* .G-btn:hover {
                        background - color: #EC6A44;
                        color: white;
                    }";
            var style4 =
                @"* .G-btn:focus-visible {
                        border: 1px solid #EC6A44 !important;
                    }";
            var str = @$"
                <section>
                <style>

                    {style1}

                    {style2}

                    {style3}

                    {style4}
​
                </style>
                <table align='center' width='700' border='0' cellspacing='0' cellpadding='0' style='width:700px; margin: 0 auto;padding: 15px;'>
                    <tr>
                        <td>
                            <table style='background-image: url(https://i.ibb.co/2jJqsgh/test.png);  background-position: center; background-repeat: no-repeat;background-size: cover; overflow: hidden; border-radius: 6px;  width: 100%; position: relative; padding: 5px 0;'>
                                <tr>
                                  <td>
                                    <table style=' width: 98%; left: 50%; top: 50%; transform: translate(-50%, -50%); background-color: rgba(255,255,255,0.7);  border-radius: 6px; z-index: 0; margin: 0 auto'>
                                        <tr>
                                             <td style='color: white;  padding-top: 30px;   padding-bottom: 30px;  padding-left: 15px;  padding-right: 15px;'>
                                        <img style='width: 100px;  height: 35px; position: relative; z-index: 9;' src='https://i.ibb.co/sJSQfdd/baitkm.png' alt='Group-1229' border='0'>
                                    </td>
                                    <td>
                                        <div style='text-align: right; flex-direction: column; align-items: flex-end; position: relative;  color: white;  padding-top: 30px;   padding-bottom: 30px;  padding-left: 15px;  padding-right: 15px;z-index: 99;'>
                                        <a style='display: block; color: #EE6B45;  font-size: 16px;  text-decoration: none;  font-weight: bold;' href='tel:+1919-545-7766'>+1919-545-7766</a>
                                        <a style='display: block; color: #014556;  font-size: 16px;text-decoration: none; margin-top: 5px;' href='http://baitkm.com/'>www.baitkm.com</a>
                                        </div>
                                    </td>
                                      </tr>
                                    </table>
                                </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <table style='width: 100%;'>
                                <tr>
                                    <td>
                                        <p style='font-size: 18px; font-weight: bold; margin-top: 20px; margin-bottom:0'>Recommended for you.</p>
                                        <p style='font-size: 16px; font-weight: normal;  margin-top: 10px; margin-bottom: 20px; margin-top:0'>We found 1 property that match your criteria.</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <table style='   padding-top: 25px;  padding-bottom: 25px;border-bottom: 1px solid rgba(0, 0, 0, 0.3);border-top: 1px solid rgba(0, 0, 0, 0.3); width: 100%;'>
                                <tr>
                                    <td>
                                        <a href='https://baitkm.com/announcement/details/{id}' style=' display: flex;align-items: center;  text-decoration: none; color: black;'>
                                            <div style='background-image: url(https://i.ibb.co/whhMzVd/Group-2.png);  background-position: center;  background-repeat: no-repeat;background-size: cover; border-radius: 5px; width: 180px; height: 140px;'></div>
                                            <div style=' width: calc(100% - 180px); padding-left: 15px;'>
                                                <div style='font-size: 16px; color: #EE6B45; margin-top: 10px; margin-bottom:0 '>{price} $</div>​​
                                                <h4 style='font-size: 12px; color: #6A6A6A; margin-top: 5px; display: flex; align-items: center'>
                                                <span style='background-image: url(https://i.ibb.co/MSZGDhd/pin-1.png); width: 15px; height: 15px; background-position: center; background-repeat: no-repeat; background-size: contain; display: block; margin-right: 10px;'></span> {address}
                                                </h4>      
                                                <h4 style='font-size: 12px; color: #6A6A6A; margin-top: 5px; display: flex; align-items: center'>
                                                <span style='background-image: url(https://i.ibb.co/488pHbg/calendar-16.png);width: 15px; height: 15px; background-position: center; background-repeat: no-repeat; background-size: contain; display: block; margin-right: 10px;'></span> {createdDate}
                                                </h4>
                                                <ul style='display: flex; flex-wrap: wrap; margin: 10px -10px 0; padding: 0px; width:83%'>
                                                    <li style= 'margin: 0; width: 25%; padding: 0 10px 0 0 ; list-style: none; display: flex;  align-items: center; font-size: 14px;'>
                                                    <span style='background-image: url(https://i.ibb.co/JcrKfCz/bed.png); width: 15px; height: 15px; background-position: center; background-repeat: no-repeat; background-size: contain; display: block; margin-right: 10px;'></span> {bedroom} </li>
                                                    <li style= 'margin: 0; width: 25%; padding: 0 10px; list-style: none; display: flex;  align-items: center; font-size: 14px;'>
                                                    <span style='background-image: url(https://i.ibb.co/NL20NKS/bath.png); width: 15px; height: 15px; background-position: center; background-repeat: no-repeat; background-size: contain; display: block; margin-right: 10px;'></span> {bathroom} </li>
                                                    <li style= 'margin: 0; width: 25%; padding: 0 10px; list-style: none; display: flex;  align-items: center; font-size: 14px;'>
                                                    <span style='background-image: url(https://i.ibb.co/1MyPT3K/plan.png); width: 15px; height: 15px; background-position: center; background-repeat: no-repeat; background-size: contain; display: block; margin-right: 10px;'></span> {area} m2 </li>
                                                    <li style='margin: 0; width: 25%; padding: 0 10px; list-style: none; display: flex;  align-items: center; font-size: 14px;'>
                                                    <span style='background-image: url(https://i.ibb.co/D4bsmgc/Mask-Group-57.png); width: 15px; height: 15px; background-position: center; background-repeat: no-repeat; background-size: contain; display: block; margin-right: 10px;'></span> {rating} </li>
                                                </ul>
                                            </div>
                                        </a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <table style='  margin-top: 20px; border-radius: 6px;background-color: #5C5C5C;padding: 10px;width: 100%; text-align: center; position: relative;'>
                                <tr>
                                    <td>
                                        <h3 style=' color: white; font-size: 14px;'>Want to change how you receive these emails?</h3>
                                        <p style='color: white; font-size: 16px; margin-top: 5px;'>You can unsubscribe from this list</p>
                                        <span style=' color: white; font-size: 14px; margin-top: 15px; display: block;'>© 2020 Baitkm, Inc.</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </section>   
            ";
            return str;
        }

        public static string NewUserEmailStyle(string email)
        {
            var style1 =
                @"* {
                    box - sizing: border-box;
                    margin: 0;
			        padding: 0;
			        font-family: Arial;
                    }
.P-links a {
color:white !important;
}
";


            var s = $@"
            <section>
            <style>

                {style1}

	            </style>
	            <table align='center' width='700' border='0' cellspacing='0' cellpadding='0' style='width:700px; margin: 0 auto; padding: 15px;'>
	            	<tr>
	            		<td>
	            			<table style='background-color: #014556; width: 100%; border-radius: 6px;	padding: 25px 15px;'>
	            				<tr>
	            					<td>
                                        <div style=' width: 120px; height: 35px; background-image: url(https://i.ibb.co/sFc4fwm/Group-1147-2x.png); background-position: center;	background-repeat: no-repeat;	background-size: contain;'></div>
	            					</td>
	            					<td>
	            						<div style='display: flex;align-items: center;justify-content: flex-end;'>
	            							<ul style='display: flex; margin: 0 auto; margin-right:0;'>
	            								<li style='list-style: none;'>
	            									<a style='width: 30px; height: 30px; margin: 3px; border-radius: 6px; background-color: white; display: flex; align-items: center; justify-content: center;'
	            									   target='_blank' href='https://www.facebook.com/'> <span style='margin: auto; width: 17px; height: 17px; background-image: url(https://i.ibb.co/X2vbbnc/facebook-2.png); background-position: center;	background-repeat: no-repeat;	background-size: contain;'></span>
                                                    </a>
	            								</li>
	            								<li style='list-style: none;'>
	            									<a style='	width: 30px; height: 30px; margin: 3px; border-radius: 6px;	background-color: white;	display: flex;	align-items: center; justify-content: center;'
	            									   target='_blank' href='https://accounts.google.com/ServiceLogin/signinchooser?hl=en&passive=true&continue=https%3A%2F%2Fwww.google.com%2F&ec=GAZAAQ&flowName=GlifWebSignIn&flowEntry=ServiceLogin'>
 <span style='margin:auto; width: 17px; height: 17px; background-image: url(https://i.ibb.co/SB1H192/google.png); background-position: center;	background-repeat: no-repeat;	background-size: contain;'></span>
                                                    </a>
	            								</li>
	            							</ul>
	            						</div>
	            					</td>
	            				</tr>
	            			</table>
	            		</td>
	            	</tr>
	            	<tr>
	            		<td>
	            			<table style='background-image: url(https://i.ibb.co/sqDG2Cp/Group-1288-2x.png);	padding: 15px;border-radius: 6px;	margin-top: 20px;width: 100%;background-position: center;	background-repeat: no-repeat;	background-size: cover;'>
	            				<tr>
	            					<td>
	            						<p style='	text-align: center;	max-width: 400px;	width: 100%;margin: 0 auto;	color: white;	font-size: 20px;'>
	            							Find properties for Sale & Rent in Saudi Arabia & across MENA region
                                        </p>
                                    </td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<h3 style='font-size: 40px;text-align: center; color: #EE6B45; margin-top: 40px; margin-bottom: 10px;'> Welcome to Baitkm.com </h3>
	            					</td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<h3 style='text-align: center;	color: white; font-size: 24px;font-weight: normal;text-align: center; margin:0;'>
	            							Your login ID:
	            							
	            						</h3>
<a href={email} target='_blank' style='font-weight: bold; text-align: center; color:white; font-size:22px; display:block; margin:0 auto'>{email}</a>
	            					</td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<p style='font-size: 24px;	font-weight: bold;	color: #EE6B45;	text-align: center;	margin-top: 60px;'>
                                            Wow to make the most of your experience
	            						</p>
	            					</td>
	            				</tr>
	            			</table>
	            		</td>
	            	</tr>
	            	<tr>
	            		<td>
	            			<table style='margin-top: 20px;	width: 100%;'>
	            				<tr>
	            					<td>
	            						<h3 style='display:block; margin:auto; font-size: 35px;color: #014556; text-align: center; font-weight: bold; margin-top: 15px; margin-bottom: 20px; text-transform: uppercase;'>
	            							Introduction
	            						</h3>
	            					</td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<p style='margin:0; font-size: 18px; color: #014556;line-height: 25px;'>
	            							Baitkm is a pioneering real estate platform in the Middle East,
	            							that connects agents,
	            							tenants, homeowners & investors. Baitkm is a comprehensive real estate
	            							marketing solution,
	            							for those looking to buy, sell or renting real estate. The website provides an
	            							integrated
	            							property search experience enhancing the customer journey for tenants,
	            							landlords and
	            							homeowners with bespoke tools and solutions. In addition to supporting the
	            							customers during
	            							their property search experience, Baitkm will also enable property agents to
	            							advertise their
	            							listings to the right audience leading to greater success in the overall
	            							market.
                                        </p>
	            					</td>
	            				</tr>
	            			</table>
	            		</td>
	            	</tr>
	            	<tr>
	            		<td>
                        <table style='margin-top: 20px; border-radius: 6px; background-color: #EE6B45; width: 100%; padding: 15px'>
	            				<tr>
	            					<td>
	            						<div style='display: flex; align-items: center; justify-content: center'>
	            							<div style='padding: 10px; width: 32%'>
	            								<div style='border-radius: 6px; background-color: white; padding: 15px'>
	            									<div style='background-image: url(https://i.ibb.co/ft0Fn6w/Mask-Group-64-2x.png); background-position: center; background-repeat: no-repeat;background-size: contain;width: 45px; height: 45px; margin: 0 auto 20px'></div>
	            									<p style='margin:0; font-size: 20px; color: #014556; text-align: center'>List an </p>
	            									<span style='display: block;margin-top: 10px; font-size: 18px; font-weight: bold; text-align: center; color: #014556; text-transform: uppercase'>announcement</span>
	            								</div>
	            							</div>
	            							<div style='padding: 10px; width: 32%'>
	            								<div style='border-radius: 6px; background-color: white; padding: 15px'>
	            									<div style='background-image: url(https://i.ibb.co/NNx53P1/Mask-Group-63-2x.png); background-position: center; background-repeat: no-repeat;background-size: contain;width: 45px; height: 45px; margin: 0 auto 20px'></div>
	            									<p style=' margin:0; font-size: 20px; color: #014556; text-align: center'>Share with your</p>
	            									<span style='display: block;margin-top: 10px; font-size: 18px; font-weight: bold; text-align: center; color: #014556;text-transform: uppercase'>friends</span>
	            								</div>
	            							</div>
	            							<div style='padding: 10px; width: 32%'>
	            								<div style='border-radius: 6px; background-color: white; padding: 15px'>
	            									<div style='background-image: url(https://i.ibb.co/RCzYVh8/Mask-Group-62-2x.png); background-position: center; background-repeat: no-repeat;background-size: contain;width: 45px; height: 45px; margin: 0 auto 20px'></div>
	            									<p style='margin:0; font-size: 20px; color: #014556; text-align: center'>Save your </p>
	            									<span style='display: block;margin-top: 10px; font-size: 18px; font-weight: bold; text-align: center; color: #014556;text-transform: uppercase'>favorites</span>
	            								</div>
	            							</div>
	            						</div>
	            					</td>
	            				</tr>
	            			</table>
	            		</td>
	            	</tr>
	            	<tr>
	            		<td>
	            			<table style='margin-top: 45px; width: 100%'>
	            				<tr>
	            					<td>
	            						<h3 style='font-size: 35px; color: #014556; text-align: center; text-transform: uppercase; font-weight: bold; margin:15px 0;'>
	            							unique features</h3></td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<p style='margin:0; margin-top:10px; font-size: 18px; color: #014556; margin-top: 15px'><span style='font-weight: bold'>a.</span> 	We	are	not	brokering	firm	therefore:	you	won’t	be	paying	money	against	brokerage.
	            						</p>
	            					</td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<p style=' margin:0; margin-top:10px; font-size: 18px; color: #014556; margin-top: 15px'><span style='font-weight: bold'>b.</span>
                                            On our website you can find properties anywhere not only in the Middle East, but you can also find properties in the
                                            United Kingdom, Europe, USA, etc and anywhere as we try to collect and match you with the maximum amount properties.
	            						</p>
	            					</td>
​
	            				</tr>
	            				<tr>
	            					<td>
	            						<p style=' margin:0; margin-top:10px; font-size: 18px; color: #014556; margin-top: 15px'><span style='font-weight: bold'>c.</span> Fast and effective.</p>
	            					</td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<p style=' margin:0; margin-top:10px; font-size: 18px; color: #014556; margin-top: 15px'><span style='font-weight: bold'>d.</span>  
                                            Our website and mobile application are connected with google maps, so you know the exact location of the property.​
	            						</p>
	            					</td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<p style='margin:0; margin-top:10px; font-size: 18px; color: #014556; margin-top: 15px'><span style='font-weight: bold'>e.</span>
                                            You can even contact the owner on the spot, by sending a message or calling them directly, to discuss or negotiate any inquiries you may have.​
	            						</p>
	            					</td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<p style=' margin:0; margin-top:10px; font-size: 18px; color: #014556; margin-top: 15px'><span style='font-weight: bold'>f.</span>
                                            Our purpose is to provide customer satisfaction, therefore, you will be available to view the rating of the existing property announcement.​
	            						</p>
	            					</td>
	            				</tr>
	            			</table>
	            		</td>
	            	</tr>
	            	<tr>
	            		<td>
	            			<table style='width: 100%; margin-top: 60px; background-color: #014556; border-radius: 6px; padding: 15px;'>
	            				<tr>
	            					<td>
	            						<ul style='display: table; align-items: center; justify-content: center; margin:auto; padding:0'>
	            							<li style='list-style: none; margin: 5px 10px; display:inline-block'>
	            								<a target='_blank' href='https://apps.apple.com/us/app/baitkm/id1476247666?ls=1'><img
	            												src='https://i.ibb.co/BcQC1KL/Group-1295-2x.png' alt='Group-1295-2x' border='0'></a>
	            							</li>
	            							<li style='list-style: none; margin: 5px 10px; display:inline-block'>
	            								<a target='_blank'
	            								   href='https://play.google.com/store/apps/details?id=com.abm.armboldmind.baitkm&hl=en'><img
	            												src='https://i.ibb.co/mhd6D5V/Group-1302-2x.png' alt='Group-1302-2x' border='0'></a>
	            							</li>
	            						</ul>
	            					</td>
	            				</tr>
	            				<tr>
	            					<td>
	            						<table style='width: 100%'>
	            							<tr>
	            								<td style='max-width: 100px'>
	            									<ul style='display: flex; align-items: center; justify-content: center; margin-top: 30px'>
	            										<li style='list-style: none; margin-right: 80px'><a
	            														style='color: white; text-decoration: none; font-size: 16px;' target='_blank'
	            														href='https://baitkm.com/'>Home</a></li>
	            										<li style='list-style: none; margin-right: 80px'><a
	            														style='color: white; text-decoration: none; font-size: 16px;' target='_blank'
	            														href='https://baitkm.com/terms'>Terms & Conditions</a></li>
	            										<li style='list-style: none'><a style='color: white; text-decoration: none; font-size: 16px;' 
                                                            target='_blank' href='https://baitkm.com/privacy'>Privacy Policy</a></li>
	            									</ul>
	            								</td>
	            							</tr>
	            						</table>
	            					</td>
	            				</tr>
	            			</table>
	            		</td>
	            	</tr>
	            </table>
            </section>
            ";

            return s;
        }
    }
}