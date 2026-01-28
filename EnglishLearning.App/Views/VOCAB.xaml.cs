using EnglishLearning.BLL.Core;
using EnglishLearning.BLL.Core.Model;
using EnglishLearning.BLL.MAUI.Manager;
using EnglishLearning.Model;
using System.Collections.ObjectModel;
using zoft.MauiExtensions.Core.Extensions;

namespace EnglishLearning.App.Views;

public partial class VOCAB : ContentPage
{
    private string wordTagName = "单词";
    private string phraseTagName = "短语";
    private V_EnglishVOCAB current;
    private string sortFieldName = "Name";
    private DataSortType sortType = DataSortType.ASC;
    private SettingInfo setting = SettingManager.GetSetting();
    private bool isFirstLoad = true;

    public VOCAB()
    {
        InitializeComponent();

        this.Init();
    }

    private void Init()
    {
        this.picker.Items.AddRange(new string[] { this.wordTagName, this.phraseTagName });

        this.picker.SelectedIndex = 0;
    }

    private bool IsWordVOCAB()
    {
        return this.picker.SelectedIndex == 0;
    }

    private bool IsPhraseVOCAB()
    {
        return this.picker.SelectedIndex == 1;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        this.setting = SettingManager.GetSetting();

        bool isWordVOCAB = this.IsWordVOCAB();
        bool isPhraseVOCAB = this.IsPhraseVOCAB();

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
                bool isValid = isWordVOCAB ? await DataProcessor.IsEnglishWordVOCAB(this.current.Id) :
                    await DataProcessor.IsEnglishPhraseVOCAB(this.current.Id);

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

    private void picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.txtKeyword.Text = "";
        this.LoadData();
    }

    private async void RemoveItemFromCollection(int id)
    {
        if (this.lvVOCAB.ItemsSource != null)
        {
            var collection = this.lvVOCAB.ItemsSource as ObservableCollection<V_EnglishVOCAB>;

            collection.Remove(collection.FirstOrDefault(item => item.Id == id));
        }
    }

    private int GetCollectionCount()
    {
        if (this.lvVOCAB.ItemsSource != null)
        {
            var collection = this.lvVOCAB.ItemsSource as ObservableCollection<V_EnglishVOCAB>;

            return collection.Count;
        }

        return 0;
    }

    private async Task<int> GetTableCount()
    {
        int count = this.IsWordVOCAB()?  await DataProcessor.GetEnglishWordVOCABCount(): await DataProcessor.GetEnglishPhraseVOCABCount();

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

        IEnumerable<V_EnglishVOCAB> vocabs = this.IsWordVOCAB() ? await DataProcessor.GetVEnglishWordVOCABs(filter, new DataSortInfo() { FieldName = this.sortFieldName, SortType = this.sortType }) :
             await DataProcessor.GetVEnglishPhraseVOCABs(filter, new DataSortInfo() { FieldName = this.sortFieldName, SortType = this.sortType });

        ObservableCollection<V_EnglishVOCAB> collection = new ObservableCollection<V_EnglishVOCAB>();

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
            IEnumerable<V_EnglishVOCAB> results = this.IsWordVOCAB()? await DataProcessor.GetEnglishWordVOCABSuggestions(keyword):
                await DataProcessor.GetEnglishPhraseVOCABSuggestions(keyword);

            this.txtKeyword.ItemsSource = results.ToList();           
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

        if (selectedItem != null && selectedItem is V_EnglishVOCAB vocab)
        {
            this.Search(vocab.Name, true);
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Grid grid = sender as Grid;

        V_EnglishVOCAB v_VOCAB = grid.BindingContext as V_EnglishVOCAB;

        this.current = v_VOCAB;

        if (v_VOCAB != null)
        {
            if (v_VOCAB is V_EnglishWordVOCAB wordVOCAB)
            {
                WordDetail wordDetail = (WordDetail)Activator.CreateInstance(typeof(WordDetail), wordVOCAB.WordId);

                await Navigation.PushAsync(wordDetail);
            }
            else if(v_VOCAB is V_EnglishPhraseVOCAB phraseVOCAB)
            {
                PhraseDetail phraseDetail = (PhraseDetail)Activator.CreateInstance(typeof(PhraseDetail), phraseVOCAB.PhraseId);

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
        this.sortFieldName = nameof(V_EnglishVOCAB.Name);
        this.sortType = DataSortType.ASC;

        this.Search(null, false);
    }

    private void tbiSortByLetterDesc_Clicked(object sender, EventArgs e)
    {
        this.sortFieldName = nameof(V_EnglishVOCAB.Name);
        this.sortType = DataSortType.DESC;

        this.Search(null, false);
    }

    private void tbiSortByCreateTimeAsc_Clicked(object sender, EventArgs e)
    {
        this.sortFieldName = nameof(V_EnglishVOCAB.CreateTime);
        this.sortType = DataSortType.ASC;

        this.Search(null, false);
    }

    private void tbiSortByCreateTimeDesc_Clicked(object sender, EventArgs e)
    {
        this.sortFieldName = nameof(V_EnglishVOCAB.CreateTime);
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

        V_EnglishVOCAB v_VOCAB = swipeItem.BindingContext as V_EnglishVOCAB;

        if (v_VOCAB != null)
        {
            bool success = this.IsWordVOCAB()? await DataProcessor.DeleteEnglishWordVOCAB(v_VOCAB.Id):
                await DataProcessor.DeleteEnglishPhraseVOCAB(v_VOCAB.Id);

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