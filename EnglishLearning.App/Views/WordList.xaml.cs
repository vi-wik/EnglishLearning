using EnglishLearning.Business;
using EnglishLearning.Business.Manager;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using EnglishLearning.Utility;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EnglishLearning.App.Views;

public partial class WordList : ContentPage
{

    private SettingInfo setting = SettingManager.GetSetting();

    public WordList()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        this.setting = SettingManager.GetSetting();
    }


    private void txtKeyword_Completed(object sender, EventArgs e)
    {
        this.Search();
    }

    private async void Search(bool fullMatch = false)
    {
        string keyword = this.txtKeyword.Text?.Trim() ?? "";

        if (keyword.Length > 0)
        {
            char firstChar = keyword.First();

            bool isChinese = StringHelper.IsChineseChar(firstChar);

            IEnumerable<V_EnglishWordWithMeaning> words = null;

            EnglishWordFilter filter = new EnglishWordFilter() { Keyword = keyword, FullMatch = fullMatch, NeedMeaning = this.setting.ShowWordMeaningWhenShowWordList };

            List<int> fullMatchWordIds = new List<int>();
            List<int> fuzzyMatchWordIds = new List<int>();

            if (!isChinese)
            {
                words = (await DataProcessor.GetEnglishWords(filter));

                fullMatchWordIds.AddRange(words.Where(item => item.Word.ToLower() == keyword.ToLower()).Select(item=>item.Id));
                fuzzyMatchWordIds.AddRange(words.Where(item => item.Word != keyword).Select(item => item.Id));

                this.lvWord.ItemsSource = words.Where(item => fullMatchWordIds.Contains(item.Id)).OrderBy(item => item.ExamTypeOrder).ThenBy(item => item.Word.ToLower())
                    .Concat(words.Where(item => fuzzyMatchWordIds.Contains(item.Id)).OrderBy(item => item.ExamTypeOrder).ThenBy(item => item.Word.ToLower()));
            }
            else
            {
                var meanings = await DataProcessor.GetEnglishWordMeanings(keyword);               

                foreach (var m in meanings)
                {
                    string meaning = m.Meaning;                  

                    string content = meaning;

                    if(this.HasBracketChar(meaning))
                    {
                        Regex regex = new Regex(@"[\[<（][\w ；，]+[\]>）]");

                        content = regex.Replace(meaning, "");                       
                    }

                    var items = content.Split('，', '；');                   

                    if (items.Any(item => item == keyword))
                    {
                        if (!fullMatchWordIds.Contains(m.WordId) && !fuzzyMatchWordIds.Contains(m.WordId))
                        {
                            fullMatchWordIds.Add(m.WordId);
                        }
                    }
                    else if(items.Any(item=>item.Contains(keyword)))
                    {
                        if(!fullMatchWordIds.Contains(m.WordId) && !fuzzyMatchWordIds.Contains(m.WordId))
                        {
                            fuzzyMatchWordIds.Add(m.WordId);
                        }
                    }
                }

                var groups = (from item in meanings group item by new { item.WordId, item.Word, item.ExamType} into gp select gp);

                List<V_EnglishWordWithMeaning> wordList = new List<V_EnglishWordWithMeaning>();

                foreach (var gp in groups)
                {
                    V_EnglishWordWithMeaning wm = new V_EnglishWordWithMeaning() { Id = gp.Key.WordId, Word = gp.Key.Word, ExamType = gp.Key.ExamType };

                    wm.CommonMeaning = string.Join("；", meanings.Where(item => item.WordId == gp.Key.WordId && item.Meaning.Contains(keyword)).OrderBy(item => item.Priority).Select(item => item.CommonMeaning));

                    wordList.Add(wm);
                }

                words = wordList.Where(item => fullMatchWordIds.Any(t => t == item.Id)).OrderBy(item=>item.ExamTypeOrder).ThenBy(item=>item.Word.ToLower())
                    .Concat(wordList.Where(item => fuzzyMatchWordIds.Any(t => t == item.Id)).OrderBy(item => item.ExamTypeOrder).ThenBy(item => item.Word.ToLower())
                    ).ToList();

                this.lvWord.ItemsSource = words;
            }

            if (fullMatch && (words == null || words.Count() == 0))
            {
                await DisplayAlert("消息", "没找到任何记录", "确定");
            }
        }
        else
        {
            this.lvWord.ItemsSource = null;
        }
    }

    private bool HasBracketChar(string value)
    {
        return value.Contains('（') || value.Contains("<") || value.Contains('[');
    }

    private async void txtKeyword_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (this.setting.ShowWordsWhileInputing)
        {
            this.Search();
        }
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
}