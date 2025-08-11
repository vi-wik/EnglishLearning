using EnglishLearning.Business.Helper;
namespace EnglishLearning.Business.Manager
{
    public class FileManager
    {
        internal static string RootFolder
        {
            get
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    string folder = FolderHelper.ANDROID_APP_FOLDER;

                    if (Directory.Exists(folder))
                    {
                        return folder;
                    }
                }                

                return FileSystem.Current.AppDataDirectory;
            }
        }
    }
}
