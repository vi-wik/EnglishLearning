using EnglishLearning.BLL.Core;
using EnglishLearning.BLL.Core.Helper;
using EnglishLearning.Model;

namespace EnglishLearning.App.Views;

public partial class WordAffixStatistic : ContentPage
{
    private string keyword;
    private EnglishWordElementType type;

    public WordAffixStatistic(EnglishWordElement element, EnglishWordElementType type)
    {
        InitializeComponent();

        this.keyword = element.Name;
        this.type = type;

        this.LoadData(element, type);
    }

    private async void LoadData(EnglishWordElement element, EnglishWordElementType type)
    {
        string keyword = element.Name;

        EnglishWordFilter filter = new EnglishWordFilter() { Keyword = keyword, NeedMeaning = true, NoLimit = true, MustHaveMeaning = true };

        filter.IsMatchPrefix = type == EnglishWordElementType.Prefix;
        filter.IsMatchSuffix = type == EnglishWordElementType.Suffix;

        if (!string.IsNullOrEmpty(keyword))
        {
            if (type == EnglishWordElementType.Prefix)
            {
                filter.IgnoreCase = false;

                if (element.ExcludeName != null)
                {
                    filter.NotBeginWith = element.ExcludeName;
                }
            }
            else if (type == EnglishWordElementType.Suffix)
            {
                if (element.ExcludeName != null)
                {
                    filter.NotEndWith = element.ExcludeName;
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

        if (!string.IsNullOrEmpty(keyword))
        {
            if (type == EnglishWordElementType.Prefix)
            {
                var contents = (await DataProcessor.GetEnglishWordPrefixStatisticsByAffixName(keyword));

                this.lvWordContent.ItemsSource = this.AppendOtherContent(contents);
            }
            else if (type == EnglishWordElementType.Suffix)
            {
                var contents = (await DataProcessor.GetEnglishWordSuffixStatisticsByAffixName(keyword));

                this.lvWordContent.ItemsSource = this.AppendOtherContent(contents);
            }
        }
        else
        {
            this.lvWordContent.ItemsSource = null;
        }
    }

    private void ShowWordCount(int count)
    {
        this.lblWordCount.Text = $"共{count}条记录";
    }

    private IEnumerable<EnglishWordAffixStatistic> AppendOtherContent(IEnumerable<EnglishWordAffixStatistic> details)
    {
        return details.Append(new EnglishWordAffixStatistic() { Id = -1, Content = "其他" });
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

        this.ShowWordsByContent(btn.BindingContext as EnglishWordAffixStatistic);
    }

    private async void ShowWordsByContent(EnglishWordAffixStatistic statistic)
    {
        if (statistic == null)
        {
            return;
        }
       
        string content = statistic.Content;
        bool isOthers = statistic.Id == -1;

        if (!string.IsNullOrEmpty(keyword))
        {
            this.SetContentButtonTextColor(content);

            IEnumerable<V_EnglishWordMeaning> meanings = null;

            if (this.type == EnglishWordElementType.Prefix)
            {
                meanings = await DataProcessor.GetEnglishWordMeaningByPrefixDetail(statistic, this.keyword);
            }
            else
            {
                meanings = await DataProcessor.GetEnglishWordMeaningBySuffixDetail(statistic, this.keyword);
            }

            List<V_EnglishWordMeaning> meaningList = new List<V_EnglishWordMeaning>();

            if (!isOthers)
            {
                string cleanContent = content.Trim('.');

                foreach (var meaning in meanings)
                {
                    string value = meaning.Meaning;

                    var items = EnglishWordMeaningHelper.SplitValue(value);

                    bool matched = false;

                    if (content.StartsWith("..."))
                    {
                        if (value.EndsWith(cleanContent))
                        {
                            matched = true;
                        }
                    }
                    else if (content.EndsWith("..."))
                    {
                        if (value.StartsWith(cleanContent))
                        {
                            matched = true;
                        }
                    }
                    else if (content.Contains("..."))
                    {
                        var contentItems = content.Split("...");

                        if (value.StartsWith(contentItems[0]) && value.EndsWith(contentItems[1]))
                        {
                            matched = true;
                        }
                    }
                    else if (value.Contains(content))
                    {
                        matched = true;
                    }

                    if (matched)
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

                wm.CommonMeaning = string.Join("£»", meaningList.Where(item => item.WordId == gp.Key.WordId).Select(item => item.Meaning));

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
}
