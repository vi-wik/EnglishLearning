using EnglishLearning.BLL.Core;
using EnglishLearning.BLL.MAUI.Manager;

namespace EnglishLearning.App.Views;

public partial class WordLearn : ContentPage
{
    public WordLearn()
    {
        InitializeComponent();
    }

    private async void TapGestureRecognizer_ExamTypeTapped(object sender, TappedEventArgs e)
    {
        WordLearnDetail page = (WordLearnDetail)Activator.CreateInstance(typeof(WordLearnDetail));

        await Navigation.PushAsync(page);
    }

    private async void TapGestureRecognizer_WordVOCABTapped(object sender, TappedEventArgs e)
    {
        WordLearnVOCAB page = (WordLearnVOCAB)Activator.CreateInstance(typeof(WordLearnVOCAB));

        await Navigation.PushAsync(page);
    }

    private async void TapGestureRecognizer_PhraseTapped(object sender, TappedEventArgs e)
    {
        this.ShowPhrase(false);
    }

    private async void TapGestureRecognizer_PhraseVOCABTapped(object sender, TappedEventArgs e)
    {
        this.ShowPhrase(true);
    }

    private async void ShowPhrase(bool isForVOCAB)
    {
        int? phraseId = await DataProcessor.GetEnglishPhraseNotLearnedNextId(isForVOCAB, SettingManager.GetSetting().PhraseVOCABLearnSortMode);

        if (phraseId > 0)
        {
            PhraseDetail page = (PhraseDetail)Activator.CreateInstance(typeof(PhraseDetail), phraseId.Value, true, isForVOCAB);

            await Navigation.PushAsync(page);
        }
        else
        {
            int count = await DataProcessor.GetEnglishPhraseVOCABCount();

            if (count == 0)
            {
                await DisplayAlert("提示", "没有相关记录。", "确定");
            }
            else
            {
                await DisplayAlert("提示", "已学完所有短语。", "确定");
            }
        }
    }

    private async void TapGestureRecognizer_SettingTapped(object sender, TappedEventArgs e)
    {
        WordLearnSetting page = (WordLearnSetting)Activator.CreateInstance(typeof(WordLearnSetting));

        await Navigation.PushAsync(page);
    }
}