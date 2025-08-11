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
        var popup = new Prompt("�½��ղؼ�", "����", "", new EventArgsInfo() { IsAdd = true });

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
                MessageHelper.ShowToastMessage("��ӳɹ���");

                this.LoadData();
            }
            else
            {
                await DisplayAlert("��ʾ", "���ʧ�ܣ�", "ȷ��");
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
                await DisplayAlert("��ʾ", "�������Ѵ��ڣ�", "ȷ��");
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
            await DisplayAlert("��ʾ", "��ѡ��Ҫɾ���ļ�¼��", "ȷ��");
            return;
        }
        else
        {
            bool isByRefering = await DataProcessor.IsMediaFavoriteCategoryBeRefered(ids);

            if (isByRefering)
            {
                await DisplayAlert("��ʾ", "��ѡ�еļ�¼���е����ڱ����ã�����ɾ����", "ȷ��");
                return;
            }

            bool confirmed = await DisplayAlert("ѯ��?", "ȷ��Ҫɾ��ѡ��ļ�¼��", "��", "��");

            if (confirmed)
            {
                int affectedRows = 0;

                affectedRows = await DataProcessor.DeleteMediaFavoriteCategoriesByIds(ids);

                this.LoadData();

                await DisplayAlert("��Ϣ", $"��ɾ��{affectedRows}����¼��", "ȷ��");
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

        var popup = new Prompt("�������ղؼ�", "����", category.Name, new EventArgsInfo() { IsAdd = false, Id = category.Id });

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
                MessageHelper.ShowToastMessage("�޸ĳɹ���");

                this.LoadData();
            }
            else
            {
                await DisplayAlert("��ʾ", "�޸�ʧ�ܣ�", "ȷ��");
            }
        }
    }

    private class EventArgsInfo
    {
        internal bool IsAdd { get; set; }
        internal int Id { get; set; }
    }
}
