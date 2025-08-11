using CommunityToolkit.Maui.Storage;
using EnglishLearning.Business.Manager;
using EnglishLearning.Business.Model;

namespace EnglishLearning.App.Views;

public partial class CommonSetting : ContentPage
{
    private SettingInfo setting;

    public CommonSetting()
    {
        InitializeComponent();

        this.setting = SettingManager.GetSetting();

        this.chkShowWordsWhileInputing.IsChecked = this.setting.ShowWordsWhileInputing;
        this.chkShowWordMeaningWhenShowWordList.IsChecked = this.setting.ShowWordMeaningWhenShowWordList;
        this.chkShowWordMeaningWhenShowVOCABs.IsChecked = this.setting.ShowWordMeaningWhenShowVOCABs;
        this.chkShowWordFullMeaning.IsChecked = this.setting.ShowWordFullMeaning;
        this.chkEnableLog.IsChecked = this.setting.EnableLog;
        this.txtPronunciationFileRootFolder.Text = this.setting.PronunciationFileRootFolder;

        int pronunciationBracketMode = setting.PronunciationBracketMode;

        if (pronunciationBracketMode == 1)
        {
            this.rbPronunciationBracket1.IsChecked = true;
        }
        else
        {
            this.rbPronunciationBracket2.IsChecked = true;
        }
    }

    private void chkShowWordsWhileInputing_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        this.setting.ShowWordsWhileInputing = this.chkShowWordsWhileInputing.IsChecked;

        this.Save();
    }

    private void chkShowWordMeaningWhenShowWordList_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        this.setting.ShowWordMeaningWhenShowWordList = this.chkShowWordMeaningWhenShowWordList.IsChecked;

        this.Save();
    }

    private void chkShowWordMeaningWhenShowVOCABs_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        this.setting.ShowWordMeaningWhenShowVOCABs = this.chkShowWordMeaningWhenShowVOCABs.IsChecked;

        this.Save();
    }

    private void chkEnableLog_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        this.setting.EnableLog = this.chkEnableLog.IsChecked;

        this.Save();
    }

    private void chkShowWordFullMeaning_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        this.setting.ShowWordFullMeaning = this.chkShowWordFullMeaning.IsChecked;

        this.Save();
    }

    private void Save()
    {
        SettingManager.SaveSetting(this.setting);
    }

    private void rbPronunciationBracket_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (this.setting == null)
        {
            return;
        }

        RadioButton radioButton = sender as RadioButton;

        if (radioButton.IsChecked)
        {
            this.setting.PronunciationBracketMode = Convert.ToInt32(radioButton.Value);

            this.Save();
        }
    }   

    private async void btnChoosePronunciationFileRootFoldern_Clicked(object sender, EventArgs e)
    {
        var result = await FolderPicker.Default.PickAsync();

        if (result.IsSuccessful)
        {
            string folder = result.Folder.Path;

            this.txtPronunciationFileRootFolder.Text = folder;

            this.setting.PronunciationFileRootFolder = folder;

            this.Save();
        }
    }
}