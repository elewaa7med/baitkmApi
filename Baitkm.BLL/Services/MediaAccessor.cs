using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums.Attachments;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.ResponseModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services
{
    public class MediaAccessor
    {
        public async Task<string> Upload(IFormFile file, UploadType uploadType, int id = 0, bool isRelativeRequested = false)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConstValues.MediaBaseUrl);
                var content = new MultipartFormDataContent();
                if (file.Length <= 0)
                    return null;
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                content.Add(new StreamContent(file.OpenReadStream())
                {
                    Headers =
                    {
                        ContentLength = file.Length,
                        ContentType = new MediaTypeHeaderValue(file.ContentType)
                    }
                }, "file", fileName);
                var response = await client.PostAsync($"{ConstValues.MediaUpload}{(int)uploadType}/{isRelativeRequested}/{id}", content);
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;
                var serialized = await response.Content.ReadAsStringAsync();
                var deserialized = JsonConvert.DeserializeObject<ServiceResult>(serialized);
                return !deserialized.Success ? null : deserialized?.Data?.ToString();
            }
        }

        public async Task<bool> MultipleUpload(List<IFormFile> files, UploadType uploadType,
            AttachmentType announcementPhotoType, int announcementId = 0, bool isRelativeRequested = false)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConstValues.MediaBaseUrl);
                client.DefaultRequestHeaders.TryAddWithoutValidation("announcementId", announcementId.ToString());
                client.DefaultRequestHeaders.TryAddWithoutValidation("announcementPhotoType", announcementPhotoType.ToString());
                var content = new MultipartFormDataContent();
                foreach (var file in files)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    using (var stream = file.OpenReadStream())
                    {
                        using (var br = new BinaryReader(stream))
                        {
                            var data = br.ReadBytes((int)stream.Length);
                            var bytes = new ByteArrayContent(data);
                            content.Add(bytes, "files", fileName);
                        }
                    }
                }
                await client.PostAsync($"{ConstValues.MediaMultipleUpload}{(int)uploadType}/{isRelativeRequested}", content);
                return true;
            }
        }

        public async Task<string> GetPhotoFromSocial(string uri)
        {
            using (var client = new HttpClient())
            {
                var model = new GetPhotoFromSocialModel
                {
                    Uri = uri
                };
                var json = Utilities.SerializeObject(model).Replace("\\", string.Empty);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.BaseAddress = new Uri(ConstValues.MediaBaseUrl);
                var response = await client.PostAsync($"{ConstValues.MediaFromSocialPage}", content);
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;
                var serialized = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string>(serialized);
            }
        }

        public async Task Remove(string fileName, UploadType type, int id = 0)
        {
            using (var client = new HttpClient())
            {
                await client.DeleteAsync($"{ConstValues.MediaBaseUrl}{ConstValues.MediaRemove}/{(int)type}/{fileName}/{id}");
            }
        }

        public async Task<long> GetLength(string fileName, UploadType type, int id = 0)
        {
            using (var client = new HttpClient())
            {
                var response =
                    await client.GetAsync(
                        $"{ConstValues.MediaBaseUrl}{ConstValues.MediaLength}/{(int)type}/{fileName}/{id}");
                if (response.StatusCode != HttpStatusCode.OK)
                    return 0;
                var serialized = await response.Content.ReadAsStringAsync();
                var deserialized = JsonConvert.DeserializeObject<ServiceResult>(serialized);
                return !deserialized.Success ? 0 : Convert.ToInt64(deserialized.Data.ToString());
            }
        }

        public async Task<string> DownloadMap(DownloadMapModel model)
        {
            var serializedModel = Utilities.SerializeObject(model);
            var content = new StringContent(serializedModel, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"{ConstValues.MediaBaseUrl}{ConstValues.DownloadMap}", content);
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;
                var serialized = await response.Content.ReadAsStringAsync();
                var deserialized = JsonConvert.DeserializeObject<ServiceResult>(serialized);
                return !deserialized.Success ? null : deserialized.Data.ToString();
            }
        }
    }
}
