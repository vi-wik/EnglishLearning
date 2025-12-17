using CommunityToolkit.Maui.Views;
using EnglishLearning.App.Helper;
using EnglishLearning.App.Model;
using EnglishLearning.Business;
using EnglishLearning.Business.Helper;
using EnglishLearning.Business.Manager;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using System.Text;
using zoft.MauiExtensions.Core.Extensions;

namespace EnglishLearning.App.Views;

public partial class WordDetail : ContentPage
{
    private V_EnglishWord englishWord = null;
    private EnglishLearning.Model.EnglishWordVOCAB vocab;
    private SettingInfo setting;
    private List<string> partOfSpeeches = null;
    private bool hasSpecialMeaning = false;
    private int syllableCount;
    private List<int> historyWordIds = new List<int>();

    private EnglishExamType learningEnglishExamType { get; set; }
    private bool isForNonExamType;
    private bool isForVOCAB;
    private bool isLearningMode = false;
    private int? lastWordId;

    public WordDetail(int wordId)
    {
        this.Init(wordId);
    }

    public WordDetail(int wordId, EnglishExamType learningEnglishExamType)
    {
        this.learningEnglishExamType = learningEnglishExamType;

        this.Init(wordId);
    }

    public WordDetail(int wordId, EnglishExamType learningEnglishExamType, bool isForVOCAB = false)
    {
        this.learningEnglishExamType = learningEnglishExamType;
        this.isForVOCAB = isForVOCAB;

        this.Init(wordId);
    }

    public WordDetail(int wordId, bool isForNonExamType, bool isForVOCAB = false)
    {
        this.isForNonExamType = isForNonExamType;
        this.isForVOCAB = isForVOCAB;

        this.Init(wordId);
    }

    private async void Init(int wordId)
    {
        InitializeComponent();

        this.setting = SettingManager.GetSetting();
        this.isLearningMode = this.learningEnglishExamType != null || this.isForNonExamType;

        if (!this.isLearningMode)
        {
            this.scrollView.GestureRecognizers.Clear();
        }

        this.ShowWord(wordId, this.setting.AutoPlayAudioWhenLearnWord && this.isLearningMode);
    }

    private void SetToolbarItemStatus(ToolbarItem item, bool enable)
    {
        FontImageSource fs = item.IconImageSource as FontImageSource;
        fs.Color = enable ? Colors.White : Colors.Transparent;

        item.IsEnabled = enable;
    }

    private async void ShowWord(int wordId, bool playAudio = false)
    {
        bool hasMedia = false;

        this.englishWord = await DataProcessor.GetVEnglishWord(wordId);

        bool hasUSPronunciation = this.englishWord?.US_Pronunciation != null;
        bool hasUKPronunciation = this.englishWord?.UK_Pronunciation != null;
        bool hasUSAudio = true;
        bool hasUKAudio = true;
        bool isFullUpperCase = this.englishWord?.Word?.All(item => char.IsUpper(item)) == true;

        if (isFullUpperCase)
        {
            hasUSAudio = false;
            hasUKAudio = false;
        }

        this.lblUS_Pronunciation.IsVisible = hasUSPronunciation;
        this.lblUK_Pronunciation.IsVisible = hasUKPronunciation;

        this.btnUSPronunciation.IsVisible = hasUSAudio;
        this.btnUKPronunciation.IsVisible = hasUKAudio;

        this.USPronunciationLayout.IsVisible = this.lblUS.IsVisible = hasUSPronunciation || hasUSAudio;
        this.UKPronunciationLayout.IsVisible = this.lblUK.IsVisible = hasUKPronunciation || hasUKAudio;

        this.PronunciationGrid.IsVisible = this.USPronunciationLayout.IsVisible || this.UKPronunciationLayout.IsVisible;

        if (!this.PronunciationGrid.IsVisible)
        {
            this.lvMeanings.Margin = new Thickness(this.lvMeanings.Margin.Left, 0, this.lvMeanings.Margin.Right, this.lvMeanings.Margin.Bottom);
        }

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

            if (playAudio && hasUSAudio)
            {
                this.PlayAudio(this.englishWord.Word, true);
            }

            #region 考试类型
            int? examType = this.englishWord.ExamType;

            this.lblExamType.IsVisible = examType.HasValue;

            if (examType.HasValue)
            {
                var examTypes = await DataProcessor.GetEnglishExamTypes();

                StringBuilder sb = new StringBuilder();

                foreach (EnglishExamType type in examTypes)
                {
                    int weight = type.Weight;

                    if ((weight & examType.Value) == weight)
                    {
                        bool isFirst = sb.Length == 0;

                        if (!isFirst)
                        {
                            sb.Append(" / ");
                        }

                        sb.Append(type.Name);
                    }

                    this.lblExamType.Text = sb.ToString();
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
                    if (!this.setting.ShowWordFullMeaning)
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

            this.lblSyllable.Text = "";

            bool needShowSyllable = this.setting.ShowWordSyllable;
            bool canShowSyllable = false;

            //无论配置是否显示音节，都要获取音节数量，因为它是判断形容词和副词比较级和最高级的依据
            int syllableCount = await DataProcessor.GetEnglishWordSyllableCount(wordId);

            if (syllableCount > 1)
            {
                var syllables = await DataProcessor.GetEnglishWordSyllables(wordId);

                canShowSyllable = true;

                UIHelper.SetEnglishWordSyllableDisplayText(this.lblSyllable, this.englishWord.Word, syllables);
            }
            else if (syllableCount == 0)
            {
                syllableCount = 1;
            }

            this.syllableCount = syllableCount;         

            this.lblSyllable.IsVisible = needShowSyllable && canShowSyllable;

            #endregion

            #region 变形

            this.InflectionExpander.IsVisible = false;

            if (this.setting.WordInflectionDisplayMode != ExpanderDisplayMode.Hidden)
            {
                this.ShowInflections(wordId);
            }

            #endregion

            #region 媒体
            this.MediaExpander.IsVisible = false;
            this.lvMedias.ItemsSource = null;

            if (this.setting.WordMediaDisplayMode != ExpanderDisplayMode.Hidden)
            {
                var medias = await MediaHelper.DecorateMedias(await DataProcessor.GetVEnglishWordMedias(this.englishWord.Id));

                hasMedia = medias.Count() > 0;

                if (hasMedia)
                {
                    this.lvMedias.ItemsSource = medias;
                    this.MediaExpander.IsVisible = true;
                    this.MediaExpander.IsExpanded = this.setting.WordMediaDisplayMode == ExpanderDisplayMode.Expanded;
                }
            }

            #endregion

            #region 例句      

            this.ExampleExpander.IsVisible = false;
            this.lvExamples.ItemsSource = null;

            if (this.setting.WordExampleDisplayMode != ExpanderDisplayMode.Hidden)
            {
                var examples = await DataProcessor.GetVEnglishWordExamples(this.englishWord.Id);

                if (examples.Count() > 0)
                {
                    List<EnglishExampleDisplay> exampleDisplays = new List<EnglishExampleDisplay>();

                    int order = 1;

                    foreach (var example in examples)
                    {
                        EnglishExampleDisplay display = new EnglishExampleDisplay();

                        display.Vocabulary = this.englishWord.Word;
                        display.Order = $"{order}.";
                        display.Example = $"{example.Example}{UIHelper.MakeupPunctuation(example.Meaning, true)}";
                        display.Meaning = $"{example.Meaning}{UIHelper.MakeupPunctuation(example.Meaning, false)}";

                        exampleDisplays.Add(display);

                        order++;
                    }

                    this.lvExamples.ItemsSource = exampleDisplays;
                    this.ExampleExpander.IsVisible = true;
                    this.ExampleExpander.IsExpanded = this.setting.WordExampleDisplayMode == ExpanderDisplayMode.Expanded;
                }
            }

            #endregion

            #region 相关单词

            this.FormExpander.IsVisible = false;
            this.lvWordForm.ItemsSource = null;

            if (this.setting.WordFormDisplayMode != ExpanderDisplayMode.Hidden)
            {
                var forms = await DataProcessor.GetVEnglishWordForms(wordId);

                int formCount = forms.Count();

                if (formCount > 0)
                {
                    this.lvWordForm.ItemsSource = forms;

                    this.FormExpander.IsVisible = true;
                    this.FormExpander.IsExpanded = this.setting.WordFormDisplayMode == ExpanderDisplayMode.Expanded;
                }
            }

            #endregion

            #region 单词结构

            this.StructureExpander.IsVisible = false;

            if (this.setting.WordStructureDisplayMode != ExpanderDisplayMode.Hidden)
            {
                var dictStructures = await DataProcessor.GetEnglishWordStructures(wordId);

                if (dictStructures.Count > 0)
                {
                    this.ShowWordStructures(dictStructures);

                    this.StructureExpander.IsVisible = true;
                    this.StructureExpander.IsExpanded = this.setting.WordStructureDisplayMode == ExpanderDisplayMode.Expanded;
                }
            }

            #endregion

            #region 异体

            this.VariantExpander.IsVisible = false;
            this.lvVariants.ItemsSource = null;

            if (this.setting.WordVariantDisplayMode != ExpanderDisplayMode.Hidden)
            {
                var variants = await DataProcessor.GetVEnglishWordVariants(wordId);

                if (variants.Count() > 0)
                {
                    this.lvVariants.ItemsSource = variants;
                    this.VariantExpander.IsVisible = true;

                    this.VariantExpander.IsExpanded = this.setting.WordVariantDisplayMode == ExpanderDisplayMode.Expanded;
                }
            }

            #endregion

            this.btnVOCAB.IsVisible = true;

            this.vocab = await DataProcessor.GetEnglishWordVOCAB(this.englishWord.Id);

            if (this.isLearningMode)
            {
                int? previousWordId = await DataProcessor.GetEnglishWordLearnedPreviousWordId(this.learningEnglishExamType?.Id, wordId, this.isForNonExamType, this.isForVOCAB);

                this.SetToolbarItemStatus(this.tbiPrevious, previousWordId.HasValue);

                this.SetToolbarItemStatus(this.tbiNext, wordId != this.lastWordId);
            }
        }
        else
        {
            this.vocab = null;
            this.btnVOCAB.IsVisible = false;
            this.lblSyllable.IsVisible = false;
        }

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

    private async void ShowMeanings(int wordId, bool showFullMeaning = false)
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

                        exitingItem.Meaning += $"和{wordInflection.TypeName}";
                    }

                }
                else if (typeId == 6 || typeId == 7)
                {
                    meaning = getMeaning(wordInflection);
                }

                if (!existing)
                {
                    virtualMeanings.Add(new EnglishWordMeaningForDisplay() { IsVirtual = true, Word = wordInflection.Word, WordId = wordInflection.WordId, PartOfSpeech = partOfSpeech, Meaning = meaning });
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
            int comparativeDegreeCount = inflections.Count(item => item.TypeId == 6);
            int superlativeDegreeCount = inflections.Count(item => item.TypeId == 7);

            if (comparativeDegreeCount + superlativeDegreeCount == 0)
            {
                string comparativeDegree = EnglishWordInflectionHelper.GetComparativeDegree(this.englishWord.Word);
                string superlativeDegree = EnglishWordInflectionHelper.GetSuperlativeDegree(this.englishWord.Word);

                bool isMoreDegree = this.englishWord.IsMoreDegree == true;

                if (isMoreDegree || this.syllableCount > 2)
                {
                    addInflection(6, "more " + this.englishWord.Word);
                    addInflection(7, "most " + this.englishWord.Word);
                }
                else
                {
                    addInflection(6, comparativeDegree);
                    addInflection(7, superlativeDegree);
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
            else if (typeId == 6 || typeId == 7)
            {
                if (!this.HasAdj())
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
            this.InflectionExpander.IsVisible = true;

            int columnIndex = 0, rowIndex = 0;
            int count = 0;

            for (int i = 0; i < labels.Count; i++)
            {
                Label label = labels[i];

                this.gvWordInflection.Add(label, columnIndex, rowIndex);

                columnIndex++;

                if (columnIndex == 2)
                {
                    columnIndex = 0;
                }

                count++;

                if (count % 2 == 0)
                {
                    rowIndex++;
                }
            }

            this.InflectionExpander.IsExpanded = setting.WordInflectionDisplayMode == ExpanderDisplayMode.Expanded;
        }
        else
        {
            this.InflectionExpander.IsVisible = false;
        }
    }

    private void InflectionExpander_ExpandedChanged(object? sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        string iconKey = this.InflectionExpander.IsExpanded ? "up" : "down";

        this.Font_InflectionShow.Glyph = Application.Current.Resources[iconKey].ToString();
    }

    private void Expander_ExpandedChanged(object? sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        var expander = sender as Expander;

        string iconKey = expander.IsExpanded ? "up" : "down";

        string fontControlName = $"Font_{expander.CommandParameter}Show";

        var imgSource = this.FindByName(fontControlName) as FontImageSource;

        imgSource.Glyph = Application.Current.Resources[iconKey].ToString();
    }

    private void ExpandButtonShow_Clicked(object sender, EventArgs e)
    {
        ImageButton btn = sender as ImageButton;

        string expanderName = $"{btn.CommandParameter}Expander";

        var expander = this.FindByName(expanderName) as Expander;

        expander.IsExpanded = !expander.IsExpanded;
    }

    private void SetPronunciationDisplayText(Label label, string pronunciation)
    {
        WordPronunciationBracketDisplayMode mode = this.setting.WordPronunciationBracketMode;

        string leftChar = mode == WordPronunciationBracketDisplayMode.Square ? "[" : "/";
        string rightChar = mode == WordPronunciationBracketDisplayMode.Square ? "]" : "/";

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
        label.Margin = new Thickness(10, 0, 0, 0);
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

            this.PlayAudio(this.englishWord.Word, isUS);
        }
    }

    private void PlayAudio(string word, bool isUS)
    {
        AudioPlayHelper.PlayEnglishWord(word, isUS);
    }

    private async void btnVOCAB_Clicked(object sender, EventArgs e)
    {
        if (this.vocab == null)
        {
            bool success = await DataProcessor.AddEnglishWordVOCAB(this.englishWord.Id);

            if (success)
            {
                this.vocab = await DataProcessor.GetEnglishWordVOCAB(this.englishWord.Id);

                this.SetStatusForVOCAB(true);

                //MessageHelper.ShowToastMessage("已添加到生词本。");
            }
            else
            {
                await DisplayAlert("错误", $"添加到生词本失败！", "确定");
            }
        }
        else
        {
            bool success = await DataProcessor.DeleteEnglishWordVOCAB(this.vocab.Id);

            if (success)
            {
                this.vocab = null;

                this.SetStatusForVOCAB(false);

                //MessageHelper.ShowToastMessage("已从生词本移除。");
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
        this.lblExamType.Text = "";
        this.gvWordInflection.Children.Clear();
        this.gvStructure.Children.Clear();

        this.btnShowAllMeaning.IsVisible = false;
        this.btnHideSpecalMeaning.IsVisible = false;
    }

    private void tbiFinishLearn_Clicked(object sender, EventArgs e)
    {
        this.FinishLearn();
    }

    private void tbiPrevious_Clicked(object sender, EventArgs e)
    {
        this.ShowPrevious();
    }

    private void tbiNext_Clicked(object sender, EventArgs e)
    {
        this.ShowNext();
    }

    private async void ShowPrevious()
    {
        if (!this.isLearningMode)
        {
            return;
        }

        int? previousWordId = null;
        int index = this.historyWordIds.IndexOf(this.englishWord.Id);

        if (index == -1)
        {
            return;
        }

        this.SetToolbarItemStatus(this.tbiNext, true);

        if (index > 0)
        {
            previousWordId = this.historyWordIds[index - 1];
        }
        else
        {
            previousWordId = await DataProcessor.GetEnglishWordLearnedPreviousWordId(this.learningEnglishExamType?.Id, this.englishWord.Id, this.isForNonExamType, this.isForVOCAB);
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

    private async void ShowNext()
    {
        if (!this.isLearningMode)
        {
            return;
        }

        int? nextWordId = null;

        int index = this.historyWordIds.IndexOf(this.englishWord.Id);

        if (index < this.historyWordIds.Count - 1)
        {
            nextWordId = this.historyWordIds[index + 1];

            this.Reset();

            bool isLastHistory = index + 1 == this.historyWordIds.Count - 1;

            this.SetToolbarItemStatus(this.tbiNext, true);

            this.ShowWord(nextWordId.Value);
        }
        else
        {
            this.FinishLearn();
        }
    }

    private bool CanFinishLearn()
    {
        int index = this.historyWordIds.IndexOf(this.englishWord.Id);

        return index == this.historyWordIds.Count - 1;
    }

    private async void FinishLearn()
    {
        bool success = await DataProcessor.SaveEnglishWordLearnedHistory(this.englishWord);

        if (success)
        {
            int? nextWordId = await DataProcessor.GetEnglishWordNotLearnedNextId(this.learningEnglishExamType, this.isForNonExamType, this.isForVOCAB, this.setting.WordVOCABLearnSortMode);

            if (nextWordId > 0)
            {
                this.Reset();
                this.SetToolbarItemStatus(this.tbiNext, true);

                this.ShowWord(nextWordId.Value, this.setting.AutoPlayAudioWhenLearnWord);
            }
            else
            {
                this.lastWordId = this.englishWord.Id;
                this.SetToolbarItemStatus(this.tbiNext, false);
                await DisplayAlert("提示", "没有更多了。", "确定");
            }
        }
    }

    private async void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
        if (!this.isLearningMode)
        {
            return;
        }

        if (e.Direction == SwipeDirection.Left)
        {
            this.ShowNext();
        }
        else
        {
            this.ShowPrevious();
        }
    }

    private async void TapGestureRecognizer_VariantTapped(object sender, TappedEventArgs e)
    {
        Grid grid = sender as Grid;

        var variant = grid.BindingContext as V_EnglishWordVariant;

        if (variant != null)
        {
            this.NavigateToWordDetailPage(variant.TargetWordId);
        }
    }

    private async void NavigateToWordDetailPage(int wordId)
    {
        WordDetail wordDetail = (WordDetail)Activator.CreateInstance(typeof(WordDetail), wordId);

        await Navigation.PushAsync(wordDetail);
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

        if (this.setting.WordInflectionDisplayMode != ExpanderDisplayMode.Hidden)
        {
            this.ShowInflections(wordId);
        }
    }

    private void SetShowHideMeaningButtonVisible(bool isShowAll)
    {
        this.btnShowAllMeaning.IsVisible = !isShowAll;
        this.btnHideSpecalMeaning.IsVisible = isShowAll;
    }

    private void MeaningLable_BindingContextChanged(object sender, EventArgs e)
    {
        Label label = sender as Label;

        EnglishWordMeaningForDisplay display = label.BindingContext as EnglishWordMeaningForDisplay;

        if (display.IsVirtual)
        {
            string text = label.Text;
            string word = display.Word;

            if (text.StartsWith(word))
            {
                label.FormattedText = new FormattedString();

                Span wordSpan = new Span() { Text = word, TextColor = Colors.Blue };

                var tapGestureRecognizer = new TapGestureRecognizer() { NumberOfTapsRequired = 1, CommandParameter = display.WordId };
                tapGestureRecognizer.Tapped += this.TapGestureRecognizer_WordTapped;

                wordSpan.GestureRecognizers.Add(tapGestureRecognizer);

                label.FormattedText.Spans.Add(wordSpan);

                label.FormattedText.Spans.Add(new Span() { Text = text.Substring(word.Length) });
            }
        }
    }


    private void TapGestureRecognizer_ScollViewTapped(object sender, TappedEventArgs e)
    {
        if (!this.isLearningMode)
        {
            return;
        }

        if (this.tbiNext.IsEnabled && this.CanFinishLearn())
        {
            this.FinishLearn();
        }
    }

    private async void TapGestureRecognizer_FormTapped(object sender, TappedEventArgs e)
    {
        Grid grid = sender as Grid;

        var form = grid.BindingContext as V_EnglishWordForm;

        if (form != null)
        {
            this.NavigateToWordDetailPage(form.TargetWordId);
        }
    }

    private async void ShowWordStructures(Dictionary<int, IEnumerable<V_EnglishWordStructure>> dict)
    {
        this.gvStructure.Clear();

        RowDefinition[] rowDefinations = new RowDefinition[dict.Count];

        for (int i = 0; i < rowDefinations.Length; i++)
        {
            rowDefinations[i] = new RowDefinition(new GridLength(0, GridUnitType.Star));
        }

        this.gvStructure.RowDefinitions = new RowDefinitionCollection(rowDefinations);

        int rowIndex = 0;

        foreach (var kp in dict)
        {
            var structrues = kp.Value;

            Label lbl = new Label();
            lbl.FormattedText = new FormattedString();

            List<Span> spans = new List<Span>();

            int count = 0;

            foreach (var structrue in structrues)
            {
                count++;

                Span span = new Span();
                Span extraBeforeSpan = null;
                Span extraAfterSpan = null;
                bool needAddGestrue = false;

                if (structrue.Prefix != null)
                {
                    span.Text = structrue.Prefix;
                    span.TextColor = Colors.Green;

                    if (structrue.PrefixId > 0)
                    {
                        needAddGestrue = true;
                    }
                }
                else if (structrue.Suffix != null)
                {
                    string suffix = structrue.Suffix;
                    int? suffixId = structrue.SuffixId;
                    string text = structrue.Suffix;

                    if (suffixId == null && suffix.StartsWith("i") && structrue.ChangeEndOldContent == "y")
                    {
                        var result = await DataProcessor.GetEnglishWordSuffixByName(suffix.Substring(1));

                        if (result != null)
                        {
                            text = suffix.Substring(1);
                            suffixId = result.Id;

                            extraBeforeSpan = new Span() { Text = "i" };
                        }
                    }

                    span.Text = text;
                    span.TextColor = suffixId > 0 ? Colors.Orange : Colors.Black;

                    if (suffixId > 0)
                    {
                        needAddGestrue = true;
                        structrue.SuffixId = suffixId;
                        structrue.Suffix = text;
                    }
                }
                else if (structrue.Root != null)
                {
                    span.Text = structrue.Root;
                    span.TextColor = Colors.Purple;

                    if (structrue.RootId > 0)
                    {
                        needAddGestrue = true;
                    }
                }
                else if (structrue.SubWord != null)
                {
                    V_EnglishWordStructure nextStructure = null;

                    if (count < structrues.Count())
                    {
                        nextStructure = structrues.Skip(count).Take(1).FirstOrDefault();
                    }

                    string displayText = structrue.SubWord;

                    if (nextStructure != null && nextStructure.ChangeEndOldContent != null)
                    {
                        displayText = structrue.SubWord.Substring(0, structrue.SubWord.Length - nextStructure.ChangeEndOldContent.Length);
                    }

                    span.Text = displayText;
                    span.TextColor = Colors.Blue;

                    if (structrue.SubWordId > 0)
                    {
                        var tapGestureRecognizer = new TapGestureRecognizer() { NumberOfTapsRequired = 1, CommandParameter = structrue.SubWordId };
                        tapGestureRecognizer.Tapped += this.TapGestureRecognizer_WordTapped;

                        span.GestureRecognizers.Add(tapGestureRecognizer);
                    }

                    if (nextStructure != null && nextStructure.ChangeEndOldContent != null)
                    {
                        extraAfterSpan = new Span() { Text = nextStructure.ChangeEndOldContent, TextColor = Colors.Gray };
                    }
                }

                if (needAddGestrue)
                {
                    var tapGestureRecognizer = new TapGestureRecognizer() { NumberOfTapsRequired = 1, CommandParameter = structrue };
                    tapGestureRecognizer.Tapped += this.TapGestureRecognizer_WordElementTapped;

                    span.GestureRecognizers.Add(tapGestureRecognizer);
                }

                if (extraBeforeSpan != null)
                {
                    spans.Add(extraBeforeSpan);
                }

                spans.Add(span);

                if (extraAfterSpan != null)
                {
                    spans.Add(extraAfterSpan);
                }
            }

            for (int i = 0; i < spans.Count; i++)
            {
                lbl.FormattedText.Spans.Add(spans[i]);

                if (i < spans.Count - 1)
                {
                    if (spans[i + 1].TextColor != Colors.Gray)
                    {
                        var separator = new Span() { Text = " + ", TextColor = Colors.Black };

                        lbl.FormattedText.Spans.Add(separator);
                    }
                }
            }

            this.gvStructure.Add(lbl, 0, rowIndex);

            rowIndex++;
        }
    }

    private async void TapGestureRecognizer_WordTapped(object sender, TappedEventArgs e)
    {
        int wordId = Convert.ToInt32(e.Parameter);

        this.NavigateToWordDetailPage(wordId);
    }

    private async void TapGestureRecognizer_WordElementTapped(object sender, TappedEventArgs e)
    {
        var structure = e.Parameter as V_EnglishWordStructure;

        if (structure != null)
        {
            EnglishWordElementType type = EnglishWordElementType.None;
            string name = null;

            if (structure.PrefixId > 0)
            {
                type = EnglishWordElementType.Prefix;
                name = structure.Prefix;
            }
            else if (structure.SuffixId > 0)
            {
                type = EnglishWordElementType.Suffix;
                name = structure.Suffix;
            }
            else if (structure.RootId > 0)
            {
                type = EnglishWordElementType.WordRoot;
                name = structure.Root;
            }

            if (type != EnglishWordElementType.None)
            {
                WordRootAffixList page = (WordRootAffixList)Activator.CreateInstance(typeof(WordRootAffixList), type, name);

                await Navigation.PushAsync(page);
            }
        }
    }
}