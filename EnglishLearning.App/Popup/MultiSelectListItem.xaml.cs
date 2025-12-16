using CommunityToolkit.Maui.Views;
using EnglishLearning.App.Helper;

namespace EnglishLearning.App.Views;

public delegate Task<bool> MultiSelectListItemHandler(List<int?> ids);

public partial class MultiSelectListItem : Popup
{
    public event MultiSelectListItemHandler OnPromptConfirm;

    public MultiSelectListItem(string title, IEnumerable<ListItemInfo> items)
    {
        InitializeComponent();

        this.Margin = 0;
        this.Padding = 0;

        this.lblTitle.Text = title;

        this.lvItems.ItemsSource = items;
    }

    private async void OnCloseImageClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }

    private async void btnOK_Clicked(object sender, EventArgs e)
    {
        List<int?> selectedIds = new List<int?>();

        var controls = this.lvItems.GetVisualTreeDescendants();

        foreach (var control in controls)
        {
            if (control is CheckBox chk)
            {
                if (chk.IsChecked)
                {
                    ListItemInfo item = chk.BindingContext as ListItemInfo;

                    selectedIds.Add(item.Id);
                }
            }
        }

        if (selectedIds.Count == 0)
        {
            MessageHelper.ShowToastMessage("ÇëÑ¡Ôñ¼ÇÂ¼");
            return;
        }

        if (this.OnPromptConfirm != null)
        {
            bool success = await this.OnPromptConfirm(selectedIds);

            if (success)
            {
                await CloseAsync();
            }
        }
    }

    private async void btnCancel_Clicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }

    private void chkSelectAll_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        IEnumerable<ListItemInfo> items = this.lvItems.ItemsSource as IEnumerable<ListItemInfo>;

        if (items != null)
        {
            foreach(var item in items)
            {
                item.IsSelected = e.Value;
            }
        }
    }
}
