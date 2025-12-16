using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using EnglishLearning.Business;
using EnglishLearning.Business.Manager;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using Microsoft.Maui.Controls.Shapes;
using zoft.MauiExtensions.Core.Extensions;

namespace EnglishLearning.App.Views;

public partial class WordLearnSetting : ContentPage
{
    private SettingInfo setting;
    private PopupOptions popupOptions = new PopupOptions() { Shadow = null, Shape = new RoundRectangle() { CornerRadius = new CornerRadius(0, 0, 0, 0) } };
   
    public WordLearnSetting()
	{
		InitializeComponent();

		this.Init();       
    }

	private async void Init()
	{
        this.setting = SettingManager.GetSetting();

        string[] sortModes = new string[] { "按字母顺序升序", "按加入时间升序", "按加入时间降序" };

        this.pickerWordVOCABSortMode.Items.AddRange(sortModes);

        this.pickerWordVOCABSortMode.SelectedIndex = (int)setting.WordVOCABLearnSortMode;

        this.pickerPhraseVOCABSortMode.Items.AddRange(sortModes);

        this.pickerPhraseVOCABSortMode.SelectedIndex = (int)setting.PhraseVOCABLearnSortMode;
    }

    private void pickerWordVOCABSortMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.setting.WordVOCABLearnSortMode =(EnglishVOCABLearnSortMode) this.pickerWordVOCABSortMode.SelectedIndex;

        this.Save();
    }
    
    private void pickerPhraseVOCABSortMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.setting.PhraseVOCABLearnSortMode = (EnglishVOCABLearnSortMode)this.pickerPhraseVOCABSortMode.SelectedIndex;

        this.Save();
    }

    private void Save()
    {
        SettingManager.SaveSetting(this.setting);
    }

    private async void TapGestureRecognizer_ClearWordLearnedHistoryTapped(object sender, TappedEventArgs e)
    {
        var examTypes = await DataProcessor.GetEnglishExamTypes();

        List<ListItemInfo> items = new List<ListItemInfo>();
        items.AddRange(examTypes.Select(item=> new ListItemInfo() { Id = item.Id, Name = item.Name }));
        items.Add(new ListItemInfo() { Name = "非考级" });

        var popup = new MultiSelectListItem("选择范围", items);

        popup.OnPromptConfirm += this.Popup_OnPromptConfirm; 

        await this.ShowPopupAsync(popup, this.popupOptions);       
    }

    private async Task<bool> Popup_OnPromptConfirm(List<int?> ids)
    {
        bool confirmed = await DisplayAlert("询问?", "确定要清除背单词历史记录吗？", "是", "否");

        if (confirmed)
        {
            int affectedRows = await DataProcessor.ClearEnglishWordLearnedHistories(ids);

            if (affectedRows > 0)
            {
                await DisplayAlert("信息", $"记录已被清除。", "确定");
                return true;
            }
            else
            {
                await DisplayAlert("信息", $"未清除任何信息。", "确定");
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private async void TapGestureRecognizer_ClearPhraseLearnedHistoryTapped(object sender, TappedEventArgs e)
    {
        bool confirmed = await DisplayAlert("询问?", "确定要清除背短语历史记录吗？", "是", "否");

        if (confirmed)
        {
            int affectedRows = await DataProcessor.ClearEnglishPhraseLearnedHistories();

            if (affectedRows > 0)
            {
                await DisplayAlert("信息", $"记录已被清除。", "确定");               
            }
            else
            {
                await DisplayAlert("信息", $"未清除任何信息。", "确定");               
            }
        }
    }
}