using EnglishLearning.App.Helper;
using EnglishLearning.Business;
using EnglishLearning.Business.Helper;
using EnglishLearning.Business.Manager;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using System.Windows.Input;

namespace EnglishLearning.App.Views;

public partial class PhraseDetail : ContentPage
{
    private V_EnglishPhrase phrase = null;
    private EnglishLearning.Model.EnglishPhraseVOCAB vocab;
    private bool isForVOCAB = false;
    private List<int> historyPhraseIds = new List<int>();
    private SettingInfo setting;
    private bool isLearningMode = false;
    private int? lastPhraseId;

    public PhraseDetail(int phraseId, bool isLearningMode = false, bool isForVOCAB = false)
    {
        InitializeComponent();

        this.isLearningMode = isLearningMode;
        this.isForVOCAB = isForVOCAB;

        this.ShowPhrase(phraseId);
    }

    private async void ShowPhrase(int phraseId)
    {
        this.lblMeaning.Text = "";

        this.phrase = await DataProcessor.GetVEnglishPhrase(phraseId);
        this.setting = SettingManager.GetSetting();

        if (this.phrase != null)
        {
            if (!this.historyPhraseIds.Contains(this.phrase.Id))
            {
                this.historyPhraseIds.Add(this.phrase.Id);
            }

            this.Title = this.phrase.Phrase;
            this.lblPhrase.Text = this.phrase.Phrase;

            string typeName = this.phrase.TypeName_EN;

            if (typeName == nameof(EnglishPhraseTypeName.Proverb) || typeName == nameof(EnglishPhraseTypeName.Slang))
            {
                this.lblMeaning.FormattedText = new FormattedString();

                Span span = new Span() { Text = $"<{this.phrase.TypeName.First()}>", TextColor = Colors.Gray };

                this.lblMeaning.FormattedText.Spans.Add(span);

                this.lblMeaning.FormattedText.Spans.Add(new Span() { Text = this.phrase.Meaning, TextColor = Colors.Black });
            }
            else
            {
                this.lblMeaning.Text = this.phrase.Meaning;
                this.lblMeaning.TextColor = Colors.Black;
            }

            bool hasSynonym = !string.IsNullOrEmpty(this.phrase.Synonym);

            this.SynonymLayout.IsVisible = hasSynonym;

            if (hasSynonym)
            {
                this.lvSynonym.ItemsSource = this.phrase.Synonym.Split(';').Select(item => new TextItem() { Text = item });
            }

            var medias = await MediaHelper.DecorateMedias(await DataProcessor.GetVEnglishPhraseMedias(this.phrase.Id));

            this.lvMedias.ItemsSource = medias;

            this.lblIntroduction.IsVisible = medias != null && medias.Count() > 0;
            this.lvMedias.IsVisible = true;

            #region 示例
            var examples = await DataProcessor.GetVEnglishPhraseExamples(this.phrase.Id);

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

            this.btnVOCAB.IsVisible = true;

            this.vocab = await DataProcessor.GetEnglishPhraseVOCAB(this.phrase.Id);

            if (this.isLearningMode)
            {
                int? previousPhraseId = await DataProcessor.GetEnglishPhraseLearnedPreviousPhraseId(phraseId, this.isForVOCAB);

                this.SetToolbarItemStatus(this.tbiPrevious, previousPhraseId.HasValue);
                this.SetToolbarItemStatus(this.tbiNext, phraseId != this.lastPhraseId);
            }
        }
        else
        {
            this.lblMeaning.Text = "未找到任何记录！";

            this.lblIntroduction.IsVisible = false;
            this.lvMedias.IsVisible = false;

            this.vocab = null;
            this.btnVOCAB.IsVisible = false;
        }

        this.SetStatusForVOCAB(this.vocab != null);
    }

    private void SetStatusForVOCAB(bool isAdded)
    {
        var fontImageSource = this.btnVOCAB.Source as FontImageSource;

        fontImageSource.FontFamily = isAdded ? "FASolid" : "FARegular";
        fontImageSource.Color = isAdded ? Colors.Orange : Colors.Gray;
    }

    private async void btnVOCAB_Clicked(object sender, EventArgs e)
    {
        if (this.vocab == null)
        {
            bool success = await DataProcessor.AddEnglishPhraseVOCAB(this.phrase.Id);

            if (success)
            {
                this.vocab = await DataProcessor.GetEnglishPhraseVOCAB(this.phrase.Id);

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
            bool success = await DataProcessor.DeleteEnglishPhraseVOCAB(this.vocab.Id);

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

        int? previousPhraseId = null;
        int index = this.historyPhraseIds.IndexOf(this.phrase.Id);

        if (index == -1)
        {
            return;
        }

        this.SetToolbarItemStatus(this.tbiNext, true);

        if (index > 0)
        {
            previousPhraseId = this.historyPhraseIds[index - 1];
        }
        else
        {
            previousPhraseId = await DataProcessor.GetEnglishPhraseLearnedPreviousPhraseId(this.phrase.Id, this.isForVOCAB);
        }

        if (previousPhraseId > 0)
        {
            if (index == 0)
            {
                this.historyPhraseIds.Insert(0, previousPhraseId.Value);
            }

            this.Reset();

            this.ShowPhrase(previousPhraseId.Value);
        }
    }

    private async void ShowNext()
    {
        if (!this.isLearningMode)
        {
            return;
        }

        int? nextPhraseId = null;

        int index = this.historyPhraseIds.IndexOf(this.phrase.Id);

        if (index < this.historyPhraseIds.Count - 1)
        {
            nextPhraseId = this.historyPhraseIds[index + 1];

            this.Reset();

            bool isLastHistory = index + 1 == this.historyPhraseIds.Count - 1;

            this.SetToolbarItemStatus(this.tbiNext, true);

            this.ShowPhrase(nextPhraseId.Value);
        }
        else
        {
            this.FinishLearn();
        }
    }

    private void SetToolbarItemStatus(ToolbarItem item, bool enable)
    {
        FontImageSource fs = item.IconImageSource as FontImageSource;
        fs.Color = enable ? Colors.White : Colors.Transparent;

        item.IsEnabled = enable;
    }

    private void Reset()
    {
        this.lblMeaning.Text = "";
    }

    private async void FinishLearn()
    {
        bool success = await DataProcessor.SaveEnglishPhraseLearnedHistory(this.phrase);

        if (success)
        {
            int? nextPhraseId = await DataProcessor.GetEnglishPhraseNotLearnedNextId(this.isForVOCAB, this.setting.WordVOCABLearnSortMode);

            if (nextPhraseId > 0)
            {
                this.Reset();
                this.SetToolbarItemStatus(this.tbiNext, true);

                this.ShowPhrase(nextPhraseId.Value);
            }
            else
            {
                this.lastPhraseId = this.phrase.Id;
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

    private bool CanFinishLearn()
    {
        int index = this.historyPhraseIds.IndexOf(this.phrase.Id);

        return index == this.historyPhraseIds.Count - 1;
    }

    public class TextItem
    {
        public string Text { get; set; }
    }
}