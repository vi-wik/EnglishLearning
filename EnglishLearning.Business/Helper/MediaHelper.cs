using EnglishLearning.Business.Manager;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EnglishLearning.Business.Helper
{
    public class MediaHelper
    {
        public static bool IsBilibiliMedia(V_EnglishMedia media)
        {
            return media.PlatformId == (int)EnglishPlatformType.bilibili;
        }

        public static bool IsImagehubImage(string url)
        {
            return url?.ToLower().Contains("imagehub.cc") == true;
        }

        public static async Task<ImageSource> GetImageSource(V_EnglishMedia media)
        {
            if (media == null)
            {
                return null;
            }

            string url = media.ImageUrl;

            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            try
            {
                string cacheFilePath = CacheManager.GetMediaImageCacheFilePath(media);

                if (File.Exists(cacheFilePath))
                {
                    return cacheFilePath;
                }

                using (var handler = new HttpClientHandler())
                {
                    var client = new HttpClient(handler);

                    if (IsImagehubImage(url))
                    {
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    }

                    var response = await client.GetAsync(url);

                    var data = await response.Content.ReadAsByteArrayAsync();

                    File.WriteAllBytes(cacheFilePath, data);

                    return cacheFilePath;                                  
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<string> GetImageUrl(V_EnglishMedia media)
        {
            ImageSource source = await GetImageSource(media);

            byte[] data = null;

            try
            {
                if (source is UriImageSource us)
                {
                    return us.Uri.ToString();
                }
                else if (source is FileImageSource fs)
                {
                    data = File.ReadAllBytes(fs.File);
                }
                else if (source is StreamImageSource ss)
                {
                    var func = ss.Stream; ;

                    using (Stream stream = await func(CancellationToken.None))
                    {
                        BinaryReader reader = new BinaryReader(stream);

                        data = reader.ReadBytes((int)stream.Length);
                    }
                }

                if (data != null)
                {
                    return $"data:image/jpeg;base64,{Convert.ToBase64String(data)}";
                }
            }
            catch (Exception ex)
            {
                                
            }          

            return null;
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

                            if (mediaDetailsInfo.data == null)
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
