using EnglishLearning.BLL.Core;
using EnglishLearning.Model;
using zoft.MauiExtensions.Core.Extensions;
namespace EnglishLearning.App.Views;

public partial class WordRootAffixList : ContentPage
{
    private string prefixTagName = "Ç°×º";
    private string suffixTagName = "ºó×º";
    private string wordRootTagName = "´Ê¸ù";

    public WordRootAffixList()
    {
        InitializeComponent();

        this.Init();

        this.picker.SelectedIndex = 0;

        this.Search();
    }

    public WordRootAffixList(EnglishWordElementType type, string name)
    {
        InitializeComponent();

        this.Init();

        if (type == EnglishWordElementType.Prefix)
        {
            this.picker.SelectedIndex = 1;
        }
        else if (type == EnglishWordElementType.Suffix)
        {
            this.picker.SelectedIndex = 2;
        }
        else if (type == EnglishWordElementType.WordRoot)
        {
            this.picker.SelectedIndex = 0;
        }

        this.txtKeyword.Text = name;

        this.Search(true);
    }

    private void Init()
    {
        this.picker.Items.AddRange(new string[] { this.wordRootTagName, this.prefixTagName, this.suffixTagName });
    }

    private void txtKeyword_Completed(object sender, EventArgs e)
    {
        this.Search();
    }

    private void OnSearchButtonClicked(object sender, EventArgs e)
    {
        this.Search();
    }

    private bool IsFilterByPrefix()
    {
        return this.picker.SelectedItem?.ToString() == this.prefixTagName;
    }

    private bool IsFilterBySuffix()
    {
        return this.picker.SelectedItem?.ToString() == this.suffixTagName;
    }

    private bool IsFilterByWordRoot()
    {
        return this.picker.SelectedItem?.ToString() == this.wordRootTagName;
    }

    private async void Search(bool exactMatch = false)
    {
        string keyword = this.txtKeyword.Text.Trim();

        bool isFilterByPrefix = this.IsFilterByPrefix();
        bool isFilterBySuffix = this.IsFilterBySuffix();
        bool isFilterByWordRoot = this.IsFilterByWordRoot();

        IEnumerable<EnglishWordElement> elements = null;

        if (isFilterByPrefix)
        {
            elements = await DataProcessor.GetEnglishWordPrefixes(keyword, true);
        }
        else if (isFilterBySuffix)
        {
            elements = await DataProcessor.GetEnglishWordSuffixes(keyword, true);
        }
        else if (isFilterByWordRoot)
        {
            elements = await DataProcessor.GetEnglishWordRoots(keyword);

            var meanings = await DataProcessor.GetVEnglishWordRootMeanings(keyword);

            foreach (var element in elements)
            {
                int rootId = element.Id;

                var meaningItems = meanings.Where(item => item.RootId == rootId);

                element.Description = string.Join("£»", meaningItems.Select(item => item.FullMeaning));
            }
        }

        if (exactMatch)
        {
            this.lvElement.ItemsSource = elements.Where(item => item.Name == keyword);
        }
        else
        {
            this.lvElement.ItemsSource = elements.Where(item => item.Name == keyword).Concat(elements.Where(item => item.Name != keyword));
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Grid grid = sender as Grid;

        var element = grid.BindingContext as EnglishWordElement;

        if (element != null)
        {
            EnglishWordElementType type = EnglishWordElementType.None;

            if (this.IsFilterByPrefix())
            {
                type = EnglishWordElementType.Prefix;
            }
            else if (this.IsFilterBySuffix())
            {
                type = EnglishWordElementType.Suffix;
            }
            else if (this.IsFilterByWordRoot())
            {
                type = EnglishWordElementType.WordRoot;
            }

            if (!element.UseStatisticForDetail)
            {
                WordRootAffixDetail detail = (WordRootAffixDetail)Activator.CreateInstance(typeof(WordRootAffixDetail), element, type);

                await Navigation.PushAsync(detail);
            }
            else
            {
                WordAffixStatistic statistic = (WordAffixStatistic)Activator.CreateInstance(typeof(WordAffixStatistic), element, type);

                await Navigation.PushAsync(statistic);
            }
        }
    }

    private void picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.txtKeyword.Text = "";
        this.Search();
    }
}