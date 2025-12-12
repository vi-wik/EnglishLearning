using EnglishLearning.Business;
using EnglishLearning.Business.Helper;
using EnglishLearning.Model;
using System.Threading.Tasks;

namespace EnglishLearning.App.Views;

public partial class WordRootAffixDetail : ContentPage
{
    public WordRootAffixDetail(EnglishWordElement element, EnglishWordElementType type)
    {
        InitializeComponent();

        this.LoadData(element, type);      
    }

    private async Task LoadData(EnglishWordElement element, EnglishWordElementType type)
    {
        if (type != EnglishWordElementType.None)
        {
            IEnumerable<V_EnglishWordWithMeaning> words = null;

            if(!element.UseFormForDetail)
            {
                words = (await DataProcessor.GetEnglishWordByRootAffix(element.Id, type)).OrderBy(item => item.Word);
            }
            else
            {
                words = await DataProcessor.GetEnglishWordsByForm(element.Name, type, 500);
            }

            this.lvWord.ItemsSource = words;
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Grid grid = sender as Grid;

        var word = grid.BindingContext as V_EnglishWordWithMeaning;

        if (word != null)
        {
            WordDetail wordDetail = (WordDetail)Activator.CreateInstance(typeof(WordDetail), word.Id);

            await Navigation.PushAsync(wordDetail);
        }
    }   
}