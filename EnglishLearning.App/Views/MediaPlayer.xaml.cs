using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using EnglishLearning.Business;
using EnglishLearning.Business.Helper;
using EnglishLearning.Model;
using EnglishLearning.Utility;
using Microsoft.Maui.Controls.Shapes;

#if ANDROID
using WebView = Android.Webkit.WebView;
#endif

namespace EnglishLearning.App.Views;

public partial class MediaPlayer : ContentPage
{
    private V_EnglishMedia media;
    private Dictionary<int, PlayTimeInfo> playTimes;
    private bool hasVideo = false;
    private int currentPlayTimeIndex = -1;
    private MediaFavorite favorite;

    private bool hasAutoPaused;
    private PopupOptions popupOptions = new PopupOptions() { Shadow = null, Shape = new RoundRectangle() { CornerRadius = new CornerRadius(0, 0, 0, 0) } };
    private DateTime? popupOpeningTime;
    private DateTime? popupClosedTime;
    private IDispatcherTimer timer;
    private bool isPlayed = false;

    public V_EnglishMedia Media
    {
        get => media;
    }

    public MediaPlayer(V_EnglishMedia media)
    {
        InitializeComponent();

        Connectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;

        this.media = media;
        this.timer = Dispatcher.CreateTimer();
        this.timer.Interval = TimeSpan.FromMilliseconds(100);     
        this.timer.Tick += this.Timer_Tick;

        this.InitPlayer();
    }
   

    private async void Current_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (!this.hasVideo)
        {
            if (NetworkHelper.IsConnectedToInternet())
            {
                await this.SetPlayerUrl();

                if (!this.timer.IsRunning)
                {
                    this.timer.Start();
                }
            }
        }
    }

    private async Task<bool> SetPlayerUrl()
    {
        if (!NetworkHelper.IsConnectedToInternet())
        {
            await DisplayAlert("提示", "未连接到网络！", "确定");
            return false;
        }

        string url = await MediaHelper.GetMediaSource(this.media);
        string poster = await MediaHelper.GetImageUrl(this.media);

        if (string.IsNullOrEmpty(url))
        {
            await DisplayAlert("提示", "无法获取媒体播放地址！", "确定");
            return false;
        }

        using (StreamReader sr = new StreamReader(await FileSystem.Current.OpenAppPackageFileAsync("html/video.html")))
        {
            string html = sr.ReadToEnd().Replace("$$SRC$$", url).Replace("$$POSTER$$", poster);

            this.player.Source = new HtmlWebViewSource { Html = html };
        }

        this.hasVideo = true;

        return true;
    }

    public async void InitPlayer()
    {
        if (this.media != null)
        {
            this.SetPlayTimes();

            this.lblTitle.Text = this.media.MediaTitle;   

            string description = string.IsNullOrEmpty(this.media.MediaDescriptionExt) ? this.media.MediaDescription : this.media.MediaDescriptionExt;

            this.lblDescription.Text = description;

            this.favorite = await DataProcessor.GetMediaFavoriteByMediaId(this.media.MediaId);

            bool success = await this.SetPlayerUrl();

            if(success)
            {
                this.timer.Start();
            }               
        }
        else
        {
            this.favorite = null;
        }

        this.SetStatusForFavorite(this.favorite != null);
    }

    private void SetStatusForFavorite(bool isAdded)
    {
        var fontImageSource = this.btnFavorite.Source as FontImageSource;

        fontImageSource.FontFamily = isAdded ? "FASolid" : "FARegular";
        fontImageSource.Color = isAdded ? Colors.Orange : Colors.Gray;
    }

    private void SetPlayTimes()
    {
        if (this.media != null && this.media.PlayTimes != null)
        {
            this.playTimes = new Dictionary<int, PlayTimeInfo>();

            int i = 0;

            foreach (var playTime in this.media.PlayTimes)
            {
                if (!string.IsNullOrEmpty(playTime.StartTime) && TimeSpan.TryParse(playTime.StartTime, out _))
                {
                    PlayTimeInfo playTimeInfo = new PlayTimeInfo();

                    playTimeInfo.StartTime = TimeSpan.Parse(playTime.StartTime);

                    if (!string.IsNullOrEmpty(playTime.EndTime) && TimeSpan.TryParse(playTime.EndTime, out _))
                    {
                        playTimeInfo.EndTime = TimeSpan.Parse(playTime.EndTime);
                    }

                    this.playTimes.Add(i, playTimeInfo);

                    i++;
                }
            }
        }
    }

    private async Task Play()
    {
        if (!this.hasVideo)
        {
            return;
        }

        TimeSpan? startTime = this.GetPlayStartTime();

        if (startTime.HasValue)
        {
            await this.SeekTo(startTime.Value);
            this.playTimes[0].HasSeeked = true;

            this.currentPlayTimeIndex = 0;
        }
        
        this.RecordHistory(startTime);

        await this.player.EvaluateJavaScriptAsync($"{this.GetPlayerJavascript()}.play();");
    }

    private string GetPlayerJavascript()
    {
        return "document.getElementById('player')";
    }

    private async Task SeekTo(TimeSpan timeSpan)
    {
        await this.player.EvaluateJavaScriptAsync($"{this.GetPlayerJavascript()}.currentTime={timeSpan.TotalSeconds};");
    }

    private bool HasPlayTimes()
    {
        return this.GetPlayStartTime().HasValue;
    }

    private TimeSpan? GetPlayStartTime()
    {
        TimeSpan? startTime = default(TimeSpan?);

        if (this.playTimes != null && this.playTimes.Count > 0)
        {
            startTime = this.playTimes[0].StartTime;
        }

        return startTime;
    }  


    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        if (this.popupOpeningTime.HasValue)
        {
            var seconds = (DateTime.Now - this.popupOpeningTime.Value).TotalSeconds;

            if (seconds < 1)
            {
                return;
            }
        }

        try
        {
            this.timer.Stop();
            this.Pause();          
        }
        catch(Exception ex) 
        {
        }

        base.OnNavigatedFrom(args);    
        
        var position = await this.GetCurrentPosition();

        if (position.HasValue)
        {
            this.RecordHistory(position.Value);
        }      

        try
        {
            this.player?.Handler?.DisconnectHandler();
        }
        catch (Exception ex)
        {
        }
    }

    private async Task<bool> IsLoaded()
    {
        try
        {
            var isLoaded = await this.player.EvaluateJavaScriptAsync("isLoaded");

            return isLoaded == "true";
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private async Task<TimeSpan?> GetCurrentPosition()
    {
        try
        {
            if(this.player.IsLoaded)
            {
                var currentTime = await this.player.EvaluateJavaScriptAsync($"{this.GetPlayerJavascript()}.currentTime;");

                if (float.TryParse(currentTime, out _))
                {
                    TimeSpan ts = TimeSpan.FromSeconds(float.Parse(currentTime));

                    return ts;
                }
            }           
        }
        catch (Exception)
        {
        }                   

        return default(TimeSpan?);
    }

    private async void RecordHistory(TimeSpan? position)
    {
        var history = new MediaAccessHistory() { MediaId = this.media.MediaId, PositionTime = DateTimeHelper.GetStandardTimeSpanString(position ?? TimeSpan.Zero) };

        try
        {
            await DataProcessor.RecordMediaAccessHistory(history);
        }
        catch (Exception ex)
        {
        }
    }


    private async void Timer_Tick(object? sender, EventArgs e)
    {
        if(!this.player.IsLoaded)
        {
            return;
        }

        if(!this.isPlayed)
        {
            bool isLoaded = await this.IsLoaded();

            if(isLoaded)
            {
                this.isPlayed = true;

                await this.Play();                
            }
        }     
        
        if(this.isPlayed)
        {
            bool needStop = false;

            if (!this.HasPlayTimes() || this.media.PlayTimes?.FirstOrDefault()?.EndTime == null)
            {
                this.timer.Stop();
                return;
            }              
        }

        if (!this.hasAutoPaused)
        {
            var position = await this.GetCurrentPosition();

            if (position.HasValue)
            {
                int currentPlayIndex = this.currentPlayTimeIndex;

                if (this.playTimes != null && this.playTimes.ContainsKey(currentPlayIndex))
                {
                    PlayTimeInfo playTimeInfo = this.playTimes[currentPlayIndex];

                    if (playTimeInfo.EndTime.HasValue && Math.Abs((playTimeInfo.EndTime.Value - position.Value).TotalMilliseconds) <= 150)
                    {
                        int nextPlayIndex = currentPlayIndex + 1;

                        if (this.playTimes.ContainsKey(nextPlayIndex))
                        {
                            this.SeekTo(this.playTimes[nextPlayIndex].StartTime);
                            this.playTimes[nextPlayIndex].HasSeeked = true;
                            this.currentPlayTimeIndex = nextPlayIndex;
                        }
                        else if (playTimeInfo.HasSeeked)
                        {
                            this.timer.Stop(); 
                            this.Pause();
                            this.hasAutoPaused = true;
                        }
                    }
                }
            }          
        }
    }

    private async void Pause()
    {
        if(this.player.IsLoaded && this.isPlayed)
        {
            await this.player.EvaluateJavaScriptAsync($"{this.GetPlayerJavascript()}.pause();");
        }        
    }

    private async void btnOpenInOtherApp_Clicked(object sender, EventArgs e)
    {
        try
        {
            this.Pause();

            var options = new BrowserLaunchOptions()
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred
            };

            string url = this.media.Url;

            if (MediaHelper.IsBilibiliMedia(this.media))
            {
                TimeSpan? startTime = this.GetPlayStartTime();

                if (startTime.HasValue)
                {
                    double seconds = startTime.Value.TotalSeconds;

                    Uri uri = new Uri(url);

                    string time = MediaHelper.GetQueryParameterValue(url, "t");

                    if (string.IsNullOrEmpty(time))
                    {
                        string query = uri.Query;

                        string connector = string.IsNullOrEmpty(query) ? "?" : "&";

                        url += $"{connector}t={seconds}";
                    }
                }
            }

            await Browser.Default.OpenAsync(url, options);
        }
        catch (Exception ex)
        {

        }
    }

    private async void btnFavorite_Clicked(object sender, EventArgs e)
    {
        if (this.favorite == null)
        {
            int? categoryId = default(int?);

            var categories = await DataProcessor.GetMediaFavoriteCategories();

            if (categories.Count() > 1)
            {
                var popup = new SelectListItem("选择收藏夹", categories.Select(item => new ListItemInfo() { Id = item.Id, Name = item.Name, IsSelected = !item.CanDelete }));

                popup.OnPromptConfirm += this.Popup_OnPromptConfirm;
                popup.Closed += this.Popup_Closed;

                this.popupOpeningTime = DateTime.Now;

                await this.ShowPopupAsync(popup, this.popupOptions);
            }
            else
            {
                categoryId = categories?.FirstOrDefault()?.Id;

                if (categoryId.HasValue)
                {
                    await this.AddMediaFavorite(categoryId.Value);
                }
            }
        }
        else
        {
            bool success = await DataProcessor.DeleteMediaFavorite(this.favorite.Id);

            if (success)
            {
                this.favorite = null;

                this.SetStatusForFavorite(false);

                //MessageHelper.ShowToastMessage("已取消收藏。");
            }
            else
            {
                await DisplayAlert("错误", "取消收藏失败！", "确定");
            }
        }
    }  

    private void Popup_Closed(object? sender, EventArgs e)
    {
        this.popupClosedTime = DateTime.Now;
    }

    private async Task<bool> Popup_OnPromptConfirm(int id)
    {
        return await this.AddMediaFavorite(id);
    }

    private async Task<bool> AddMediaFavorite(int categoryId)
    {
        bool success = await DataProcessor.AddMediaFavorite(this.media.MediaId, categoryId);

        if (success)
        {
            this.favorite = await DataProcessor.GetMediaFavoriteByMediaId(this.media.MediaId);

            this.SetStatusForFavorite(true);

            //MessageHelper.ShowToastMessage("收藏成功。");           
        }
        else
        {
            await DisplayAlert("错误", $"收藏失败！", "确定");
        }

        return success;
    }

    public class PlayTimeInfo
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool HasSeeked { get; set; }
    }

    private void player_Loaded(object sender, EventArgs e)
    {

#if ANDROID

        var view = sender as Microsoft.Maui.Controls.WebView;
        var handler = view.Handler;
        var webview = handler?.PlatformView as WebView;

        if (webview is not null)
        {
            webview.Settings.MediaPlaybackRequiresUserGesture = false;
        }
#endif
    }
}