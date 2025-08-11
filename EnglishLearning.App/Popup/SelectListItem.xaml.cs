using CommunityToolkit.Maui.Views;
using EnglishLearning.App.Helper;

namespace EnglishLearning.App.Views;

public partial class SelectListItem : Popup
{
    public SelectListItem(string title, IEnumerable<ListItemInfo> items)
    {
        InitializeComponent();

        this.lblTitle.Text = title;

        this.lvItems.ItemsSource = items;
    }

    private async void OnCloseImageClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }

    private async void btnOK_Clicked(object sender, EventArgs e)
    {
        int? selectedId = default(int?);

        var controls = this.lvItems.GetVisualTreeDescendants();

        foreach (var control in controls)
        {
            if (control is RadioButton rb)
            {
                if (rb.IsChecked)
                {
                    ListItemInfo item = rb.BindingContext as ListItemInfo;

                    selectedId = item.Id;

                    break;
                }
            }
        }

        if (!selectedId.HasValue)
        {
            MessageHelper.ShowToastMessage("ÇëÑ¡Ôñ¼ÇÂ¼");
            return;
        }

        await CloseAsync(selectedId.Value);
    }

    private async void btnCancel_Clicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}

public class ListItemInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsSelected { get; set; }
}