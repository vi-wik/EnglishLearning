using EnglishLearning.App.Views;
using EnglishLearning.Business;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using System.Globalization;
using MyMediaPlayer = EnglishLearning.App.Views.MediaPlayer;

namespace EnglishLearning.App.Controls;

public partial class MediaCardView : ContentView
{
    private DateTime? lastTapTime;

    public MediaCardView()
    {
        InitializeComponent();
    }

    private bool CanNavigateToMediaPlayer()
    {
        var bindingContext = this.BindingContext;

        if (bindingContext == null)
        {
            return false;
        }

        if (bindingContext is EnglishMediaForEditing mediaForEditing)
        {
            if (mediaForEditing.IsEditing)
            {
                return false;
            }
        }

        return true;
    }

    private void TapGestureRecognizer_MediaTapped(object sender, TappedEventArgs e)
    {
        if (this.CanNavigateToMediaPlayer())
        {
            DateTime now = DateTime.Now;

            if (this.lastTapTime.HasValue && (now - this.lastTapTime.Value).TotalMilliseconds < 1000)
            {
                return;
            }

            var currentPage = Shell.Current.CurrentPage;

            string currentPageName = currentPage.ToString();

            if (currentPageName.Contains(nameof(EnglishLearning.App.Views.MediaPlayer)))
            {
                return;
            }           

            this.lastTapTime = now;

            this.ShowMediaPlayer((sender as View).BindingContext as V_EnglishMedia);
        }
    }

    private async void ShowMediaPlayer(V_EnglishMedia media)
    {
        if (media is V_EnglishWordMedia wordMedia)
        {
            var playTimes = await DataProcessor.GetVEnglishWordMediaPlayTimes(wordMedia.Id);

            media.PlayTimes = playTimes.Select(item => item as EnglishMediaPlayTime).ToList();
        }
        else if (media is V_EnglishPhraseMedia phraseMedia)
        {
            var playTimes = await DataProcessor.GetVEnglishPhraseMediaPlayTimes(phraseMedia.Id);

            media.PlayTimes = playTimes.Select(item => item as EnglishMediaPlayTime).ToList();
        }
        else if (media is V_EnglishConsonantMedia consonantMedia)
        {
            var playTimes = await DataProcessor.GetVEnglishConsonantMediaPlayTimes(consonantMedia.Id);

            media.PlayTimes = playTimes.Select(item => item as EnglishMediaPlayTime).ToList();
        }
        else if (media is V_EnglishVowelMedia wowelMedia)
        {
            var playTimes = await DataProcessor.GetVEnglishVowelMediaPlayTimes(wowelMedia.Id);

            media.PlayTimes = playTimes.Select(item => item as EnglishMediaPlayTime).ToList();
        }
        else if (media is V_EnglishTopicDetailMedia topicDetailMedia)
        {
            var playTimes = await DataProcessor.GetVEnglishTopicDetailMediaPlayTimes(topicDetailMedia.Id);

            media.PlayTimes = playTimes.Select(item => item as EnglishMediaPlayTime).ToList();
        }

        MyMediaPlayer player = (MyMediaPlayer)Activator.CreateInstance(typeof(MyMediaPlayer), media);

        await Navigation.PushAsync(player);
    }

    public void ShowProgressBar()
    {
        this.pb.IsVisible = true;
        this.InvalidateLayout();
    }
}


