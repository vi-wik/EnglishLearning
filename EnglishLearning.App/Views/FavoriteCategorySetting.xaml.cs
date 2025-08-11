using CommunityToolkit.Maui.Views;
using EnglishLearning.App.Helper;
using EnglishLearning.App.Model;
using EnglishLearning.Business;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;
using System.Xml.Linq;

namespace EnglishLearning.App.Views;

public partial class FavoriteCategorySetting : ContentPage
{
    public FavoriteCategorySetting()
    {
        InitializeComponent();

        this.LoadData();
    }

    private async void LoadData()
    {
        var categories = await DataProcessor.GetMediaFavoriteCategories();

        List<MediaFavoriteCategoryForEditing> editableCategories = categories.Select(item => new MediaFavoriteCategoryForEditing()
        {
            Id = item.Id,
            Name = item.Name,
            Priority = item.Priority,
            CanDelete = item.CanDelete
        }).ToList();

        this.lvCategories.ItemsSource = editableCategories;
    }

    private async void btnAdd_Clicked(object sender, EventArgs e)
    {
        var popup = new Prompt("新建收藏夹", "名称", "", new EventArgsInfo() { IsAdd = true });

        popup.OnPromptConfirm += Popup_OnPromptConfirm;

        var result = await this.ShowPopupAsync(popup);

        if(result == null)
        {
            return;
        }

        var name = result.ToString();

        if (!string.IsNullOrEmpty(name))
        {
            MediaFavoriteCategory category = new MediaFavoriteCategory() { Name = name };

            bool success = await DataProcessor.AddMediaFavoriteCategory(category);

            if (success)
            {
                MessageHelper.ShowToastMessage("添加成功。");

                this.LoadData();
            }
            else
            {
                await DisplayAlert("提示", "添加失败！", "确定");
            }
        }
    }

    private async Task<bool> Popup_OnPromptConfirm(string content, object args)
    {
        EventArgsInfo info = args as EventArgsInfo;

        if (info != null)
        {
            bool isNameExisting = await DataProcessor.IsMediaFavoriteCategoryNameExisting(info.IsAdd, content, info.IsAdd ? default(int?) : info.Id);

            if (isNameExisting)
            {
                await DisplayAlert("提示", "该名称已存在！", "确定");
                return false;
            }

            return true;
        }

        return false;
    }

    private async void btnDelete_Clicked(object sender, EventArgs e)
    {
        List<int> ids = new List<int>();

        foreach (MediaFavoriteCategoryForEditing item in this.lvCategories.ItemsSource)
        {
            if (item.IsSelected)
            {
                ids.Add(item.Id);
            }
        }

        if (ids.Count == 0)
        {
            await DisplayAlert("提示", "请选择要删除的记录！", "确定");
            return;
        }
        else
        {
            bool isByRefering = await DataProcessor.IsMediaFavoriteCategoryBeRefered(ids);

            if (isByRefering)
            {
                await DisplayAlert("提示", "你选中的记录中有的正在被引用，不能删除！", "确定");
                return;
            }

            bool confirmed = await DisplayAlert("询问?", "确定要删除选择的记录吗？", "是", "否");

            if (confirmed)
            {
                int affectedRows = 0;

                affectedRows = await DataProcessor.DeleteMediaFavoriteCategoriesByIds(ids);

                this.LoadData();

                await DisplayAlert("信息", $"已删除{affectedRows}条记录。", "确定");
            }
        }
    }

    private async void EditButton_Clicked(object sender, EventArgs e)
    {
        ImageButton button = sender as ImageButton;

        var category = button.BindingContext as MediaFavoriteCategoryForEditing;

        if (category == null)
        {
            return;
        }

        var popup = new Prompt("重命名收藏夹", "名称", category.Name, new EventArgsInfo() { IsAdd = false, Id = category.Id });

        popup.OnPromptConfirm += Popup_OnPromptConfirm;

        var result = await this.ShowPopupAsync(popup);

        if(result == null)
        {
            return;
        }

        var name = result.ToString();

        if (!string.IsNullOrEmpty(name))
        {
            bool success = await DataProcessor.RenameMediaFavoriteCategory(category.Id, name);

            if (success)
            {
                MessageHelper.ShowToastMessage("修改成功。");

                this.LoadData();
            }
            else
            {
                await DisplayAlert("提示", "修改失败！", "确定");
            }
        }
    }

    private class EventArgsInfo
    {
        internal bool IsAdd { get; set; }
        internal int Id { get; set; }
    }
}
