using EnglishLearning.App.Model;
using EnglishLearning.Business;
using EnglishLearning.Business.Manager;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using EnglishLearning.Utility;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System.Text.RegularExpressions;
using Grid = Microsoft.Maui.Controls.Grid;

namespace EnglishLearning.App.Views;

public partial class PhraseList : ContentPage
{
    private SettingInfo setting = SettingManager.GetSetting();
    private string currentAlphabet = string.Empty;
    private IEnumerable<string> alphabets = null;

    public PhraseList()
    {
        InitializeComponent();

        this.Init();
    }

    private async void Init()
    {
        this.alphabets = await DataProcessor.GetEnglishPhraseAlphabets();

        this.lvPhraseAlphabet.ItemsSource = this.alphabets.Select(item => new { Alphabet = item });
        this.lvPhraseAlphabet.Loaded += this.LvPhraseAlphabet_Loaded;
    }

    private void LvPhraseAlphabet_Loaded(object? sender, EventArgs e)
    {
        if (this.alphabets != null && this.alphabets.Count() > 0)
        {
            this.ShowPhrasesByAlphabet(this.alphabets.First());
        }
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
        this.SetAlphabetButtonTextColor(null);

        string keyword = this.txtKeyword.Text?.Trim() ?? "";

        if (keyword.Length > 0)
        {
            char firstChar = keyword.First();

            bool isChinese = StringHelper.IsChineseChar(firstChar);

            EnglishWordFilter filter = new EnglishWordFilter() { Keyword = keyword, FullMatch = fullMatch, NeedMeaning = this.setting.ShowWordMeaningWhenShowWordList };

            var phrases = await DataProcessor.GetEnglishPhrases(filter, isChinese);

            List<EnglishPhrase> sortedPhrases = new List<EnglishPhrase>();

            List<int> fullMatchPhraseIds = new List<int>();
            List<int> fuzzyMatchPhraseIds = new List<int>();

            foreach (var p in phrases)
            {
                string phrase = p.Phrase;
                string meaning = p.Meaning;
                string synonym = p.Synonym;
                string abbreviation = p.Abbreviation;

                var phraseItems = phrase.Split(' ', '-');
                var meaningItems = meaning?.Split('£»', '£¬');
                var synonymItetms = synonym?.Split(';');

                if (!isChinese)
                {
                    if (phrase.ToLower() == keyword.ToLower()
                        || (abbreviation != null && abbreviation == keyword.ToLower())
                        || (synonymItetms != null && synonymItetms.Any(item => item.ToLower() == keyword.ToLower())))
                    {
                        fullMatchPhraseIds.Add(p.Id);
                    }
                    else if (phraseItems.Any(item => item.ToLower() == keyword.ToLower())
                        || (synonymItetms != null && synonymItetms.Any(item => item.Split(' ').Any(t => t.ToLower() == keyword.ToLower()))))
                    {
                        fuzzyMatchPhraseIds.Add(p.Id);
                    }
                }
                else
                {
                    if (meaningItems != null)
                    {
                        if (meaningItems.Contains(keyword))
                        {
                            fullMatchPhraseIds.Add(p.Id);
                        }
                        else if (meaningItems.Any(item => item.Contains(keyword)))
                        {
                            fuzzyMatchPhraseIds.Add(p.Id);
                        }
                    }
                }
            }

            sortedPhrases = phrases.Where(item => fullMatchPhraseIds.Any(t => t == item.Id)).OrderBy(item => item.Phrase.ToLower())
                 .Concat(phrases.Where(item => fuzzyMatchPhraseIds.Any(t => t == item.Id)).OrderBy(item => item.Phrase.ToLower()))
                 .Concat(phrases.Where(item => !fullMatchPhraseIds.Any(t => t == item.Id) && !fuzzyMatchPhraseIds.Any(t => t == item.Id)).OrderBy(item => item.Phrase.ToLower()))
                 .ToList();

            this.lvPhrase.ItemsSource = sortedPhrases;
        }
        else
        {
            this.lvPhrase.ItemsSource = null;
        }
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

        var phrase = grid.BindingContext as EnglishPhrase;

        if (phrase != null)
        {
            PhraseDetail phraseDetail = (PhraseDetail)Activator.CreateInstance(typeof(PhraseDetail), phrase.Id);

            await Navigation.PushAsync(phraseDetail);
        }
    }

    private void btnAlphabet_Clicked(object sender, EventArgs e)
    {
        Button btn = sender as Button;

        this.ShowPhrasesByAlphabet(btn.Text);
    }

    private async void ShowPhrasesByAlphabet(string alphabet)
    {
        this.txtKeyword.Text = "";

        this.SetAlphabetButtonTextColor(alphabet);

        var pharase = await DataProcessor.GetEnglishPhrasesByAlphabet(alphabet);

        this.lvPhrase.ItemsSource = pharase;
    }


    private void SetAlphabetButtonTextColor(string currentAlphabet)
    {
        var controls = this.lvPhraseAlphabet.GetVisualTreeDescendants();

        foreach (var control in controls)
        {
            if (control is Button btn)
            {
                if (btn.Text != currentAlphabet)
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