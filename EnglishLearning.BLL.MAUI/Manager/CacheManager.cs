using EnglishLearning.Model;
using EnglishLearning.Utility;

namespace EnglishLearning.BLL.MAUI.Manager
{
    public class CacheManager : FileManager
    {
        private static string imageFolderName => "image";

        public static string ImageCacheFolder => Path.Combine(CacheRootFolder, imageFolderName);

        static CacheManager()
        {
            if(!Directory.Exists(ImageCacheFolder))
            {
                Directory.CreateDirectory(ImageCacheFolder);
            }
        }

        public static string GetMediaImageCacheFilePath(V_EnglishMedia media)
        {
            string extension =!string.IsNullOrEmpty(media.ImageUrl) ? Path.GetExtension(media.ImageUrl) : ".jpg";

            string fileName = $"media_{media.MediaId}{extension}";

            return Path.Combine(ImageCacheFolder, fileName);
        }

        public static bool Clear()
        {
            try
            {
                var imageFolder = ImageCacheFolder;

                var files = Directory.GetFiles(imageFolder);

                foreach (var file in files)
                {
                    File.Delete(file);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.LogError(ExceptionHelper.GetExceptionDetails(ex));

                return false;
            }
        }
    }
}
