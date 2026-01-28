using System.Net.NetworkInformation;

namespace EnglishLearning.BLL.MAUI.Helper
{
    public class NetworkHelper
    {
        public static bool IsConnectedToInternet()
        {
            var current = Connectivity.Current;

            if (current.NetworkAccess == NetworkAccess.Internet)
            {
                return true;
            }
            else
            {
                return false; 
            }
        }
    }
}
