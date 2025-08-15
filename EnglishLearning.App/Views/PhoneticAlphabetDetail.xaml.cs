using EnglishLearning.Business;
using EnglishLearning.Business.Model;
using EnglishLearning.Model;

namespace EnglishLearning.App.Views;

public partial class PhoneticAlphabetDetail : ContentPage
{
    public object Content { get; set; }   


    public PhoneticAlphabetDetail()
    {
        InitializeComponent();

        this.LoadData();
    }

    public PhoneticAlphabetDetail(object content)
    {
        InitializeComponent();

        this.Content = content;

        this.LoadData();
    }

    private async void LoadData()
    {
        if(this.Content == null)
        {
            return;
        }       

        if (this.Content is EnglishConsonant consonant)
        {
            this.Title = "¸¨Òô£º" + consonant.Consonant;
            this.lblDescription.Text = consonant.Description;
            this.lblDescription.IsVisible = !string.IsNullOrEmpty(consonant.Description);

            var medias = await DataProcessor.GetVEnglishConsonantMedias(consonant.Id);

            var groupedMedias = (from item in medias 
                                group item by new { SubcategoryName = item.SubcategoryName?? "Overview", Description = item.SubcategoryDescription, Priority= item.SubcategoryPriority } into gp 
                                select new { Key = gp.Key, Value= gp.Select(item=>item) });

            List<EnglishConsonantMediaGroup> groups = new List<EnglishConsonantMediaGroup>();

            foreach (var gp in groupedMedias)
            {
                var key = gp.Key;
                var subcategoryMedias = gp.Value;

                EnglishConsonantMediaGroup group = new EnglishConsonantMediaGroup(key.SubcategoryName, key.Description,key.Priority, subcategoryMedias.OrderBy(item=>item.Priority).ToList());

                groups.Add(group);
            }           

            this.lvMedias.ItemsSource = groups.OrderBy(item=>item.Priority);
        }
        else if (this.Content is EnglishVowel vowel)
        {
            this.Title = "ÔªÒô£º" + vowel.Vowel;
            this.lblDescription.Text = vowel.Description;
            this.lblDescription.IsVisible = !string.IsNullOrEmpty(vowel.Description);

            var medias = await DataProcessor.GetVEnglishVowelMedias(vowel.Id);

            var groupedMedias = (from item in medias
                                 group item by new { SubcategoryName = item.SubcategoryName ?? "Overview", Description = item.SubcategoryDescription, Priority = item.SubcategoryPriority } into gp
                                 select new { Key = gp.Key, Value = gp.Select(item => item) });

            List<EnglishVowelMediaGroup> groups = new List<EnglishVowelMediaGroup>();

            foreach (var gp in groupedMedias)
            {
                var key = gp.Key;
                var subcategoryMedias = gp.Value;

                EnglishVowelMediaGroup group = new EnglishVowelMediaGroup(key.SubcategoryName, key.Description, key.Priority, subcategoryMedias.OrderBy(item => item.Priority).ToList());

                groups.Add(group);
            }

            this.lvMedias.ItemsSource = groups;
        }
    }
}