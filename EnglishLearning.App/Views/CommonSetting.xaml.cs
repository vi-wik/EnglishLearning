using CommunityToolkit.Maui.Storage;
using EnglishLearning.BLL.Core.Model;
using EnglishLearning.BLL.MAUI.Manager;
using zoft.MauiExtensions.Core.Extensions;

namespace EnglishLearning.App.Views;

public partial class CommonSetting : ContentPage
{
    private SettingInfo setting;

    public CommonSetting()
    {
        InitializeComponent();

        this.setting = SettingManager.GetSetting();

        this.switchShowWordsWhileInputing.IsToggled = this.setting.ShowWordsWhileInputing;
        this.switchShowWordMeaningWhenShowWordList.IsToggled = this.setting.ShowWordMeaningWhenShowWordList;
        this.switchShowWordMeaningWhenShowVOCABs.IsToggled = this.setting.ShowWordMeaningWhenShowVOCABs;
        this.switchShowWordFullMeaning.IsToggled = this.setting.ShowWordFullMeaning;
    
        this.switchShowWordSyllable.IsToggled = this.setting.ShowWordSyllable;
        this.switchAutoPlayAudioWhenLearnWord.IsToggled = this.setting.AutoPlayAudioWhenLearnWord;
        this.switchEnableLog.IsToggled = this.setting.EnableLog;
        this.txtPronunciationFileRootFolder.Text = this.setting.PronunciationFileRootFolder;   

        var expanderDisplayModeNames = ControlDisplay.ExpanderDisplayModeNames.Values;

        this.pickerWordInflectionDisplayMode.Items.AddRange(expanderDisplayModeNames);
        this.pickerWordFormDisplayMode.Items.AddRange(expanderDisplayModeNames);
        this.pickerWordStructureDisplayMode.Items.AddRange(expanderDisplayModeNames);
        this.pickerWordVariantDisplayMode.Items.AddRange(expanderDisplayModeNames);
        this.pickerWordMediaDisplayMode.Items.AddRange(expanderDisplayModeNames);
        this.pickerWordExampleDisplayMode.Items.AddRange(expanderDisplayModeNames);

        this.SetPickerSelectedItem(this.pickerWordInflectionDisplayMode, setting.WordInflectionDisplayMode);
        this.SetPickerSelectedItem(this.pickerWordFormDisplayMode, setting.WordFormDisplayMode);
        this.SetPickerSelectedItem(this.pickerWordStructureDisplayMode, setting.WordStructureDisplayMode);
        this.SetPickerSelectedItem(this.pickerWordVariantDisplayMode, setting.WordVariantDisplayMode);
        this.SetPickerSelectedItem(this.pickerWordMediaDisplayMode, setting.WordMediaDisplayMode);
        this.SetPickerSelectedItem(this.pickerWordExampleDisplayMode, setting.WordExampleDisplayMode);

        var wordPronunciationBracketDisplayModeNames = ControlDisplay.WordPronunciationBracketDisplayModeNames.Values;

        this.pickerWordPronunciationBracketMode.Items.AddRange(wordPronunciationBracketDisplayModeNames);

        foreach(var kp in ControlDisplay.WordPronunciationBracketDisplayModeNames)
        {
            if(kp.Key == setting.WordPronunciationBracketMode)
            {
                this.pickerWordPronunciationBracketMode.SelectedItem = kp.Value;
                break;
            }
        }       
    }

    private void SetPickerSelectedItem(Picker picker, ExpanderDisplayMode mode)
    {
        string name = this.GetExpanderDisplayModeName(mode);

        foreach (string item in picker.Items)
        {
            if (item == name)
            {
                picker.SelectedItem = item;
                break;
            }
        }
    }

    private void switchShowWordsWhileInputing_Toggled(object sender, ToggledEventArgs e)
    {
        this.setting.ShowWordsWhileInputing = e.Value;

        this.Save();
    }

    private void switchShowWordMeaningWhenShowWordList_Toggled(object sender, ToggledEventArgs e)
    {
        this.setting.ShowWordMeaningWhenShowWordList = e.Value;

        this.Save();
    }

    private void switchShowWordMeaningWhenShowVOCABs_Toggled(object sender, ToggledEventArgs e)
    {
        this.setting.ShowWordMeaningWhenShowVOCABs = e.Value;

        this.Save();
    }

    private void switchEnableLog_Toggled(object sender, ToggledEventArgs e)
    {
        this.setting.EnableLog = e.Value;

        this.Save();
    }

    private void switchShowWordFullMeaning_Toggled(object sender, ToggledEventArgs e)
    {
        this.setting.ShowWordFullMeaning = e.Value;

        this.Save();
    }  

    private void switchShowWordSyllabe_Toggled(object sender, ToggledEventArgs e)
    {
        this.setting.ShowWordSyllable = e.Value;

        this.Save();
    }

    private void switchAutoPlayAudioWhenLearnWord_Toggled(object sender, ToggledEventArgs e)
    {
        this.setting.AutoPlayAudioWhenLearnWord = e.Value;

        this.Save();
    }

    private void Save()
    {
        SettingManager.SaveSetting(this.setting);
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

    private void pickerWordInflectionDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.setting.WordInflectionDisplayMode = this.GetExpanderDisplayMode(this.pickerWordInflectionDisplayMode.SelectedItem?.ToString());

        this.Save();
    }

    private void pickerWordMediaDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.setting.WordMediaDisplayMode = this.GetExpanderDisplayMode(this.pickerWordMediaDisplayMode.SelectedItem?.ToString());

        this.Save();
    }

    private void pickerWordExampleDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.setting.WordExampleDisplayMode = this.GetExpanderDisplayMode(this.pickerWordExampleDisplayMode.SelectedItem?.ToString());

        this.Save();
    }

    private void pickerWordFormDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.setting.WordFormDisplayMode = this.GetExpanderDisplayMode(this.pickerWordFormDisplayMode.SelectedItem?.ToString());

        this.Save();
    }

    private void pickerWordStructureDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.setting.WordStructureDisplayMode = this.GetExpanderDisplayMode(this.pickerWordStructureDisplayMode.SelectedItem?.ToString());

        this.Save();
    }

    private void pickerWordVariantDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.setting.WordVariantDisplayMode = this.GetExpanderDisplayMode(this.pickerWordVariantDisplayMode.SelectedItem?.ToString());

        this.Save();
    }

    private ExpanderDisplayMode GetExpanderDisplayMode(string name)
    {
        foreach (var kp in ControlDisplay.ExpanderDisplayModeNames)
        {
            if (kp.Value == name)
            {
                return kp.Key;
            }
        }

        return ExpanderDisplayMode.Expanded;
    }

    private string GetExpanderDisplayModeName(ExpanderDisplayMode mode)
    {
        foreach (var kp in ControlDisplay.ExpanderDisplayModeNames)
        {
            if (kp.Key == mode)
            {
                return kp.Value;
            }
        }

        return string.Empty;
    }

    private void pickerWordPronunciationBracketMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        foreach(var kp in ControlDisplay.WordPronunciationBracketDisplayModeNames)
        {
            if(kp.Value == this.pickerWordPronunciationBracketMode.SelectedItem?.ToString())
            {
                this.setting.WordPronunciationBracketMode = kp.Key;

                this.Save();

                break;
            }
        }       
    }   
}