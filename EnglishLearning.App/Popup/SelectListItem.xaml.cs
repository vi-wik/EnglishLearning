using CommunityToolkit.Maui.Views;
using EnglishLearning.App.Helper;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnglishLearning.App.Views;

public delegate Task<bool> SelectListItemHandler(int id);

public partial class SelectListItem : Popup
{
    public event SelectListItemHandler OnPromptConfirm;

    public SelectListItem(string title, IEnumerable<ListItemInfo> items)
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

        if(selectedId.HasValue && this.OnPromptConfirm != null)
        {
            bool success = await this.OnPromptConfirm(selectedId.Value);

            if(success)
            {
                await CloseAsync();
            }
        }        
    }

    private async void btnCancel_Clicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}

public class ListItemInfo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private bool isSelected;

    public int? Id { get; set; }
    public string Name { get; set; }
    public bool IsSelected
    {
        get
        {
            return this.isSelected;
        }

        set
        {
            if (value != this.isSelected)
            {
                this.isSelected = value;
                NotifyPropertyChanged();
            }
        }
    }

    protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}