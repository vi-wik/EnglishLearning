using EnglishLearning.Model;

namespace EnglishLearning.App.Views;

public partial class My : ContentPage
{
	public My()
	{
		InitializeComponent();
	}

    private async void TapGestureRecognizer_HistoryTapped(object sender, TappedEventArgs e)
    {
        History history = (History)Activator.CreateInstance(typeof(History));

        await Navigation.PushAsync(history);
    }

    private async void TapGestureRecognizer_VOCABTapped(object sender, TappedEventArgs e)
    {
        VOCAB vocab = (VOCAB)Activator.CreateInstance(typeof(VOCAB));

        await Navigation.PushAsync(vocab);
    }

    private async void TapGestureRecognizer_FavoriteTapped(object sender, TappedEventArgs e)
    {
        Favorite favorite = (Favorite)Activator.CreateInstance(typeof(Favorite));

        await Navigation.PushAsync(favorite);
    }

    private async void tbiSetting_Clicked(object sender, EventArgs e)
    {
        Setting setting = (Setting)Activator.CreateInstance(typeof(Setting));

        await Navigation.PushAsync(setting);
    }
}