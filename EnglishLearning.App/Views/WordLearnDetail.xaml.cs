using EnglishLearning.Business;
using EnglishLearning.Model;
using Microsoft.Maui.Controls.Shapes;

namespace EnglishLearning.App.Views;

public partial class WordLearnDetail : ContentPage
{
    private bool isLoaded = false;
    private List<EnglishExamTypeWordLearnedStatisticInfo> statistics;

    public WordLearnDetail()
	{
		InitializeComponent();

        this.LoadData();

        this.isLoaded = true;
    }

    private async void LoadData()
    {
        int itemsCountOfEachRow = 2;

        var examTypes = (await DataProcessor.GetEnglishExamTypes()).ToList();
        this.statistics = await this.GetData();

        int count = examTypes.Count();
        int rowCount = count % itemsCountOfEachRow == 0 ? count / itemsCountOfEachRow : count / itemsCountOfEachRow + 1;

        RowDefinition[] rowDefinations = new RowDefinition[rowCount];

        for (int i = 0; i < rowDefinations.Length; i++)
        {
            rowDefinations[i] = new RowDefinition(new GridLength(0, GridUnitType.Star));
        }

        this.gvExamType.RowDefinitions = new RowDefinitionCollection(rowDefinations);

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
                border.Padding = new Thickness(5, 5);
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

                var statisticInfo = this.statistics.FirstOrDefault(item => item.Id == examType.Id);

                Label statisticLabel = new Label();
                statisticLabel.BindingContext = statisticInfo;
                statisticLabel.Margin = new Thickness(0, 5);
                statisticLabel.HorizontalOptions = LayoutOptions.Center;
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

    private string GetStatisticDisplayText(EnglishExamTypeWordLearnedStatisticInfo statisticInfo)
    {
        return $"({statisticInfo.LearnedCount}/{statisticInfo.Total})";
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        this.RefreshData();
    }

    private async Task<List<EnglishExamTypeWordLearnedStatisticInfo>> GetData()
    {
        return (await DataProcessor.GetEnglishExamTypeWordLearnedStatistics()).ToList();
    }

    private async void RefreshData()
    {
        if (!this.isLoaded)
        {
            return;
        }

        this.statistics = await this.GetData();

        var controls = this.gvExamType.GetVisualTreeDescendants();

        foreach (var control in controls)
        {
            if (control is Label)
            {
                Label label = control as Label;

                if (label.BindingContext is EnglishExamTypeWordLearnedStatisticInfo info)
                {
                    var statisticInfo = this.statistics.FirstOrDefault(item => item.Id == info.Id);

                    if (statisticInfo != null)
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

    private async void ShowDetail(object content)
    {
        EnglishExamType examType = content as EnglishExamType;

        int? wordId = await DataProcessor.GetEnglishWordNotLearnedNextId(examType);

        if (wordId > 0)
        {
            WordDetail wordDetail = (WordDetail)Activator.CreateInstance(typeof(WordDetail), wordId.Value, examType);

            await Navigation.PushAsync(wordDetail);
        }
        else
        {
            var statisticInfo = this.statistics.FirstOrDefault(item => item.Id == examType.Id);

            if (statisticInfo != null && statisticInfo.Total > 0 && statisticInfo.Total == statisticInfo.LearnedCount)
            {
                await DisplayAlert("提示", "该科目已学完。", "确定");
            }
        }
    }
}