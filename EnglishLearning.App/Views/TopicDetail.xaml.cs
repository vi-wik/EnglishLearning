using EnglishLearning.Business;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;

namespace EnglishLearning.App.Views;

public partial class TopicDetail : ContentPage
{
    private EnglishTopic topic;
    public EnglishTopic Topic
    {
        get => topic;
    }

    public TopicDetail()
    {
        InitializeComponent();
    }

    public TopicDetail(EnglishTopic topic)
    {
        InitializeComponent();

        this.topic = topic;

        if (this.picker.Items.Count > 0)
        {
            this.picker.SelectedIndex = 0;
        }       

        this.ShowTopicDetails();
    }

    private void tbiShowSearchControl_Clicked(object sender, EventArgs e)
    {
        bool isVisible = this.txtKeyword.IsVisible;

        this.picker.IsVisible = this.txtKeyword.IsVisible = this.btnSearch.IsVisible = !isVisible;
    }

    private void OnSearchButtonClicked(object sender, EventArgs e)
    {
        this.Search();
    }

    private void txtKeyword_Completed(object sender, EventArgs e)
    {
        this.Search();
    }

    private async void ShowTopicDetails()
    {
        if (this.topic == null)
        {
            return;
        }        

        this.Title = this.topic.Name;

        this.Search();
    }  

    private async void Search()
    {
        string keyword = this.txtKeyword.Text?.Trim();

        string filterBy = this.picker.SelectedItem?.ToString();

        bool isFilterByBoth = filterBy == "二者都";

        bool isFilterByTopic = filterBy == "按栏位" || isFilterByBoth;
        bool isFilterByTitle = filterBy == "按标题" || isFilterByBoth;

        var topicDetails = await DataProcessor.GetEnglishTopicDetails(this.topic.Id, isFilterByTopic?  keyword: null);

        var topicDetailMedias = await DataProcessor.GetVEnglishTopicDetailMedias(this.topic.Id, isFilterByTitle? keyword: null);

        List<EnglishTopicDetailMediaGroup> groups = new List<EnglishTopicDetailMediaGroup>();

        foreach (var topicDetail in topicDetails)
        {
            var medias = topicDetailMedias.Where(item => item.TopicDetailId == topicDetail.Id).ToList();

            if (medias.Count > 0)
            {
                EnglishTopicDetailMediaGroup group = new EnglishTopicDetailMediaGroup(topicDetail.Name, medias) { Description = topicDetail.Description };

                groups.Add(group);
            }
        }

        this.lvMedias.ItemsSource = groups;
    }

    private void TapGestureRecognizer_MediaTapped(object sender, TappedEventArgs e)
    {
        this.ShowMediaPlayer((sender as View).BindingContext as V_EnglishMedia);
    }

    private async void ShowMediaPlayer(V_EnglishMedia media)
    {
        MediaPlayer player = (MediaPlayer)Activator.CreateInstance(typeof(MediaPlayer), media);

        await Navigation.PushAsync(player);
    }
}