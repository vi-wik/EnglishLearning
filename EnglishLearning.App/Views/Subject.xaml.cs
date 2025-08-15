using EnglishLearning.Business;
using EnglishLearning.DataAccess;
using EnglishLearning.Model;
using System.Xml.Linq;

namespace EnglishLearning.App.Views;

public partial class Subject : ContentPage
{
    private int subjectId;
    private bool isDataLoaded = false;

    public Subject()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        var path = Shell.Current.CurrentState.Location.ToString().Trim('/');

        EnglishSubject subject = await DataProcessor.GetEnglishSubjectByEnName(path);

        this.subjectId = subject == null ? 0 : subject.Id;

        if (this.subjectId >= 1)
        {
            if (!this.isDataLoaded)
            { 
                this.LoadData();
            }
        }
    }

    private async void LoadData()
    {
        var subject = await DataProcessor.GetEnglishSubject(this.subjectId);

        if (subject == null)
        {
            return;
        }

        this.lblDescription.Text = subject.Description;
        this.lblDescription.IsVisible = !string.IsNullOrEmpty(subject.Description);

        var medias = await DataProcessor.GetVEnglishSubjectMedias(this.subjectId);

        this.lvMedias.ItemsSource = medias;

        this.lblIntroductionTitle.IsVisible = medias.Count() > 0;

        var topics = await DataProcessor.GetEnglishTopics(this.subjectId);

        this.lblDetailsTitle.IsVisible = topics.Count() > 0;

        this.lvTopics.ItemsSource = topics;

        this.isDataLoaded = true;
    }

    private void TapGestureRecognizer_TopicTapped(object sender, TappedEventArgs e)
    {
        var topic = (sender as View).BindingContext as EnglishTopic;

        this.ShowTopicDetails(topic);
    }

    private async void ShowTopicDetails(EnglishTopic topic)
    {
        TopicDetail topicDetail = (TopicDetail)Activator.CreateInstance(typeof(TopicDetail), topic);

        await Navigation.PushAsync(topicDetail);
    }
}