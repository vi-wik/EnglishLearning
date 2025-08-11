using EnglishLearning.Business;
using EnglishLearning.DataAccess;
using EnglishLearning.Model;
using Microsoft.Maui.Controls.Shapes;

namespace EnglishLearning.App.Views;

public partial class WordLearn : ContentPage
{
    private bool isLoaded = false;

	public WordLearn()
	{
		InitializeComponent();     

        this.LoadData();

        this.isLoaded = true;
    }

    private async void LoadData()
    {
        int itemsCountOfEachRow = 2;

        var examTypes = (await DataProcessor.GetEnglishExamTypes()).ToList();
        var statistics = (await DataProcessor.GetEnglishExamStatistics()).ToList();

        int count = examTypes.Count();
        int rowCount = count % itemsCountOfEachRow == 0 ? count / itemsCountOfEachRow : count / itemsCountOfEachRow + 1;

        RowDefinition[] consonantRowDefinations = new RowDefinition[rowCount];

        for (int i = 0; i < consonantRowDefinations.Length; i++)
        {
            consonantRowDefinations[i] = new RowDefinition(new GridLength(0, GridUnitType.Star));
        }

        this.gvExamType.RowDefinitions = new RowDefinitionCollection(consonantRowDefinations);

        int index = 0;

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < itemsCountOfEachRow; j++)
            {
                var examType = examTypes[index];

                Border border = new Border();
                border.Stroke = Colors.LightBlue;
                border.StrokeThickness = 1;
                border.Margin = new Thickness(10, 10);
                border.Padding = new Thickness(5,5);
                border.StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(4, 4, 4, 4)
                };

                border.BindingContext = examType;

                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.NumberOfTapsRequired = 1;
                tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;

                border.GestureRecognizers.Add(tapGestureRecognizer);

                VerticalStackLayout layout = new VerticalStackLayout();

                Label nameLabel = new Label() { Text = examType.Name };
                nameLabel.HorizontalOptions = LayoutOptions.Center;
                nameLabel.TextColor = Colors.Green;
                nameLabel.FontSize = 20;

                layout.Add(nameLabel);

                var statisticInfo = statistics.FirstOrDefault(item => item.Id == examType.Id);

                Label statisticLabel = new Label();
                statisticLabel.BindingContext = statisticInfo;
                statisticLabel.Margin = new Thickness(0, 5);
                statisticLabel.HorizontalOptions= LayoutOptions.Center;
                statisticLabel.Text = this.GetStatisticDisplayText(statisticInfo);
                statisticLabel.FontSize = 14;
                statisticLabel.TextColor = Colors.Gray;

                layout.Add(statisticLabel);

                border.Content = layout;               

                this.gvExamType.Add(border, j, i);

                index++;

                if (index >= count)
                {
                    break;
                }
            }
        }
    }

    private string GetStatisticDisplayText(EnglishExamStatisticInfo statisticInfo)
    {
        return $"({statisticInfo.LearnedCount}/{statisticInfo.Total})";
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        this.RefreshData();
    }

    private async void RefreshData()
    {
        if(!this.isLoaded)
        {
            return;
        }

        var statistics = (await DataProcessor.GetEnglishExamStatistics()).ToList();

        var controls = this.gvExamType.GetVisualTreeDescendants();

        foreach(var control in controls)
        {
            if(control is Label)
            {
                Label label = control as Label;

                if (label.BindingContext is EnglishExamStatisticInfo info)
                {
                    var statisticInfo = statistics.FirstOrDefault(item => item.Id == info.Id);

                    if(statisticInfo!=null)
                    {
                        label.Text = this.GetStatisticDisplayText(statisticInfo);
                    }                   
                }
            }
        }
    }

    private void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
    {
        var border = (sender as Border);

        if (border != null)
        {
            this.ShowDetail(border.BindingContext);
        }
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        var border = (sender as Border);

        if (border != null)
        {
            //this.ShowDetail(btn.CommandParameter);
        }
    }

    private async void ShowDetail(object content)
    {
        EnglishExamType examType = content as EnglishExamType;

        int wordId = await DataProcessor.GetEnglishWordLearnNextId(examType);

        if(wordId>0)
        {
            WordDetail wordDetail = (WordDetail)Activator.CreateInstance(typeof(WordDetail), wordId, examType);

            await Navigation.PushAsync(wordDetail);
        }       
    }
}