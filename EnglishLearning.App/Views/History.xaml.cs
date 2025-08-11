using EnglishLearning.App.Controls;
using EnglishLearning.Business;
using EnglishLearning.Business.Manager;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using EnglishLearning.Utility;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using zoft.MauiExtensions.Core.Extensions;

namespace EnglishLearning.App.Views;

public partial class History : ContentPage, INotifyPropertyChanged
{
    public History()
    {
        InitializeComponent();

        this.refreshView.Command = this.RefreshCommand;

        this.LoadData();
    }

    private ICommand RefreshCommand
    {
        get
        {
            return new Command(() =>
        {
            this.refreshView.IsRefreshing = true;
            this.LoadData();
            this.refreshView.IsRefreshing = false;
        });
        }
    }

    private async void LoadData()
    {
        var histories = (await DataProcessor.GetVMediaAccessHistories()).OrderByDescending(item => item.LastAccessTime).ToList();

        var todayHistories = histories.Where(item => item.LastAccessTime.Date == DateTime.Today);
        var yesterdayHistories = histories.Where(item => item.LastAccessTime.Date == DateTime.Today.AddDays(-1));
        var earlierHistories = histories.Where(item => item.LastAccessTime.Date <= DateTime.Today.AddDays(-2));

        List<MediaAccessHistoryGroup> groups = new List<MediaAccessHistoryGroup>();

        if (todayHistories.Count() > 0)
        {
            string name = "����";

            var items = todayHistories.Select(item => this.CreateVEnglishMedia(item, name));

            ObservableCollection<EnglishMediaForEditing> collection = new ObservableCollection<EnglishMediaForEditing>();
            collection.AddRange(items);

            MediaAccessHistoryGroup group = new MediaAccessHistoryGroup(name, collection);

            groups.Add(group);
        }

        if (yesterdayHistories.Count() > 0)
        {
            string name = "����";

            var items = yesterdayHistories.Select(item => this.CreateVEnglishMedia(item, name));

            ObservableCollection<EnglishMediaForEditing> collection = new ObservableCollection<EnglishMediaForEditing>();
            collection.AddRange(items);

            MediaAccessHistoryGroup group = new MediaAccessHistoryGroup(name, collection);

            groups.Add(group);
        }

        if (earlierHistories.Count() > 0)
        {
            string name = "����";

            var items = earlierHistories.Select(item => this.CreateVEnglishMedia(item, null));

            ObservableCollection<EnglishMediaForEditing> collection = new ObservableCollection<EnglishMediaForEditing>();
            collection.AddRange(items);

            MediaAccessHistoryGroup group = new MediaAccessHistoryGroup(name, collection);

            groups.Add(group);
        }

        bool useGroup = groups.Count > 1;

        this.lvMedias.IsGrouped = useGroup;

        if (groups.Count == 1)
        {
            this.lvMedias.ItemsSource = groups[0];
        }
        else
        {
            this.lvMedias.ItemsSource = groups;
        }

        this.SetToolbarButtonStatus(histories.Count());
    }

    private void SetToolbarButtonStatus(int recordCount)
    {
        var tbiClear = this.ToolbarItems[0];
        var tbiManage = this.ToolbarItems[1];

        tbiClear.Text = recordCount == 0 ? "" : "���";
        tbiClear.IsEnabled = recordCount > 0;

        tbiManage.Text = recordCount == 0 ? "" : "����";
        tbiManage.IsEnabled = recordCount > 0;
    }

    private EnglishMediaForEditing CreateVEnglishMedia(V_MediaAccessHistory mediaAccessHistory, string name)
    {
        string playTime = mediaAccessHistory.PositionTime;

        var meida = new EnglishMediaForEditing()
        {
            MediaId = mediaAccessHistory.MediaId,
            MediaTitle = mediaAccessHistory.MediaTitle,
            ImageUrl = mediaAccessHistory.ImageUrl,
            PlatformId = mediaAccessHistory.PlatformId,
            Url = mediaAccessHistory.Url,
            Source = mediaAccessHistory.Source,
            MediaTypeName = this.GetDateTimeDisplayString(name, mediaAccessHistory.LastAccessTime)
        };

        if (!string.IsNullOrEmpty(mediaAccessHistory.Duration))
        {
            TimeSpan position = TimeSpan.Parse(mediaAccessHistory.PositionTime);
            TimeSpan duration = TimeSpan.Parse(mediaAccessHistory.Duration);

            meida.Progress = position.TotalSeconds / duration.TotalSeconds;

            if (position.TotalSeconds >= duration.TotalSeconds)
            {
                playTime = "00:00:00";
            }
        }

        meida.PlayTimes = new List<EnglishMediaPlayTime>() { new EnglishMediaPlayTime() { StartTime = playTime } };

        return meida;
    }

    private string GetDateTimeDisplayString(string date, DateTime dateTime)
    {
        return dateTime.ToString($"{(date ?? dateTime.ToString("MM-dd"))} HH:mm");
    }

    private void tbiManage_Clicked(object sender, EventArgs e)
    {
        this.SetControlStatus();
    }

    private void SetControlStatus()
    {
        var tbiClear = this.ToolbarItems[0];
        var tbiManage = this.ToolbarItems[1];

        bool isManage = tbiManage.Text == "����";

        this.actionGrid.IsVisible = isManage ? true : false;

        this.SetCheckboxesVisible(isManage);

        tbiManage.Text = isManage ? "���" : "����";

        tbiClear.Text = isManage ? "" : "���";
        tbiClear.IsEnabled = !isManage;
    }

    private void SetCheckboxesVisible(bool visible)
    {
        var dataSource = this.lvMedias.ItemsSource;

        if (dataSource is List<MediaAccessHistoryGroup>)
        {
            foreach (MediaAccessHistoryGroup group in dataSource)
            {
                foreach (var item in group)
                {
                    item.IsEditing = visible;
                }
            }
        }
        else
        {
            ObservableCollection<EnglishMediaForEditing> histories = dataSource as ObservableCollection<EnglishMediaForEditing>;

            foreach (var item in histories)
            {
                item.IsEditing = visible;
            }
        }
    }

    private void SetCheckboxesSelected(bool selected)
    {
        var controls = (this.lvMedias as View).GetVisualTreeDescendants();

        var dataSource = this.lvMedias.ItemsSource;

        if (dataSource is List<MediaAccessHistoryGroup>)
        {
            foreach (MediaAccessHistoryGroup group in dataSource)
            {
                foreach (var item in group)
                {
                    item.IsSelected = selected;
                }
            }
        }
        else
        {
            ObservableCollection<EnglishMediaForEditing> histories = dataSource as ObservableCollection<EnglishMediaForEditing>;

            foreach (var item in histories)
            {
                item.IsEditing = selected;
            }
        }
    }

    private void chkSelectAll_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        this.SetCheckboxesSelected(this.chkSelectAll.IsChecked);
    }

    private async void tbiClear_Clicked(object sender, EventArgs e)
    {
        bool confirmed = await DisplayAlert("ѯ��?", "ȷ��Ҫ�����ʷ��¼��", "��", "��");

        if (confirmed)
        {
            int affectedRows = await DataProcessor.ClearMediaAccessHistories();

            this.LoadData();

            await DisplayAlert("��Ϣ", $"��ɾ��{affectedRows}����ʷ��¼��", "ȷ��");
        }
    }

    private async void btnDelete_Clicked(object sender, EventArgs e)
    {
        List<int> mediaIds = new List<int>();

        int total = 0;

        var dataSource = this.lvMedias.ItemsSource;

        if (dataSource is List<MediaAccessHistoryGroup>)
        {
            foreach (MediaAccessHistoryGroup group in dataSource)
            {
                foreach (var item in group)
                {
                    if (item.IsSelected)
                    {
                        mediaIds.Add(item.MediaId);
                    }

                    total++;
                }
            }
        }
        else
        {
            ObservableCollection<EnglishMediaForEditing> histories = dataSource as ObservableCollection<EnglishMediaForEditing>;

            foreach (var item in histories)
            {
                if (item.IsSelected)
                {
                    mediaIds.Add(item.MediaId);
                }

                total++;
            }
        }

        if (mediaIds.Count == 0)
        {
            await DisplayAlert("��ʾ", "��ѡ��Ҫɾ���ļ�¼��", "ȷ��");
            return;
        }
        else
        {
            bool confirmed = await DisplayAlert("ѯ��?", "ȷ��Ҫɾ��ѡ�����ʷ��¼��", "��", "��");

            if (confirmed)
            {
                int affectedRows = 0;

                if (mediaIds.Count == total)
                {
                    affectedRows = await DataProcessor.ClearMediaAccessHistories();
                }
                else
                {
                    affectedRows = await DataProcessor.DeleteMediaAccessHistoriesByMediaIds(mediaIds);
                }

                this.LoadData();

                this.SetControlStatus();

                await DisplayAlert("��Ϣ", $"��ɾ��{affectedRows}����ʷ��¼��", "ȷ��");
            }
        }
    }

    private void lvMedias_ChildAdded(object sender, ElementEventArgs e)
    {
        var element = e.Element;

        if (element is SwipeView)
        {
            var controls = element.GetVisualTreeDescendants();

            foreach (var control in controls)
            {
                if (control is MediaCardView mediaCardView)
                {
                    mediaCardView.ShowProgressBar();
                }
            }
        }
    }

    private async void SwipeItemDelete_Clicked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = sender as SwipeItem;

        EnglishMediaForEditing media = swipeItem.BindingContext as EnglishMediaForEditing;

        try
        {
            int affectedRows = await DataProcessor.DeleteMediaAccessHistoriesByMediaIds(new List<int>() { media.MediaId });

            if (affectedRows > 0)
            {
                var dataSource = this.lvMedias.ItemsSource;

                if (dataSource is List<MediaAccessHistoryGroup>)
                {
                    foreach (MediaAccessHistoryGroup group in dataSource)
                    {
                        foreach (var item in group)
                        {
                            if (item.MediaId == media.MediaId)
                            {
                                group.Remove(item);
                            }
                        }
                    }
                }
                else
                {
                    ObservableCollection<EnglishMediaForEditing> histories = dataSource as ObservableCollection<EnglishMediaForEditing>;

                    foreach (var item in histories)
                    {
                        if (item.MediaId == media.MediaId)
                        {
                            histories.Remove(item);
                            break;
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("��Ϣ", $"ɾ��ʧ�ܣ�", "ȷ��");
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);

            await DisplayAlert("����", ex.Message, "ȷ��");
        }
    }
}