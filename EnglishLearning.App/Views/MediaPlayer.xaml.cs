using CommunityToolkit.Maui.Views;
using EnglishLearning.App.Helper;
using EnglishLearning.Business;
using EnglishLearning.Business.Helper;
using EnglishLearning.DataAccess;
using EnglishLearning.Model;
using EnglishLearning.Utility;

namespace EnglishLearning.App.Views;

public partial class MediaPlayer : ContentPage
{
    private V_EnglishMedia media;
    private Dictionary<int, PlayTimeInfo> playTimes;
    private bool hasVideo = false;
    private int currentPlayTimeIndex = -1;
    private MediaFavorite favorite;

    private bool hasAutoPaused;

    public V_EnglishMedia Media
    {
        get => media;
    }

    public MediaPlayer()
    {
        InitializeComponent();
    }

    public MediaPlayer(V_EnglishMedia media)
    {
        InitializeComponent();

        Connectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;

        this.media = media;

        this.InitPlayer();
    }

    private async void Current_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (!this.hasVideo)
        {
            if (NetworkHelper.IsConnectedToInternet())
            {
                await this.SetPlayerUrl();
                this.Play();
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

        if (string.IsNullOrEmpty(url))
        {
            await DisplayAlert("提示", "无法获取媒体播放地址！", "确定");
            return false;
        }

        this.player.Source = url;
        this.hasVideo = true;

        return true;
    }

    public async void InitPlayer()
    {
        if (this.media != null)
        {
            this.SetPlayTimes();

            this.lblTitle.Text = this.media.MediaTitle;

            var width = AppShell.Current.Window.Width;

            this.player.WidthRequest = width;

            this.player.MetadataTitle = this.media.MediaTitle;
            this.player.MetadataArtist = this.media.TeacherName;           

            string description = string.IsNullOrEmpty(this.media.MediaDescriptionExt) ? this.media.MediaDescription : this.media.MediaDescriptionExt;

            this.lblDescription.Text = description;
            this.lblDescription.IsVisible = !string.IsNullOrEmpty(description);

            this.favorite = await DataProcessor.GetMediaFavoriteByMediaId(this.media.MediaId);

            await this.SetPlayerUrl();
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

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        this.Play();
    }

    private void Play()
    {
        if (!this.hasVideo)
        {
            return;
        }

        TimeSpan? startTime = this.GetPlayStartTime();

        if (startTime.HasValue)
        {
            this.SeekTo(startTime.Value);
            this.playTimes[0].HasSeeked = true;

            this.currentPlayTimeIndex = 0;
        }

        this.player.Play();

        this.RecordHistory(startTime);
    }

    private void SeekTo(TimeSpan timeSpan)
    {
        //MainThread.BeginInvokeOnMainThread(() =>
        //{
        this.player.SeekTo(TimeSpan.FromSeconds(timeSpan.TotalSeconds));
        //});
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

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        TimeSpan position = this.player.Position;

        this.player.Stop();

        this.RecordHistory(position);

        try
        {
            this.player?.Handler?.DisconnectHandler();
        }
        catch (Exception ex)
        {

        }
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

    private void player_PositionChanged(object sender, CommunityToolkit.Maui.Core.Primitives.MediaPositionChangedEventArgs e)
    {
        if (!this.hasAutoPaused)
        {
            int currentPlayIndex = this.currentPlayTimeIndex;

            if (this.playTimes != null && this.playTimes.ContainsKey(currentPlayIndex))
            {
                PlayTimeInfo playTimeInfo = this.playTimes[currentPlayIndex];

                if (playTimeInfo.EndTime.HasValue && Math.Abs((playTimeInfo.EndTime.Value - e.Position).TotalMilliseconds) <= 300)
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
                        this.player.Pause();
                        this.hasAutoPaused = true;
                    }
                }
            }
        }
    }

    private async void btnOpenInOtherApp_Clicked(object sender, EventArgs e)
    {
        try
        {
            this.player.Stop();

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

                object result = await this.ShowPopupAsync(popup);

                if (result == null)
                {
                    return;
                }

                categoryId = Convert.ToInt32(result);
            }
            else
            {
                categoryId = categories?.FirstOrDefault()?.Id;
            }

            if (!categoryId.HasValue)
            {
                await DisplayAlert("提示", "无任何收藏夹！", "确定");
                return;
            }

            bool success = await DataProcessor.AddMediaFavorite(this.media.MediaId, categoryId.Value);

            if (success)
            {
                this.favorite = await DbObjectsFetcher.GetMediaFavoriteByMediaId(this.media.MediaId);

                this.SetStatusForFavorite(true);

                MessageHelper.ShowToastMessage("收藏成功。");
            }
            else
            {
                await DisplayAlert("错误", $"收藏失败！", "确定");
            }
        }
        else
        {
            bool success = await DataProcessor.DeleteMediaFavorite(this.favorite.Id);

            if (success)
            {
                this.favorite = null;

                this.SetStatusForFavorite(false);

                MessageHelper.ShowToastMessage("已取消收藏。");
            }
            else
            {
                await DisplayAlert("错误", "取消收藏失败！", "确定");
            }
        }
    }

    public class PlayTimeInfo
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool HasSeeked { get; set; }
    }
}