using EnglishLearning.App.Helper;
using EnglishLearning.App.Model;
using EnglishLearning.Business;
using EnglishLearning.DataAccess;
using EnglishLearning.Model;
using System.Linq;
using Microsoft.Maui.Controls.Shapes;
using zoft.MauiExtensions.Core.Extensions;
using EnglishLearning.Business.Model;
using EnglishLearning.Business.Helper;
using EnglishLearning.Business.Manager;

namespace EnglishLearning.App.Views;

public partial class WordDetail : ContentPage
{
    private V_EnglishWord englishWord = null;
    private EnglishLearning.Model.VOCAB vocab;
    private SettingInfo setting;
    private List<string> partOfSpeeches = null;
    private bool hasSpecialMeaning = false;
    private int syllableCount;
    private List<int> historyWordIds = new List<int>();

    private EnglishExamType learningEnglishExamType { get; set; }

    public WordDetail(int wordId)
    {
        this.Init(wordId);
    }

    public WordDetail(int wordId, EnglishExamType learningEnglishExamType)
    {
        this.Init(wordId, learningEnglishExamType);
    }

    private void Init(int wordId, EnglishExamType learningEnglishExamType = null)
    {
        InitializeComponent();

        this.setting = SettingManager.GetSetting();

        this.learningEnglishExamType = learningEnglishExamType;

        if (learningEnglishExamType != null)
        {
            this.SetToolbarItemStatus(true);
        }
        else
        {
            this.scrollView.GestureRecognizers.Clear();
        }

        this.ShowWord(wordId);
    }

    private void SetToolbarItemStatus(bool enable)
    {
        FontImageSource fs = this.tbiFinishLearn.IconImageSource as FontImageSource;
        fs.Color = enable ? Colors.White : Colors.Transparent;

        this.tbiFinishLearn.IsEnabled = enable;
    }


    private async void ShowWord(int wordId)
    {
        bool hasMedias = false;

        this.englishWord = await DataProcessor.GetVEnglishWord(wordId);

        bool hasUSPronunciation = this.englishWord?.US_Pronunciation != null;
        bool hasUKPronunciation = this.englishWord?.UK_Pronunciation != null;
        bool hasUSAudio = true;
        bool hasUKAudio = true;

        this.lblUS_Pronunciation.IsVisible = hasUSPronunciation;
        this.lblUK_Pronunciation.IsVisible = hasUKPronunciation;

        this.btnUSPronunciation.IsVisible = hasUSAudio;
        this.btnUKPronunciation.IsVisible = hasUKAudio;

        this.USPronunciationLayout.IsVisible = this.lblUS.IsVisible = hasUSPronunciation || hasUSAudio;
        this.UKPronunciationLayout.IsVisible = this.lblUK.IsVisible = hasUKPronunciation || hasUKAudio;

        if (this.englishWord != null)
        {
            if (!this.historyWordIds.Contains(this.englishWord.Id))
            {
                this.historyWordIds.Add(this.englishWord.Id);
            }

            this.lblTitle.Text = this.englishWord.Word;

            this.SetPronunciationDisplayText(this.lblUS_Pronunciation, this.englishWord.US_Pronunciation);
            this.SetPronunciationDisplayText(this.lblUK_Pronunciation, this.englishWord.UK_Pronunciation);
            this.btnVOCAB.IsVisible = true;

            #region 考试类型
            int? examType = this.englishWord.ExamType;

            this.ExamTypeLayout.IsVisible = examType.HasValue;

            if (examType.HasValue)
            {
                var examTypes = await DataProcessor.GetEnglishExamTypes();

                foreach (EnglishExamType type in examTypes)
                {
                    int weight = type.Weight;

                    if ((weight & examType.Value) == weight)
                    {
                        Border border = new Border();
                        border.Stroke = Colors.Gray;
                        border.StrokeThickness = 1;
                        border.Margin = new Thickness(3, 0);
                        border.Padding = new Thickness(3, 3);
                        border.StrokeShape = new RoundRectangle
                        {
                            CornerRadius = new CornerRadius(4, 4, 4, 4)
                        };

                        Label label = new Label() { Text = type.Name };
                        label.TextColor = Colors.Gray;
                        label.FontSize = 10;

                        border.Content = label;

                        this.ExamTypeLayout.Children.Add(border);
                    }
                }
            }
            #endregion

            #region 含义

            var meaningStatistic = await DataProcessor.GetEnglishWordMeaningSpecialStatistic(wordId);

            if (meaningStatistic != null)
            {
                this.hasSpecialMeaning = meaningStatistic.HasSpecial;

                if (this.hasSpecialMeaning)
                {
                    if(!this.setting.ShowWordFullMeaning)
                    {
                        this.btnShowAllMeaning.IsVisible = true;
                    }
                    else
                    {
                        this.btnHideSpecalMeaning.IsVisible = true;
                    }    
                }              
            }
            else
            {
                this.hasSpecialMeaning = false;               
            }

            this.ShowMeanings(wordId, this.setting.ShowWordFullMeaning);

            #endregion           

            #region 音节
            var syllables = await DataProcessor.GetEnglishWordSyllables(wordId);
            int syllableCount = syllables.Count();

            this.syllableCount = syllableCount;

            if (syllableCount == 0)
            {
                syllableCount = 1;
            }

            if (syllableCount > 1)
            {
                this.lblSyllable.IsVisible = true;
                UIHelper.SetEnglishWordSyllableDisplayText(this.lblSyllable, this.englishWord.Word, syllables);
            }
            else
            {
                this.lblSyllable.Text = "";
                this.lblSyllable.IsVisible = false;
            }
            #endregion

            #region 变形

            this.ShowInflections(wordId);

            #endregion

            #region 媒体
            var medias = await DataProcessor.GetVEnglishWordMedias(this.englishWord.Id);

            this.lvMedias.ItemsSource = medias;
            hasMedias = medias.Count() > 0;
            this.lblIntroduction.IsVisible = hasMedias;
            #endregion

            #region 例句
            var examples = await DataProcessor.GetVEnglishWordExamples(this.englishWord.Id);

            List<EnglishExampleDisplay> exampleDisplays = new List<EnglishExampleDisplay>();

            int order = 1;

            foreach (var example in examples)
            {
                EnglishExampleDisplay display = new EnglishExampleDisplay();

                display.Order = $"{order}.";
                display.Example = $"{example.Example}{UIHelper.MakeupPunctuation(example.Example, true)}";
                display.Meaning = $"{example.Meaning}{UIHelper.MakeupPunctuation(example.Meaning, false)}";

                exampleDisplays.Add(display);

                order++;
            }

            this.lblExample.IsVisible = exampleDisplays.Count > 0;
            this.lvExamples.ItemsSource = exampleDisplays;

            #endregion

            #region 异体
            var variants = await DataProcessor.GetVEnglishWordVariants(wordId);

            this.lvVariants.ItemsSource = variants;
            this.lblVariant.IsVisible = variants.Count() > 0;

            #endregion


            this.btnVOCAB.IsVisible = true;

            this.vocab = await DbObjectsFetcher.GetVOCAB(EnglishObjectType.Word, this.englishWord.Id);
        }
        else
        {
            this.vocab = null;
            this.btnVOCAB.IsVisible = false;
            this.lblSyllable.IsVisible = false;
        }

        this.lblIntroduction.IsVisible = hasMedias;
        this.lvMedias.IsVisible = hasMedias;

        this.SetStatusForVOCAB(this.vocab != null);
    }

    private bool HasVerb()
    {
        return this.partOfSpeeches.Contains("v") || this.partOfSpeeches.Contains("vt") || this.partOfSpeeches.Contains("vi");
    }

    private bool HasAdj()
    {
        return this.partOfSpeeches.Contains("adj");
    }

    private bool HasAdv()
    {
        return this.partOfSpeeches.Contains("adv");
    }

    private bool HasNoun()
    {
        return this.partOfSpeeches.Contains("n") || this.partOfSpeeches.Contains("un");
    }

    private async void ShowMeanings(int wordId, bool showFullMeaning=false)
    {
        var meanings = await DataProcessor.GetEnglishWordMeanings(this.englishWord.Id, new EnglishWordMeaningFilter() { ShowSpecialMeaning = showFullMeaning });

        var displayMeanings = meanings.Select(item => new EnglishWordMeaningForDisplay()
        {
            Id = item.Id,
            PartOfSpeechId = item.PartOfSpeechId,
            PartOfSpeech = item.PartOfSpeech,
            Meaning = item.Meaning,
            IsFromWeb = item.IsFromWeb,
            IsOld = item.IsOld,
            Priority = item.Priority
        }).ToList();

        this.partOfSpeeches = meanings.Select(item => item.PartOfSpeech).ToList();

        int meaningCount = displayMeanings.Count;

        if (meaningCount == 0)
        {
            var wordInflections = await DataProcessor.GetVEnglishWordInflectionsByTargetWordId(wordId);

            List<EnglishWordMeaningForDisplay> virtualMeanings = new List<EnglishWordMeaningForDisplay>();

            Func<V_EnglishWordInflection, string> getMeaning = (inflection) =>
            {
                return $"{inflection.Word}的{inflection.TypeName}";
            };

            foreach (var wordInflection in wordInflections)
            {
                int typeId = wordInflection.TypeId;

                string partOfSpeech = null;
                string meaning = null;
                bool existing = false;

                if (typeId == 1)
                {
                    partOfSpeech = "n";
                    meaning = getMeaning(wordInflection);
                }
                else if (typeId == 2 || typeId == 3 || typeId == 4 || typeId == 5)
                {
                    partOfSpeech = "v";

                    var exitingItem = virtualMeanings.FirstOrDefault(item => item.PartOfSpeech == partOfSpeech && item.Word == wordInflection.Word);

                    if (exitingItem == null)
                    {
                        meaning = getMeaning(wordInflection);
                    }
                    else
                    {
                        existing = true;
                        exitingItem.Meaning += $"及{wordInflection.TypeName}";
                    }

                }
                else if (typeId == 6 || typeId == 7)
                {
                    meaning = getMeaning(wordInflection);
                }

                if (!existing)
                {
                    virtualMeanings.Add(new EnglishWordMeaningForDisplay() { Word = wordInflection.Word, PartOfSpeech = partOfSpeech, Meaning = meaning });
                }
            }

            displayMeanings = virtualMeanings;
        }

        bool hasFromWeb = false;
        bool hasAdj = false;
        bool hasAdv = false;

        foreach (var meaning in displayMeanings)
        {
            string partOfSpeech = meaning.PartOfSpeech;

            if (meaning.IsFromWeb)
            {
                meaning.PartOfSpeech = "[网络]";
                hasFromWeb = true;
            }

            if (partOfSpeech != null)
            {
                meaning.PartOfSpeech += ".";
            }            
        }

        object partOfSpeechColumnWidth = 40;

        if (meanings.Count() == 1)
        {
            partOfSpeechColumnWidth = new GridLength(0, GridUnitType.Auto);
        }
        else
        {
            partOfSpeechColumnWidth = hasFromWeb ? 50 : 40;
        }

        displayMeanings.ForEach(item => { item.PartOfSpeechColumnWidth = partOfSpeechColumnWidth; });

        this.lvMeanings.ItemsSource = displayMeanings;      
    }

    private async void ShowInflections(int wordId)
    {
        this.gvWordInflection.Clear();

        List<Label> labels = new List<Label>();

        Func<double> getLabelMargin = () =>
        {
            if (labels.Count > 0)
            {
                return 15;
            }

            return 0;
        };

        var inflectionTypes = await DataProcessor.GetEnglishWordInflectionTypes();
        var inflections = (await DataProcessor.GetVEnglishWordInflections(wordId)).ToList();

        Action<int, string> addInflection = (inflectonTypeId, targetWord) =>
        {
            var inflectionType = inflectionTypes.FirstOrDefault(item => item.Id == inflectonTypeId);

            inflections.Add(new V_EnglishWordInflection() { TypeId = inflectionType.Id, TypeName = inflectionType.Name_CN, TypePriority = inflectionType.Priority, TargetWord = targetWord });
        };

        if ((this.HasAdj() || this.HasAdv()) && this.englishWord.HasDegree == true)
        {
            string comparativeDegree = EnglishWordInflectionHelper.GetComparativeDegree(this.englishWord.Word);
            string superlativeDegree = EnglishWordInflectionHelper.GetSuperlativeDegree(this.englishWord.Word);

            int comparativeDegreeCount = inflections.Count(item => item.TypeId == 6);
            int superlativeDegreeCount = inflections.Count(item => item.TypeId == 7);

            if (this.syllableCount >= 1)
            {
                if (comparativeDegreeCount == 0 && this.syllableCount <= 2)
                {
                    addInflection(6, comparativeDegree);
                }

                if (this.syllableCount > 2)
                {
                    addInflection(6, "more " + this.englishWord.Word);
                }

                if (superlativeDegreeCount == 0 && this.syllableCount <= 2)
                {
                    addInflection(7, superlativeDegree);
                }

                if (this.syllableCount > 2)
                {
                    addInflection(7, "most " + this.englishWord.Word);
                }
            }
        }

        var groups = (from item in inflections group item by new { item.TypeId, item.TypeName, item.TypePriority } into gp select gp).OrderBy(item => item.Key.TypePriority); ;

        foreach (var group in groups)
        {
            int typeId = group.Key.TypeId;
            string typeName = group.Key.TypeName;

            if (typeId == 1)
            {
                if (!this.HasNoun())
                {
                    continue;
                }
            }
            else if (typeId == 2 || typeId == 3 || typeId == 4 || typeId == 5)
            {
                if (!this.HasVerb())
                {
                    continue;
                }
            }

            labels.Add(this.CreateWordInflectionTitleLabel(typeName, getLabelMargin()));

            var targetWords = inflections.Where(item => item.TypeId == typeId).OrderBy(item => item.Priority).Select(item => item.TargetWord);

            labels.Add(this.CreateWordInflectionValueLabel(string.Join("，", targetWords)));
        }

        if (labels.Count > 0)
        {
            this.gvWordInflection.IsVisible = true;

            int columnIndex = 0, rowIndex = 0;
            int count = 0;

            for (int i = 0; i < labels.Count; i++)
            {
                Label label = labels[i];

                this.gvWordInflection.Add(label, columnIndex, rowIndex);

                columnIndex++;

                if (columnIndex == 4)
                {
                    columnIndex = 0;
                }

                count++;

                if (count % 4 == 0)
                {
                    rowIndex++;
                }
            }
        }
        else
        {
            this.gvWordInflection.IsVisible = false;
        }
    }

    private void SetPronunciationDisplayText(Label label, string pronunciation)
    {
        int mode = this.setting.PronunciationBracketMode;

        string leftChar = mode == 1 ? "[" : "/";
        string rightChar = mode == 1 ? "]" : "/";

        label.FormattedText = new FormattedString();

        label.FormattedText.Spans.Add(new Span() { Text = leftChar, TextColor = Colors.Gray });
        label.FormattedText.Spans.Add(new Span() { Text = pronunciation, TextColor = Colors.Blue });
        label.FormattedText.Spans.Add(new Span() { Text = rightChar, TextColor = Colors.Gray });
    }

    private Label CreateWordInflectionTitleLabel(string title, double margin = 0)
    {
        Label label = new Label() { Text = title };
        label.TextColor = Colors.Gray;
        label.HorizontalOptions = LayoutOptions.Start;

        return label;
    }

    private Label CreateWordInflectionValueLabel(string value)
    {

        Label label = new Label() { Text = value };
        label.Margin = new Thickness(5, 0, 0, 0);
        label.FontAttributes = FontAttributes.Bold;

        return label;
    }

    private void SetStatusForVOCAB(bool isAdded)
    {
        var fontImageSource = this.btnVOCAB.Source as FontImageSource;

        fontImageSource.FontFamily = isAdded ? "FASolid" : "FARegular";
        fontImageSource.Color = isAdded ? Colors.Orange : Colors.Gray;
    }

    private void OnPronounceButtonClicked(object sender, EventArgs e)
    {
        string parameter = (sender as ImageButton).CommandParameter.ToString();

        if (parameter != null && this.englishWord != null)
        {
            bool isUS = parameter == "US";

            AudioPlayHelper.PlayEnglishWord(this.englishWord.Word, isUS);

        }
    }

    private async void btnVOCAB_Clicked(object sender, EventArgs e)
    {
        if (this.vocab == null)
        {
            bool success = await DataProcessor.AddVOCAB(EnglishObjectType.Word, this.englishWord.Id);

            if (success)
            {
                this.vocab = await DbObjectsFetcher.GetVOCAB(EnglishObjectType.Word, this.englishWord.Id);

                this.SetStatusForVOCAB(true);

                MessageHelper.ShowToastMessage("已添加到生词本。");
            }
            else
            {
                await DisplayAlert("错误", $"添加到生词本失败！", "确定");
            }
        }
        else
        {
            bool success = await DataProcessor.DeleteVOCAB(this.vocab.Id);

            if (success)
            {
                this.vocab = null;

                this.SetStatusForVOCAB(false);

                MessageHelper.ShowToastMessage("已从生词本移除。");
            }
            else
            {
                await DisplayAlert("错误", "从生词本移除失败！", "确定");
            }
        }
    }

    private void TouchBehavior_LongPressCompleted(object sender, CommunityToolkit.Maui.Core.LongPressCompletedEventArgs e)
    {
        var clipboard = Clipboard.Default;
        clipboard.SetTextAsync(this.lblTitle.Text);

        MessageHelper.ShowToastMessage("已复制到剪切板。");
    }

    private void Reset()
    {
        this.ExamTypeLayout.Children.Clear();
        this.gvWordInflection.Children.Clear();

        this.btnShowAllMeaning.IsVisible = false;
        this.btnHideSpecalMeaning.IsVisible = false;
    }

    private void tbiFinishLearn_Clicked(object sender, EventArgs e)
    {
        this.FinishLearn();
    }

    private async void FinishLearn()
    {
        bool success = await DataProcessor.SaveWordLearnHistory(this.learningEnglishExamType, this.englishWord);

        if (success)
        {
            int nextWordId = await DataProcessor.GetEnglishWordLearnNextId(this.learningEnglishExamType);

            if (nextWordId > 0)
            {
                this.Reset();
                this.SetToolbarItemStatus(true);

                this.ShowWord(nextWordId);
            }
        }
    }

    private async void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
        if (this.learningEnglishExamType == null)
        {
            return;
        }

        if (e.Direction == SwipeDirection.Left)
        {
            int? nextWordId = null;

            int index = this.historyWordIds.IndexOf(this.englishWord.Id);

            if (index < this.historyWordIds.Count - 1)
            {
                nextWordId = this.historyWordIds[index + 1];

                this.Reset();
                this.SetToolbarItemStatus(index + 1 == this.historyWordIds.Count - 1);

                this.ShowWord(nextWordId.Value);
            }
            else
            {
                this.FinishLearn();
            }
        }
        else
        {
            int? previousWordId = null;
            int index = this.historyWordIds.IndexOf(this.englishWord.Id);

            if (index == -1)
            {
                return;
            }

            this.SetToolbarItemStatus(false);

            if (index > 0)
            {
                previousWordId = this.historyWordIds[index - 1];
            }
            else
            {
                previousWordId = await DataProcessor.GetPreviousEnglishLearnedWordId(this.learningEnglishExamType.Id, this.englishWord.Id);
            }

            if (previousWordId > 0)
            {
                if (index == 0)
                {
                    this.historyWordIds.Insert(0, previousWordId.Value);
                }

                this.Reset();
                this.ShowWord(previousWordId.Value);
            }
        }
    }

    private async void TapGestureRecognizer_VariantTapped(object sender, TappedEventArgs e)
    {
        Grid grid = sender as Grid;

        var variant = grid.BindingContext as V_EnglishWordVariant;

        if (variant != null)
        {
            WordDetail wordDetail = (WordDetail)Activator.CreateInstance(typeof(WordDetail), variant.TargetWordId);

            await Navigation.PushAsync(wordDetail);
        }
    }

    private void btnShowAllMeaning_Clicked(object sender, EventArgs e)
    {
        this.ShowOrHideSpecialMeaning(true);
    }

    private void btnHideSpecalMeaning_Clicked(object sender, EventArgs e)
    {
        this.ShowOrHideSpecialMeaning(false);
    }

    private void ShowOrHideSpecialMeaning(bool show)
    {
        int wordId = this.englishWord.Id;

        this.ShowMeanings(wordId, show);

        this.SetShowHideMeaningButtonVisible(show);

        this.ShowInflections(wordId);
    }

    private void SetShowHideMeaningButtonVisible(bool isShowAll)
    {
        this.btnShowAllMeaning.IsVisible = !isShowAll;
        this.btnHideSpecalMeaning.IsVisible = isShowAll;
    }
}