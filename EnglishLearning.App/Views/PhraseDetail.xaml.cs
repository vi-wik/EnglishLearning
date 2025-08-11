using EnglishLearning.App.Helper;
using EnglishLearning.Business;
using EnglishLearning.Business.Model;
using EnglishLearning.DataAccess;
using EnglishLearning.Model;
using System.Windows.Input;

namespace EnglishLearning.App.Views;

public partial class PhraseDetail : ContentPage
{
    private V_EnglishPhrase phrase = null;
    private EnglishLearning.Model.VOCAB vocab;   

    public PhraseDetail(int phraseId)
    {
        InitializeComponent();       

        this.ShowPhrase(phraseId);
    }   

    private async void ShowPhrase(int phraseId)
    {
        this.lblMeaning.Text = "";

        this.phrase = await DataProcessor.GetVEnglishPhrase(phraseId);

        if (this.phrase != null)
        {
            this.Title = this.phrase.Phrase;

            string typeName = this.phrase.TypeName_EN;

            if(typeName == nameof(EnglishPhraseTypeName.Proverb) || typeName == nameof(EnglishPhraseTypeName.Slang))
            {
                this.lblPhraseType.IsVisible = true;
                this.lblPhraseType.Text = $"<{this.phrase.TypeName.First()}>";
            }
            else
            {
                this.lblPhraseType.IsVisible = false;
            }

            this.lblMeaning.Text = this.phrase.Meaning;

            bool hasSynonym = !string.IsNullOrEmpty(this.phrase.Synonym);

            this.SynonymLayout.IsVisible = hasSynonym;

            if(hasSynonym)
            {
                this.lvSynonym.ItemsSource = this.phrase.Synonym.Split(';').Select(item => new TextItem() { Text = item });
            }           

            var medias = await DataProcessor.GetVEnglishPhraseMedias(this.phrase.Id);

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

                display.Example = $"{order}. {example.Example}{UIHelper.MakeupPunctuation(example.Example, true)}";
                display.Meaning = $"{example.Meaning}{UIHelper.MakeupPunctuation(example.Meaning, false)}";

                exampleDisplays.Add(display);

                order++;
            }

            this.lblExample.IsVisible = exampleDisplays.Count > 0;
            this.lvExamples.ItemsSource = exampleDisplays;

            #endregion

            this.btnVOCAB.IsVisible = true;

            this.vocab = await DbObjectsFetcher.GetVOCAB(EnglishObjectType.Phrase, this.phrase.Id);
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
            bool success = await DataProcessor.AddVOCAB(EnglishObjectType.Phrase, this.phrase.Id);

            if (success)
            {
                this.vocab = await DbObjectsFetcher.GetVOCAB(EnglishObjectType.Phrase, this.phrase.Id);

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

    public class TextItem
    {
        public string Text { get; set; }
    }
}