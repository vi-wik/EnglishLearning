using EnglishLearning.Business.Helper;
using EnglishLearning.Business.Manager;
using Plugin.Maui.Audio;
using System.Net;
using System.Text;

namespace EnglishLearning.App.Helper
{
    public class AudioPlayHelper
    {
        public static string wordAudioUrlFormat_Bd = "https://sensearch.baidu.com/gettts?lan={0}&spd=3&source=alading&text={1}";
        public static string wordAudioUrlFormat_Yd = "https://dict.youdao.com/dictvoice?audio={0}&type={1}";

        public static async void PlayEnglishWord(string word, bool isUS)
        {
            var setting = SettingManager.GetSetting();

            bool played = false;

            if (setting != null)
            {
                if ((await PermissionHelper.CheckPermission(PermissionType.Read)))
                {
                    string folder = setting.PronunciationFileRootFolder;

                    if (Directory.Exists(folder))
                    {
                        string subFolder = isUS ? "us" : "uk";

                        string cleanWord = word.Replace("-", "").Replace(" ", "");

                        string filePath = Path.Combine(folder, subFolder, $"{word}.mp3");

                        if (!File.Exists(filePath))
                        {
                            filePath = Path.Combine(folder, subFolder, $"{word}_.mp3");
                        }

                        if (File.Exists(filePath))
                        {
                            if (PlayLocalFile(filePath))
                            {
                                played = true;
                            }
                        }
                    }
                }               
            }

            if (!played)
            {
                string url = string.Format(wordAudioUrlFormat_Yd, word, isUS ? 2 : 1);

                bool success = await PlayNetworkFile(url);

                if (!success)
                {
                    url = string.Format(wordAudioUrlFormat_Bd, isUS ? "en" : "uk", word);

                    await PlayNetworkFile(url);
                }
            }
        }

        private static bool PlayLocalFile(string filePath)
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    var audioPlayer = AudioManager.Current.CreatePlayer(filePath);

                    audioPlayer.Play();                    
                }
                else
                {
                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        MemoryStream ms = new MemoryStream();

                        fs.CopyTo(ms);

                        var audioPlayer = AudioManager.Current.CreatePlayer(ms);

                        audioPlayer.Play();                       
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);

                return false;
            }
        }

        public static async Task<bool> PlayNetworkFile(string url)
        {
            if (!NetworkHelper.IsConnectedToInternet())
            {
                MessageHelper.ShowToastMessage("未连接到网络！", CommunityToolkit.Maui.Core.ToastDuration.Short);

                return false;
            }

            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    MemoryStream ms = new MemoryStream(await webClient.DownloadDataTaskAsync(url));

                    var audioPlayer = AudioManager.Current.CreatePlayer(ms);

                    audioPlayer.Play();

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);

                return false;
            }
        }
    }
}
