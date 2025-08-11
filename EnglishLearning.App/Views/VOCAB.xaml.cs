using EnglishLearning.Business;
using EnglishLearning.Business.Manager;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using System.Collections.ObjectModel;
using zoft.MauiExtensions.Core.Extensions;

namespace EnglishLearning.App.Views;

public partial class VOCAB : ContentPage
{
    private V_VOCAB current;
    private string sortFieldName = "Name";
    private DataSortType sortType = DataSortType.ASC;
    private SettingInfo setting = SettingManager.GetSetting();
    private bool isFirstLoad = true;

    public VOCAB()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        this.setting = SettingManager.GetSetting();

        if (this.isFirstLoad)
        {
            this.LoadData();
            this.isFirstLoad = false;
        }
        else
        {
            bool hasCurrent = this.current != null;

            if (hasCurrent)
            {
                bool isValid = await DataProcessor.IsVOCAB(this.current.Id);

                if (!isValid)
                {
                    this.RemoveItemFromCollection(this.current.Id);
                }

                this.current = null;
            }
            else
            {
                int tableCount = await this.GetTableCount();
                int collectionCount = this.GetCollectionCount();

                if (tableCount != collectionCount)
                {
                    this.LoadData();
                }
            }
        }
    }

    private async void RemoveItemFromCollection(int id)
    {
        if (this.lvVOCAB.ItemsSource != null)
        {
            var collection = this.lvVOCAB.ItemsSource as ObservableCollection<V_VOCAB>;

            collection.Remove(collection.FirstOrDefault(item => item.Id == id));
        }
    }

    private int GetCollectionCount()
    {
        if (this.lvVOCAB.ItemsSource != null)
        {
            var collection = this.lvVOCAB.ItemsSource as ObservableCollection<V_VOCAB>;

            return collection.Count;
        }

        return 0;
    }

    private async Task<int> GetTableCount()
    {
        int count = await DataProcessor.GetVOCABCount();

        return count;
    }

    private async void LoadData()
    {
        this.Search(null, false);
    }

    private void OnSearchButtonClicked(object sender, EventArgs e)
    {
        this.DoSearch();
    }

    private async void DoSearch()
    {
        string keyword = this.txtKeyword.Text?.Trim();

        if (string.IsNullOrEmpty(keyword))
        {
            await DisplayAlert("提示", "请输入查询内容！", "确定");
            return;
        }

        this.Search();
    }

    private async void Search(string keyword = null, bool fullMatch = false)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            keyword = this.txtKeyword.Text?.Trim();
        }

        EnglishWordFilter filter = new EnglishWordFilter() { Keyword = keyword, FullMatch = fullMatch, NeedMeaning = this.setting.ShowWordMeaningWhenShowVOCABs };

        var vocabs = await DataProcessor.GetVVOCABs(filter, new DataSortInfo() { FieldName = this.sortFieldName, SortType = this.sortType });

        ObservableCollection<V_VOCAB> collection = new ObservableCollection<V_VOCAB>();

        collection.AddRange(vocabs);

        this.lvVOCAB.ItemsSource = collection;
    }

    private void txtKeyword_Completed(object sender, EventArgs e)
    {
        this.Search();
    }

    private async void AutoCompleteEntry_TextChanged(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntryTextChangedEventArgs e)
    {
        string keyword = this.txtKeyword.Text.Trim();

        if (keyword.Length > 2)
        {
            var words = await DataProcessor.GetVOCABSuggestions(keyword);

            this.txtKeyword.ItemsSource = words.ToList();
        }
        else
        {
            this.txtKeyword.ItemsSource = null;
            this.lvVOCAB.ItemsSource = null;
        }
    }

    private void txtKeyword_SuggestionChosen(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntrySuggestionChosenEventArgs e)
    {
        object selectedItem = e.SelectedItem;

        if (selectedItem != null && selectedItem is V_VOCAB)
        {
            this.Search((selectedItem as V_VOCAB).Name, true);
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Grid grid = sender as Grid;

        V_VOCAB v_VOCAB = grid.BindingContext as V_VOCAB;

        this.current = v_VOCAB;

        if (v_VOCAB != null)
        {
            if (v_VOCAB.Type == 1)
            {
                WordDetail wordDetail = (WordDetail)Activator.CreateInstance(typeof(WordDetail), v_VOCAB.WordId);

                await Navigation.PushAsync(wordDetail);
            }
            else
            {
                PhraseDetail phraseDetail = (PhraseDetail)Activator.CreateInstance(typeof(PhraseDetail), v_VOCAB.PhraseId);

                await Navigation.PushAsync(phraseDetail);
            }
        }
    }

    private void tbiRefresh_Clicked(object sender, EventArgs e)
    {
        this.Search(null, false);
    }

    private void tbiSortByLetterAsc_Clicked(object sender, EventArgs e)
    {
        this.sortFieldName = nameof(V_VOCAB.Name);
        this.sortType = DataSortType.ASC;

        this.Search(null, false);
    }

    private void tbiSortByLetterDesc_Clicked(object sender, EventArgs e)
    {
        this.sortFieldName = nameof(V_VOCAB.Name);
        this.sortType = DataSortType.DESC;

        this.Search(null, false);
    }

    private void tbiSortByCreateTimeAsc_Clicked(object sender, EventArgs e)
    {
        this.sortFieldName = nameof(V_VOCAB.CreateTime);
        this.sortType = DataSortType.ASC;

        this.Search(null, false);
    }

    private void tbiSortByCreateTimeDesc_Clicked(object sender, EventArgs e)
    {
        this.sortFieldName = nameof(V_VOCAB.CreateTime);
        this.sortType = DataSortType.DESC;

        this.Search(null, false);
    }

    private async void tbiManage_Clicked(object sender, EventArgs e)
    {
        VOCABManage manage = (VOCABManage)Activator.CreateInstance(typeof(VOCABManage));

        await Navigation.PushAsync(manage);
    }

    private async void SwipeItemRemove_Clicked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = sender as SwipeItem;

        V_VOCAB v_VOCAB = swipeItem.BindingContext as V_VOCAB;

        if (v_VOCAB != null)
        {
            bool success = await DataProcessor.DeleteVOCAB(v_VOCAB.Id);

            if (success)
            {
                this.RemoveItemFromCollection(v_VOCAB.Id);
            }
            else
            {
                await DisplayAlert("错误", "从生词本移除失败！", "确定");
            }
        }
    }
}