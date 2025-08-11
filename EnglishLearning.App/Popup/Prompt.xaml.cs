using CommunityToolkit.Maui.Views;
using EnglishLearning.App.Helper;
namespace EnglishLearning.App.Views;

public delegate Task<bool> PromptHandler(string content, object args);

public partial class Prompt : Popup
{
    public event PromptHandler OnPromptConfirm;

    string title;
    string defaultContent;
    string contentName;
    object args;    

    public Prompt(string title, string contentName = null, string defaultContent = "", object args = null)
    {
        InitializeComponent();     


        this.title = title;
        this.defaultContent = defaultContent;
        this.contentName = contentName;
        this.args = args;

        this.lblTitle.Text = title;

        if (!string.IsNullOrEmpty(contentName))
        {
            this.lblName.Text = contentName;
        }
    }

    private async void OnCloseImageClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }

    private void Popup_Opened(object sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e)
    {
        if (!string.IsNullOrEmpty(defaultContent))
        {
            this.txtContent.Text = defaultContent;
        }
    }

    private string GetContentName()
    {
        return this.contentName ?? "内容";
    }

    private async void btnOK_Clicked(object sender, EventArgs e)
    {
        string content = this.txtContent.Text.Trim();

        string contentName = this.GetContentName();

        if (string.IsNullOrEmpty(content))
        {
            MessageHelper.ShowToastMessage($"请输入{contentName}！");
            return;
        }
        else if(content == this.defaultContent)
        {
            MessageHelper.ShowToastMessage($"{contentName}与原{contentName}相同！");
            return;
        }

        if (this.OnPromptConfirm != null)
        {
            bool isValid = await this.OnPromptConfirm(content, args);

            if (isValid)
            {
                await CloseAsync(content);
            }
        }
        else
        {
            await CloseAsync(content);
        }
    }

    private async void btnCancel_Clicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}