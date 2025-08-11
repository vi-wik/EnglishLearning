using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace EnglishLearning.Business.Helper
{
    public class MediaHelper
    {
        public static bool IsBilibiliMedia(V_EnglishMedia media)
        {
            return media.PlatformId == (int)EnglishPlatformType.bilibili;
        }

        public static async Task<string> GetMediaSource(V_EnglishMedia media)
        {
            if (IsBilibiliMedia(media))
            {
                if (!string.IsNullOrEmpty(media.Source))
                {
                    string expirationTime = GetBilibiliVideoExpirationTime(media.Source);

                    if (!IsBilibiliVideoExpirated(expirationTime))
                    {
                        return media.Source;
                    }
                }

                var mediaExtraInfo = await DataProcessor.GetEnglishMediaExtraInfo(media.MediaId);

                if (mediaExtraInfo != null)
                {
                    using (WebClient webClient = new WebClient())
                    {
                        string mediaSource = null;

                        try
                        {
                            string detailsUrl = $"https://api.bilibili.com/x/player/playurl?avid={mediaExtraInfo.Aid}&cid={mediaExtraInfo.Cid}&qn=1&type=&otype=json&platform=html5&high_quality=0";
                            string result = webClient.DownloadString(detailsUrl);

                            var mediaDetailsInfo = JsonConvert.DeserializeObject<BilibiliMediaDetailsInfo>(result);

                            if(mediaDetailsInfo.data == null)
                            {
                                return null;
                            }

                            mediaSource = mediaDetailsInfo.data.durl[0].url.Replace(@"\u0026", "&");

                            media.Source = mediaSource;

                            await DataProcessor.UpdateMediaSource(media.MediaId, mediaSource);

                            return mediaSource;
                        }
                        catch (Exception ex)
                        {
                            return mediaSource;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (media.Source != null)
                {
                    return media.Source;
                }

                return media.Url;
            }
        }

        private static string GetBilibiliVideoExpirationTime(string url)
        {
            string value = GetQueryParameterValue(url, "deadline");

            if (!string.IsNullOrEmpty(value) && long.TryParse(value, out _))
            {
                DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(value) * 1000).DateTime.ToLocalTime();

                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return null;
        }

        public static string GetQueryParameterValue(string url, string parameterName)
        {
            Uri uri = new Uri(url);
            string query = uri.Query;

            NameValueCollection queryParameters = HttpUtility.ParseQueryString(query);

            string value = queryParameters[parameterName];

            return value;
        }

        private static bool IsBilibiliVideoExpirated(string datetime)
        {
            if (string.Compare(datetime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) <= 0)
            {
                return true;
            }

            return false;
        }
    }
}
