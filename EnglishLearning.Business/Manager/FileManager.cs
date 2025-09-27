using EnglishLearning.Business.Helper;
namespace EnglishLearning.Business.Manager
{
    public class FileManager
    {
        internal static string DataRootFolder
        {
            get
            {
                return FileSystem.Current.AppDataDirectory;
            }
        }

        internal static string CacheRootFolder
        {
            get
            {
                return FileSystem.Current.CacheDirectory;
            }
        }
    }
}
