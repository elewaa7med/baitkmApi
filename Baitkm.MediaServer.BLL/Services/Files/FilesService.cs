using Baitkm.Enums.Attachments;
using Baitkm.Infrastructure.Helpers;
using Baitkm.MediaServer.BLL.Helpers;
using Baitkm.MediaServer.BLL.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Distributed;
using SautinSoft.Document;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Color = SautinSoft.Document.Color;
using SixImage = SixLabors.ImageSharp.Image;

namespace Baitkm.MediaServer.BLL.Services.Files
{
    public class FilesService : IFilesService
    {
        private readonly IDistributedCache _cache;
        private readonly string _webRootPath;
        public FilesService(IDistributedCache cache,
            IHostingEnvironment environment)
        {
            _cache = cache;
            _webRootPath = environment.WebRootPath;
        }

        public async Task<DownloadFileModel> Download(UploadType type, string fileName, int id, bool isBlur)
        {
            string resultFileName;
            if (isBlur)
                resultFileName = FilesUtilities.GetRelativePathBlurs(
                    $"{Path.GetFileNameWithoutExtension(fileName)}_blur{Path.GetExtension(fileName)}", type, id);
            else
                resultFileName = FilesUtilities.GetRelativePath(fileName, type, id);
            var file = Path.Combine(_webRootPath, resultFileName);
            var theFile = new FileInfo(file);
            if (!theFile.Exists)
                return null;
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            if (string.IsNullOrEmpty(contentType))
                contentType = "image/jpg";
            if (!contentType.StartsWith("image"))
                return new DownloadFileModel
                {
                    Bytes = await File.ReadAllBytesAsync(file),
                    ContentType = contentType
                };
            var key = fileName.CacheKeyGenerator(type, isBlur);
            var fromCache = _cache.Get(key);
            if (fromCache != null)
                return new DownloadFileModel
                {
                    Bytes = fromCache,
                    ContentType = contentType
                };
            byte[] fileBytes;
            try
            {
                fileBytes = await File.ReadAllBytesAsync(file);
            }
            catch (IOException)
            {
                return null;
            }
            _cache.Set(key, fileBytes);
            return new DownloadFileModel
            {
                ContentType = contentType,
                Bytes = fileBytes
            };
        }

        public string DownloadFromSocialPage(string uri)
        {
            try
            {
                var imageName =
                    FilesUtilities.GetDestFileName(uri.Contains("google") ? "googleImage.jpg" : "fbImage.jpg",
                        UploadType.ProfilePhoto);
                var dest = FilesUtilities.GetRelativePath(imageName, UploadType.ProfilePhoto);
                //var destBlur = FilesUtilities.GetRelativePathBlurs(imageName, UploadType.ProfilePhoto);
                var newFile = Path.Combine(_webRootPath, dest);
                //var newFileBlur = Path.Combine(_webRootPath, destBlur);
                Path.GetDirectoryName(newFile).CreateDirectory();

                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(new Uri(uri), newFile.SlashConverter());
                    //webClient.DownloadFile(new Uri(uri), FilesUtilities.SlashConverter(newFileBlur));
                }

                return Path.GetFileName(newFile);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> FileUploader(UploadFileModel model, UploadType type, bool isRelativeRequested, int announcementId = 0)
        {
            if (model.File.Length <= 0)
                return null;
            var dest = model.File.FileName.GetDestFileName();
            dest = FilesUtilities.GetRelativePath(dest, type, announcementId);
            var newFile = Path.Combine(_webRootPath, dest);
            string newDest = null;
            try
            {
                Path.GetDirectoryName(newFile).CreateDirectory();
                using (var stream = new FileStream(newFile, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }
                newDest = model.File.FileName.GetDestFileName();
                var extension = Path.GetExtension(newDest);
                if (!extension.Contains(".jpg") && !extension.Contains(".jpeg") && !extension.Contains(".png"))
                    return Path.GetFileName(newFile);
                else
                {
                    using (var current = new Bitmap(newFile.SlashConverter()))
                    {
                        current.ExifRotate();
                        newDest = FilesUtilities.GetRelativePath(newDest, type, announcementId);
                        var newFilePath = Path.Combine(_webRootPath, newDest);
                        using (var writeStream = new FileStream(newFilePath, FileMode.Create))
                        {
                            var ext = Path.GetExtension(newFilePath);
                            if (string.IsNullOrEmpty(ext) || !ext.ToLower().Contains("png"))
                                current.Save(writeStream, ImageFormat.Jpeg);
                            else
                                current.Save(writeStream, ImageFormat.Png);
                        }
                    }
                }
                if (type == UploadType.HomePageCoverImage || type == UploadType.ProfilePhoto || type == UploadType.MessageFiles)
                    return !isRelativeRequested ? Path.GetFileName(dest) : dest;
                var waterMarkDest = SetWaterMark(Path.GetFileName(newDest), type, announcementId);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed                             
                Task.Factory.StartNew(() => Blur(Path.GetFileName(waterMarkDest), type), TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                RemoveFile(newFile, type, announcementId);
                return !isRelativeRequested ? Path.GetFileName(waterMarkDest) : waterMarkDest;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                RemoveFile(dest, type, announcementId);
                return null;
            }
        }

        public bool MultipleFileUploader(MultipleFileUploadModel model, UploadType type,
                AttachmentType announcementPhotoType, bool isRelativeRequested, int announcementId)
        {
            Parallel.ForEach(model.Files, async file =>
            {
                var dest = file.FileName.GetDestFileName();
                dest = FilesUtilities.GetRelativePath(dest, type, announcementId);
                new FileExtensionContentTypeProvider().TryGetContentType(dest, out var contentType);
                var newFile = Path.Combine(_webRootPath, dest);
                //try
                //{
                //    Path.GetDirectoryName(newFile).CreateDirectory();
                //    using (var stream = new FileStream(newFile, FileMode.Create))
                //    {
                //        using (var readStream = file.OpenReadStream())
                //        {
                //            lock (dest)
                //            {
                //                readStream.CopyToAsync(stream).Wait();
                //            }
                //        }
                //    }
                //    var waterMarkDest = SetWaterMark(Path.GetFileName(dest), type, announcementId);
                //    if (announcementId != 0)
                //        await Callback(Path.GetFileName(waterMarkDest), announcementId, announcementPhotoType);
                //    if (announcementPhotoType == AnnouncementPhotoType.OtherDocumentations)
                //        ConvertPdf(Path.GetFileName(newFile), type, announcementId);
                //}
                string newFilePath = null;
                try
                {
                    Path.GetDirectoryName(newFile).CreateDirectory();
                    lock (dest)
                    {
                        using (var stream = new FileStream(newFile, FileMode.Create))
                        {
                            file.CopyToAsync(stream).Wait();
                        }
                        if (type != UploadType.AnnouncementDocument && !contentType.Contains("video"))
                            newFilePath = Exif(newFile, file.FileName, type, announcementId);
                    }

                    string waterMarkDest = null;
                    if (announcementPhotoType == AttachmentType.OtherImages && announcementId != 0)
                    {
                        if (!contentType.Contains("video"))
                        {
                            waterMarkDest = SetWaterMark(Path.GetFileName(newFilePath), type, announcementId);
                            await Callback(Path.GetFileName(waterMarkDest), announcementId, announcementPhotoType);
                        }
                        else
                        {
                            await Callback(Path.GetFileName(dest), announcementId, announcementPhotoType);
                            await Task.Factory.StartNew(() => GetThumbnail(newFile, type, announcementId), TaskCreationOptions.LongRunning);
                        }
                    }
                    if (!contentType.Contains("video"))
                        RemoveFile(newFile, UploadType.AnnouncementPhoto, announcementId);
                    if (announcementPhotoType == AttachmentType.OtherDocumentations && announcementId != 0
                        && type != UploadType.MessageFiles)
                    {
                        ConvertPdf(Path.GetFileName(newFile), type, announcementId);
                        await Callback(Path.GetFileName(dest), announcementId, announcementPhotoType);
                    }
                    //if (announcementId != 0 )
                    //    await Callback(Path.GetFileName(waterMarkDest), announcementId, announcementPhotoType);
                    //Blur(Path.GetFileName(newFile), type, announcementId);
                    //#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed                             
                    //                    Task.Factory.StartNew(() => Blur(Path.GetFileName(newFile), type, announcementId), TaskCreationOptions.LongRunning);
                    //#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                catch (Exception)
                {
                    await Decrement(announcementId);
                    RemoveFile(dest, type, announcementId);
                }
            });
            return true;
        }

        public bool RemoveFile(string fileName, UploadType type, int id)
        {
            var path = Path.Combine(_webRootPath, FilesUtilities.GetRelativePath(fileName, type, id)).SlashConverter();
            if (!File.Exists(path))
                return true;
            File.Delete(path);
            var blurPath = Path.Combine(_webRootPath,
                FilesUtilities.GetRelativePathBlurs(
                    $"{Path.GetFileNameWithoutExtension(fileName)}_blur{Path.GetExtension(fileName)}", type, id)).SlashConverter();
            if (!File.Exists(blurPath))
                return true;
            File.Delete(blurPath);
            return true;
        }

        public async Task<DownloadFileModel> Resize(UploadType type, string fileName, int maxWidth, int maxHeight, int id, bool isBlur)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var mime);
            string resultFileName;
            if (type == UploadType.AnnouncementDocument)
            {
                resultFileName = FilesUtilities.GetRelativePdfFile($"{Path.GetFileNameWithoutExtension(fileName)}.jpg", type, id);
                new FileExtensionContentTypeProvider().TryGetContentType(resultFileName, out mime);
            }
            else if (isBlur)
                resultFileName = FilesUtilities.GetRelativePathBlurs(
                    $"{Path.GetFileNameWithoutExtension(fileName)}_blur{Path.GetExtension(fileName)}", type, id);
            else
                resultFileName = FilesUtilities.GetRelativePath(fileName, type, id);
            var file = Path.Combine(_webRootPath, resultFileName);
            if (mime == null)
                return null;
            if (!mime.StartsWith("image"))
                return new DownloadFileModel
                {
                    ContentType = mime,
                    Bytes = await File.ReadAllBytesAsync(file)
                };
            var theFile = new FileInfo(file);
            if (!theFile.Exists)
                return null;
            var key = fileName.CacheKeyGenerator(type, isBlur, maxWidth, maxHeight).ToString();
            var x = key.GetType();
            var fromCache = _cache.Get(key);
            if (fromCache != null)
                return new DownloadFileModel
                {
                    Bytes = fromCache,
                    ContentType = mime
                };
            byte[] fileBytes;
            try
            {
                using (var bitmap = new Bitmap(file))
                {
                    int width;
                    int height;
                    if (bitmap.Width > bitmap.Height)
                    {
                        width = maxWidth;
                        height = Convert.ToInt32(bitmap.Height * maxWidth / (double)bitmap.Width);
                    }
                    else
                    {
                        width = Convert.ToInt32(bitmap.Width * maxHeight / (double)bitmap.Height);
                        height = maxHeight;
                    }
                    var newBitmap = new Bitmap(bitmap, new Size(width, height));
                    fileBytes = newBitmap.ToByteArray(Path.GetExtension(resultFileName));
                    newBitmap.Dispose();
                }
            }
            catch (IOException)
            {
                return null;
            }
            if (string.IsNullOrEmpty(mime))
                mime = "image/jpg";
            _cache.Set(key, fileBytes);
            return new DownloadFileModel
            {
                Bytes = fileBytes,
                ContentType = mime
            };
        }

        public long GetLength(string fileName, UploadType uploadType, int id)
        {
            long length = 0;
            var path = Path.Combine(_webRootPath, FilesUtilities.GetRelativePath(fileName, uploadType, id));
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
                length = fileInfo.Length;
            return length;
        }

        public string SetWaterMark(string fileName, UploadType uploadType, int announcementId = 0)
        {
            var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_water{Path.GetExtension(fileName)}";
            var dest = FilesUtilities.GetRelativePath(fileName, uploadType, announcementId);
            if (announcementId != 0)
                dest = $"{Path.GetDirectoryName(dest)}/{Path.GetFileName(dest)}";
            var newDest = FilesUtilities.GetRelativePath(newFileName, uploadType, announcementId);
            var theFile = Path.Combine(_webRootPath, dest);
            var newFile = Path.Combine(_webRootPath, newDest);
            var waterMark = Path.Combine(_webRootPath, "waterMark.png");
            Bitmap outputImage = null;
            Graphics g = null;
            try
            {
                using (outputImage = new Bitmap(theFile.SlashConverter()))
                {
                    using (g = Graphics.FromImage(outputImage))
                    {
                        g.CompositingMode = CompositingMode.SourceCopy;
                        Rectangle destRect = new Rectangle(0, 0, outputImage.Width, outputImage.Height);
                        g.DrawImage(outputImage, destRect, 0, 0, outputImage.Width, outputImage.Height, GraphicsUnit.Pixel);
                        g.CompositingMode = CompositingMode.SourceOver;
                        using (var waterMarkBm = new Bitmap(waterMark))
                        {
                            var width = (int)(outputImage.Width * 0.15);
                            var height = width * waterMarkBm.Height / waterMarkBm.Width;
                            var newBitmap = new Bitmap(waterMarkBm, new Size(width, height));
                            int paddingWidth;
                            int paddingHeight;
                            if (outputImage.Width > outputImage.Height)
                            {
                                paddingHeight = outputImage.Height - (newBitmap.Height + (int)(outputImage.Height * 0.05));
                                paddingWidth = outputImage.Width - (newBitmap.Width + (int)(outputImage.Height * 0.05));
                            }
                            else
                            {
                                paddingWidth = outputImage.Width - (newBitmap.Width + (int)(outputImage.Width * 0.05));
                                paddingHeight = outputImage.Height - (newBitmap.Height + (int)(outputImage.Width * 0.05));
                            }
                            Rectangle destWaterRect = new Rectangle(paddingWidth, paddingHeight, outputImage.Width, outputImage.Height);
                            g.DrawImage(newBitmap, destWaterRect, 0, 0, outputImage.Width,
                                outputImage.Height, GraphicsUnit.Pixel);
                        }
                        outputImage.Save(newFile.SlashConverter());
                    }
                }
                return newFile;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                outputImage?.Dispose();
                g?.Dispose();
            }
        }

        #region Private

        private void Blur(string fileName, UploadType type, int announcementId = 0)
        {
            var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_blur{Path.GetExtension(fileName)}";
            var dest = FilesUtilities.GetRelativePath(fileName, type, announcementId);
            if (announcementId != 0)
                dest = $"{Path.GetDirectoryName(dest)}/{Path.GetFileName(dest)}";
            var newDest = FilesUtilities.GetRelativePathBlurs(newFileName, type, announcementId);
            if (announcementId != 0)
                newDest = $"{Path.GetDirectoryName(newDest)}/{Path.GetFileName(newDest)}";
            var theFile = Path.Combine(_webRootPath, dest);
            var newFile = Path.Combine(_webRootPath, newDest);
            var extension = Path.GetExtension(newFile);
            try
            {
                Path.GetDirectoryName(newFile).CreateDirectory();
                using (var stream = new FileStream(newFile, FileMode.Create))
                {
                    using (var image = SixImage.Load(theFile))
                    {
                        try
                        {
                            image.Mutate(x => x.GaussianBlur(Constants.BlurLevel).ApplyProcessors());

                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        if (string.IsNullOrEmpty(extension) || !extension.ToLower().Contains("png"))
                            image.SaveAsJpeg(stream);
                        else
                            image.SaveAsPng(stream);
                    }
                }
            }
            catch (Exception)
            {
                if (!File.Exists(newFile))
                    return;
                File.Delete(newFile);
            }
        }

        private void ConvertPdf(string fileName, UploadType type, int announcementId = 0)
        {
            var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.jpg";
            var dest = FilesUtilities.GetRelativePath(fileName, type, announcementId);
            if (announcementId != 0)
                dest = $"{Path.GetDirectoryName(dest)}/{Path.GetFileName(dest)}";
            var newDest = FilesUtilities.GetRelativePdfFile(newFileName, type, announcementId);
            var theFile = Path.Combine(_webRootPath, dest);
            var newFile = Path.Combine(_webRootPath, newDest);
            Path.GetDirectoryName(newFile).CreateDirectory();
            newFile = newFile.SlashConverter();
            DocumentCore document = DocumentCore.Load(theFile);
            DocumentPaginator dp = document.GetPaginator(new PaginatorOptions());
            var page = dp.Pages[0];
            using (var image = page.Rasterize(800, Color.White))
            {
                image.Save(newFile);
            }
        }

        private string Exif(string newFile, string fileName, UploadType uploadType, int announcementId = 0)
        {
            var newDest = fileName.GetDestFileName();
            newDest = FilesUtilities.GetRelativePath(newDest, uploadType, announcementId);
            var newFilePath = Path.Combine(_webRootPath, newDest);
            var exist = File.Exists(newFile);
            if (!exist)
                return null;
            using (var current = new Bitmap(newFile.SlashConverter()))
            {
                current.ExifRotate();
                using (var writeStream = new FileStream(newFilePath, FileMode.Create))
                {
                    var ext = Path.GetExtension(newFilePath);
                    if (string.IsNullOrEmpty(ext) || !ext.ToLower().Contains("png"))
                        current.Save(writeStream, ImageFormat.Jpeg);
                    else
                        current.Save(writeStream, ImageFormat.Png);
                }
            }
            return newFilePath;
        }

        private void GetThumbnail(string fileName, UploadType uploadType, int announcementId = 0)
        {
            var fn = Path.GetFileNameWithoutExtension(fileName);
            var imageDest = FilesUtilities.GetRelativePath(fn, uploadType, announcementId);
            var theFile = Path.Combine(_webRootPath, imageDest);
            var dest = FilesUtilities.GetRelativePath(fileName, uploadType, announcementId);
            var videoFile = Path.Combine(_webRootPath, dest);
            var thumbPath = Path.Combine($"{theFile}_thumb.jpg");
            videoFile = videoFile.SlashConverter();
            thumbPath = thumbPath.SlashConverter();
            var argument = $"-i \"{videoFile}\" -ss 00:00:00.001 -vframes 1 \"{thumbPath}\"";
            using (var process = argument.CreateProcess())
            {
                process.Start();
            }
        }

        private async Task Callback(string path, int announcementId, AttachmentType announcementPhotoType)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Constants.ApiBaseUrl);
                client.DefaultRequestHeaders.TryAddWithoutValidation("announcementId", announcementId.ToString());
                client.DefaultRequestHeaders.TryAddWithoutValidation("announcementPhotoType", announcementPhotoType.ToString());
                await client.PostAsync($"api/Announcement/Callback/{path}", null);
            }
        }

        private async Task Decrement(int announcementId)
        {
            using (var client = new HttpClient())
            {
                await client.GetAsync(new Uri($"{Constants.ApiBaseUrl}api/Announcement/DecrementCount/{announcementId}"));
            }
        }

        //public async Task<string> DownloadMap(DownloadMapModel model)
        public string DownloadMap(DownloadMapModel model)
        {
            var fileName = $"announcementMap{model.AnnouncementId}.jpg";
            var dest = fileName.GetDestFileName();
            dest = FilesUtilities.GetRelativePath(dest, UploadType.AnnouncementPhoto, model.AnnouncementId);
            var newFile = Path.Combine(_webRootPath, dest);
            newFile = newFile.SlashConverter();
            Path.GetDirectoryName(newFile).CreateDirectory();
            var webRequest = WebRequest.Create(model.Url);
            webRequest.Method = "GET";
            webRequest.ContentType = "image/png";
            var webResponse = webRequest.GetResponse();
            var stream = webResponse.GetResponseStream();
            using (var fileStream = new FileStream(newFile, FileMode.Create, FileAccess.Write))
            {
                stream?.CopyTo(fileStream);
            }
            //await Callback(Path.GetFileName(newFile), model.AnnouncementId, AttachmentType.OtherImages);

            return !model.IsRelativeRequested ? Path.GetFileName(dest) : dest;
        }
    }
    #endregion
}
