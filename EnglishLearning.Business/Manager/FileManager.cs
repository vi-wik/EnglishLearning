using EnglishLearning.Business.Helper;
namespace EnglishLearning.Business.Manager
{
    public class FileManager
    {
        internal static string RootFolder
        {
            get
            {
                return FileSystem.Current.AppDataDirectory;
            }
        }
    }
}
