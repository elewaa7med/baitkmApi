using Baitkm.Enums.Attachments;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace Baitkm.MediaServer.BLL.Helpers
{
    public static class FilesUtilities
    {
        public static string GetFileName(this string fileName)
        {
            fileName = Regex.Replace(fileName, @"\s+", "");
            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                fileName = fileName.Trim('"');
            if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                fileName = Path.GetFileName(fileName);
            return fileName;
        }

        public static string GetDestFileName(this string fileName)
        {
            fileName = fileName.GetFileName();
            return $"{Path.GetFileNameWithoutExtension(fileName)}-{DateTime.Now.Ticks}{Path.GetExtension(fileName)}";
        }

        public static string GetDestFileName(string name, UploadType uploadType)
        {
            var fileName = GetFileName(name);

            var fileNewName =
                $"{Path.GetFileNameWithoutExtension(fileName)}-{DateTime.Now.Ticks}{Path.GetExtension(fileName)}";
            return fileNewName;
        }

        public static void CreateDirectory(this string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string GetRelativePathBlurs(string fileName, UploadType uploadType, int id = 0)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;
            fileName = Path.GetFileName(fileName);
            switch (uploadType)
            {
                case UploadType.ProfilePhoto:
                    return $"{Constants.FileBaseFolder}{Constants.ProfilePhotoBlur}{fileName}";
                case UploadType.AnnouncementPhoto:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.AnnouncementPhotoBlur}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.AnnouncementPhotoBlur}{fileName}";
                case UploadType.AnnouncementBasePhoto:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.AnnouncementBasePhotoBlur}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.AnnouncementBasePhotoBlur}{fileName}";
                case UploadType.MessageFiles:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.MessageFilesBlur}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.MessageFilesBlur}{fileName}";
                case UploadType.HomePageCoverImage:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.HomePageCoverImageBlur}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.HomePageCoverImageBlur}{fileName}";
                case UploadType.SupportConversationFiles:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.SupportConversationFiles}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.SupportConversationFiles}{fileName}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(uploadType));
            }
        }

        public static string GetRelativePdfFile(string fileName, UploadType uploadType, int id = 0)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;
            fileName = Path.GetFileName(fileName);
            switch (uploadType)
            {
                case UploadType.AnnouncementDocument:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.AnnouncementDocumentImage}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.AnnouncementDocumentImage}{fileName}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(uploadType));
            }
        }

        public static string GetRelativePath(string fileName, UploadType uploadType, int id = 0, int announcementPhotoTypeId = 0)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;
            fileName = Path.GetFileName(fileName);
            switch (uploadType)
            {
                case UploadType.ProfilePhoto:
                    return $"{Constants.FileBaseFolder}{Constants.ProfilePhoto}{fileName}";
                case UploadType.AnnouncementPhoto:
                    if (id != 0)
                    {
                        if (announcementPhotoTypeId != 0)
                            return $"{Constants.FileBaseFolder}{Constants.AnnouncementPhoto}{id}{announcementPhotoTypeId}/{fileName}";
                        return $"{Constants.FileBaseFolder}{Constants.AnnouncementPhoto}{id}/{fileName}";
                    }
                    return $"{Constants.FileBaseFolder}{Constants.AnnouncementPhoto}{fileName}";
                case UploadType.AnnouncementBasePhoto:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.AnnouncementBasePhoto}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.AnnouncementBasePhoto}{fileName}";
                case UploadType.MessageFiles:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.MessageFiles}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.MessageFiles}{fileName}";
                case UploadType.HomePageCoverImage:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.HomePageCoverImage}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.HomePageCoverImage}{fileName}";
                case UploadType.AnnouncementDocument:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.AnnouncementDocument}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.AnnouncementDocument}{fileName}";
                case UploadType.SupportConversationFiles:
                    if (id != 0)
                        return $"{Constants.FileBaseFolder}{Constants.SupportConversationFiles}{id}/{fileName}";
                    return $"{Constants.FileBaseFolder}{Constants.SupportConversationFiles}{fileName}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(uploadType));
            }
        }

        public static string CacheKeyGenerator(this string fileName, UploadType type, bool isBlur, int width = 0, int height = 0)
        {
            if (width != 0 && height != 0)
                return $"Cache{fileName}/{type}/{width}/{height}/{isBlur}";
            return $"Cache{fileName}/{type}/{isBlur}";
        }

        public static string SlashConverter(this string value)
        {
            return value.Replace("/", "\\");
        }

        public static byte[] ToByteArray(this Bitmap current, string extension)
        {
            using (var stream = new MemoryStream())
            {
                if (string.IsNullOrEmpty(extension) || !extension.ToLower().Contains("png"))
                    current.Save(stream, ImageFormat.Jpeg);
                else
                    current.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static Process CreateProcess(this string argument)
        {
            var process = new Process
            {
                StartInfo =
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = @"C:\Program Files\FFMpeg\ffmpeg.exe",
                    Arguments = argument,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            return process;
        }
    }
}
