using EnglishLearning.Business;
using EnglishLearning.Business.Helper;
using EnglishLearning.Model;
using zoft.MauiExtensions.Core.Extensions;
namespace EnglishLearning.App.Views;

public partial class WordStatistic : ContentPage
{
    private string prefixTagName = "前缀";
    private string suffixTagName = "后缀";

    public WordStatistic()
    {
        InitializeComponent();

        this.Init();
    }

    private void Init()
    {
        this.picker.Items.AddRange(new string[] { this.prefixTagName, this.suffixTagName });
        this.picker.SelectedIndex = 0;
    }

    private void txtKeyword_Completed(object sender, EventArgs e)
    {
        this.Search(null, true);
    }

    private void OnSearchButtonClicked(object sender, EventArgs e)
    {
        this.Search(null, true);
    }

    private async void AutoCompleteEntry_TextChanged(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntryTextChangedEventArgs e)
    {
        string keyword = this.txtKeyword.Text.Trim();

        if (keyword.Length >= 1)
        {
            IEnumerable<EnglishWordAffix> affixes = null;

            if(this.IsFilterByPrefix())
            {
                affixes = await DataProcessor.GetEnglishWordPrefixSuggestions(keyword);
            }
            else if(this.IsFilterBySuffix())
            {
                affixes = await DataProcessor.GetEnglishWordSuffixSuggestions(keyword);
            }

            this.txtKeyword.ItemsSource = affixes.ToList();
        }
        else
        {
            this.txtKeyword.ItemsSource = null;
            this.lvWord.ItemsSource = null;
            this.lvWordContent.ItemsSource = null;
        }
    }

    private bool IsFilterByPrefix()
    {
        return this.picker.SelectedItem?.ToString() == this.prefixTagName;
    }

    private bool IsFilterBySuffix()
    {
        return this.picker.SelectedItem?.ToString() == this.suffixTagName;
    }

    private void txtKeyword_SuggestionChosen(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntrySuggestionChosenEventArgs e)
    {
        object selectedItem = e.SelectedItem;

        if (selectedItem != null && selectedItem is EnglishWordAffix prefix)
        {
            this.Search(prefix.Name, true);
        }
    }

    private async void Search(string keyword = null, bool showWordContent = false)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            keyword = this.txtKeyword.Text?.Trim();
        }

        if (string.IsNullOrEmpty(keyword))
        {
            this.lvWord.ItemsSource = null;
            this.lvWordContent.ItemsSource = null;
            return;
        }

        bool isFilterByPrefix = this.IsFilterByPrefix();
        bool isFilterBySuffix = this.IsFilterBySuffix();

        EnglishWordAffix affix = null;
        
        if(isFilterByPrefix)
        {
            affix = await DataProcessor.GetEnglishWordPrefixByName(keyword);
        }
        else
        {
            affix = await DataProcessor.GetEnglishWordSuffixByName(keyword);
        }

        if (affix == null)
        {
            this.lvWord.ItemsSource = null;
            this.lvWordContent.ItemsSource = null;
            return;
        }

        EnglishWordFilter filter = new EnglishWordFilter() { Keyword = keyword, NeedMeaning = true, NoLimit = true, MustHaveMeaning = true };

        filter.IsMatchPrefix = this.IsFilterByPrefix();
        filter.IsMatchSuffix = this.IsFilterBySuffix();

        if (!string.IsNullOrEmpty(keyword))
        {
            if (isFilterByPrefix)
            {
                filter.IgnoreCase = false;               

                if (affix.ExcludeName != null)
                {
                    filter.NotBeginWith = affix.ExcludeName;
                }
            }
            else if(isFilterBySuffix)
            {
                if (affix.ExcludeName != null)
                {
                    filter.NotEndWith = affix.ExcludeName;
                }
            }    
        }

        List<int> fullMatchWordIds = new List<int>();
        List<int> fuzzyMatchWordIds = new List<int>();

        var words = (await DataProcessor.GetEnglishWords(filter));

        fullMatchWordIds.AddRange(words.Where(item => item.Word.ToLower() == keyword.ToLower()).Select(item => item.Id));
        fuzzyMatchWordIds.AddRange(words.Where(item => item.Word != keyword).Select(item => item.Id));

        this.lvWord.ItemsSource = words.Where(item => fullMatchWordIds.Contains(item.Id)).OrderBy(item => item.ExamTypeOrder).ThenBy(item => item.Word.ToLower())
            .Concat(words.Where(item => fuzzyMatchWordIds.Contains(item.Id)).OrderBy(item => item.ExamTypeOrder).ThenBy(item => item.Word.ToLower()));

        this.ShowWordCount(words.Count());

        if (showWordContent)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                if (isFilterByPrefix)
                {
                    var contents = (await DataProcessor.GetEnglishWordPrefixDetailsByAffixName(keyword));

                    this.lvWordContent.ItemsSource = this.AppendOtherContent(contents);
                }
                else if(isFilterBySuffix)
                {
                    var contents = (await DataProcessor.GetEnglishWordSuffixDetailsByAffixName(keyword));

                    this.lvWordContent.ItemsSource = this.AppendOtherContent(contents);
                }
            }
            else
            {
                this.lvWordContent.ItemsSource = null;
            }
        }
    }

    private void ShowWordCount(int count)
    {
        this.lblWordCount.Text = $"共{count}条记录";
    }

    private IEnumerable<EnglishWordAffixDetail> AppendOtherContent(IEnumerable<EnglishWordAffixDetail> details)
    {
        return details.Append(new EnglishWordAffixDetail() { Id = -1, Content = "其他" });
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Grid grid = sender as Grid;

        var word = grid.BindingContext as V_EnglishWordWithMeaning;

        if (word != null)
        {
            WordDetail wordDetail = (WordDetail)Activator.CreateInstance(typeof(WordDetail), word.Id);

            await Navigation.PushAsync(wordDetail);
        }
    }

    private void btnWordContent_Clicked(object sender, EventArgs e)
    {
        Button btn = sender as Button;

        this.ShowWordsByContent(btn.BindingContext as EnglishWordAffixDetail);
    }

    private async void ShowWordsByContent(EnglishWordAffixDetail detail)
    {
        if (detail == null)
        {
            return;
        }

        string keyword = this.txtKeyword.Text?.Trim();
        string content = detail.Content;
        bool isOthers = detail.Id == -1;

        if (!string.IsNullOrEmpty(keyword))
        {
            this.SetContentButtonTextColor(content);

            IEnumerable<V_EnglishWordMeaning> meanings = null;

            if(this.IsFilterByPrefix())
            {
                meanings = await DataProcessor.GetEnglishWordMeaningByPrefixDetail(detail, keyword);
            }
            else
            {
                meanings = await DataProcessor.GetEnglishWordMeaningBySuffixDetail(detail, keyword);
            }

            List<V_EnglishWordMeaning> meaningList = new List<V_EnglishWordMeaning>();

            if(!isOthers)
            {
                string cleanContent = content.Trim('.');

                foreach (var meaning in meanings)
                {
                    string value = meaning.Meaning;

                    var items = EnglishWordMeaningHelper.SplitValue(value);

                    bool matched = false;

                    if (content.StartsWith("..."))
                    {
                        if(value.EndsWith(cleanContent))
                        {
                            matched = true;
                        }
                    }
                    else if(content.EndsWith("..."))
                    {
                        if(value.StartsWith(cleanContent))
                        {
                            matched = true;
                        }
                    }
                    else if(content.Contains("..."))
                    {
                        var contentItems = content.Split("...");

                        if (value.StartsWith(contentItems[0]) && value.EndsWith(contentItems[1]))
                        {
                            matched = true;
                        }
                    }
                    else if(value.Contains(content))
                    {
                        matched = true;
                    }

                    if(matched)
                    {
                        meaningList.Add(meaning);
                    }
                }
            }
            else
            {
                meaningList.AddRange(meanings);
            }            

            var groups = (from item in meaningList group item by new { item.WordId, item.Word, item.ExamType } into gp select gp);

            List<V_EnglishWordWithMeaning> wordList = new List<V_EnglishWordWithMeaning>();

            foreach (var gp in groups)
            {
                V_EnglishWordWithMeaning wm = new V_EnglishWordWithMeaning() { Id = gp.Key.WordId, Word = gp.Key.Word, ExamType = gp.Key.ExamType };

                wm.CommonMeaning = string.Join("；", meaningList.Where(item => item.WordId == gp.Key.WordId).Select(item => item.Meaning));

                wordList.Add(wm);
            }

            this.lvWord.ItemsSource = wordList.OrderBy(item => item.ExamTypeOrder).ThenBy(item => item.Word);

            this.ShowWordCount(wordList.Count);
        }
    }

    private void SetContentButtonTextColor(string currentContent)
    {
        var controls = this.lvWordContent.GetVisualTreeDescendants();

        foreach (var control in controls)
        {
            if (control is Button btn)
            {
                if (btn.Text != currentContent)
                {
                    btn.TextColor = Colors.Black;
                }
                else
                {
                    btn.TextColor = Colors.Red;
                }
            }
        }
    }

    private void picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.txtKeyword.Text = "";
    }
}