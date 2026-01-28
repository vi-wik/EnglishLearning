using EnglishLearning.BLL.Core;
using EnglishLearning.BLL.Core.Model;
using EnglishLearning.BLL.MAUI.Manager;
using EnglishLearning.Model;
using EnglishLearning.Utility;
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

            if (!isChinese)
            {
                words = (await DataProcessor.GetEnglishWords(filter));

                List<int> fullMatchWordIds = new List<int>();
                List<int> fuzzyMatchWordIds = new List<int>();

                fullMatchWordIds.AddRange(words.Where(item => item.Word.ToLower() == keyword.ToLower()).Select(item=>item.Id));
                fuzzyMatchWordIds.AddRange(words.Where(item => item.Word.ToLower() != keyword.ToLower()).Select(item => item.Id));

                this.lvWord.ItemsSource = words.Where(item => fullMatchWordIds.Contains(item.Id)).OrderBy(item => item.ExamTypeOrder).ThenBy(item => item.Word.ToLower())
                    .Concat(words.Where(item => fuzzyMatchWordIds.Contains(item.Id)).OrderBy(item => item.ExamTypeOrder).ThenBy(item=>item.MeaningOrder).ThenBy(item => item.Word.ToLower()));
            }
            else
            {
                var meanings = await DataProcessor.GetEnglishWordMeanings(keyword);

                Dictionary<int,int> dictFullMatchWordId = new Dictionary<int, int>();
                Dictionary<int,int> dictFuzzyMatchWordId = new Dictionary<int, int>();

                foreach (var m in meanings)
                {
                    string commonMeaning = this.GetCleanMeaning(m.CommonMeaning);
                    string specialMeaning = this.GetCleanMeaning(m.SpecialMeaning);

                    var commonItems = this.GetMeaningItems(commonMeaning);
                    var specialItems = this.GetMeaningItems(specialMeaning);

                    bool fullMatchInCommon = false;
                    bool fullMatchInSpecial = false;

                    if (commonItems.Any(item => item == keyword))
                    {
                        fullMatchInCommon = true;
                    }
                    else if(specialItems.Any(item => item == keyword))
                    {
                        fullMatchInSpecial = true;
                    }

                    if(fullMatchInCommon || fullMatchInSpecial)
                    {
                        if (!dictFullMatchWordId.ContainsKey(m.WordId) && !dictFuzzyMatchWordId.ContainsKey(m.WordId))
                        {
                            dictFullMatchWordId.Add(m.WordId, (fullMatchInCommon? 1: 2));
                        }
                    }
                    else 
                    {
                        bool fuzzyMatchInCommon = false;
                        bool fuzzyMatchInSpecial = false;

                        if (commonItems.Any(item => item.Contains(keyword)))
                        {
                            fuzzyMatchInCommon = true;
                        }
                        else if(specialItems.Any(item => item.Contains(keyword)))
                        {
                            fuzzyMatchInSpecial = false;
                        }

                        if(fuzzyMatchInCommon || fuzzyMatchInSpecial)
                        {
                            if (!dictFullMatchWordId.ContainsKey(m.WordId) && !dictFuzzyMatchWordId.ContainsKey(m.WordId))
                            {
                                dictFuzzyMatchWordId.Add(m.WordId, fuzzyMatchInCommon? 1: 2);
                            }
                        }                      
                    }
                }

                var groups = (from item in meanings group item by new { item.WordId, item.Word, item.ExamType} into gp select gp);

                List<V_EnglishWordWithMeaning> wordList = new List<V_EnglishWordWithMeaning>();

                foreach (var gp in groups)
                {
                    int id = gp.Key.WordId;                    

                    V_EnglishWordWithMeaning wm = new V_EnglishWordWithMeaning() { Id = id, Word = gp.Key.Word, ExamType = gp.Key.ExamType };

                    var matchedMeanings = meanings.Where(item => item.WordId == id && (item.Meaning.Contains(keyword) || item?.SpecialMeaning?.Contains(keyword)==true));

                    if(matchedMeanings.Count()>0)
                    {
                        wm.CommonMeaning = string.Join("；", matchedMeanings.OrderBy(item => item.Priority).Select(item=>item.CommonMeaning));

                        if(matchedMeanings.Any(item=>item.SpecialMeaning?.Contains(keyword) == true) )
                        {
                            wm.SpecialMeaning = string.Join("；", matchedMeanings.OrderBy(item => item.Priority).Select(item => item.SpecialMeaning));
                        }                        

                        if (dictFullMatchWordId.ContainsKey(id))
                        {
                            wm.MeaningPriority = dictFullMatchWordId[id];
                        }

                        if (dictFuzzyMatchWordId.ContainsKey(id))
                        {
                            wm.MeaningPriority = dictFuzzyMatchWordId[id];
                        }

                        wordList.Add(wm);
                    }                   
                }    

                words = wordList.Where(item => dictFullMatchWordId.Keys.Any(t => t == item.Id)).OrderBy(item=>item.MeaningPriority).ThenBy(item => item.ExamTypeOrder).ThenBy(item=>item.ExamType).ThenBy(item => item.Word.ToLower())
                    .Concat(wordList.Where(item => dictFuzzyMatchWordId.Keys.Any(t => t == item.Id)).OrderBy(item=>item.MeaningPriority).ThenBy(item => item.ExamTypeOrder).ThenBy(item=>item.ExamType).ThenBy(item => item.Word.ToLower())
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

    private string[] GetMeaningItems(string meaning)
    {
        if(string.IsNullOrEmpty(meaning))
        {
            return Array.Empty<string>();
        }

        return meaning.Split('，', '；');
    }

    private string GetCleanMeaning(string meaning)
    {
        if (this.HasBracketChar(meaning))
        {
            Regex regex = new Regex(@"[\[<（][\w ；，、。]+[\]>）]");

            return  regex.Replace(meaning, "");
        }

        return meaning;
    }

    private bool HasBracketChar(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

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