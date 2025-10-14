using EnglishLearning.App.Views;
using EnglishLearning.Business;
using EnglishLearning.Business.Model;
using System.Text;

namespace EnglishLearning.App.Controls;

public partial class EnglishExampleView : ContentView
{
    public EnglishExampleView()
    {
        InitializeComponent();
    }

    private void Example_BindingContextChanged(object sender, EventArgs e)
    {
        Label label = sender as Label;

        label.FormattedText = new FormattedString();

        EnglishExampleDisplay display = label.BindingContext as EnglishExampleDisplay;

        string example = display.Example;

        StringBuilder sb = new StringBuilder();

        int count = 0;
        int total = example.Count();

        Action addSpan = () =>
        {
            string text = sb.ToString();

            Span span = new Span() { Text = text };

            if (text.ToUpper() == display.Vocabulary?.ToUpper())
            {
                span.TextColor = Colors.Blue;
            }
            else if (text.Length > 1)
            {
                var tapGestureRecognizer = new TapGestureRecognizer() { NumberOfTapsRequired = 1, CommandParameter = text };
                tapGestureRecognizer.Tapped += this.TapGestureRecognizer_ExampleItemTapped;

                span.GestureRecognizers.Add(tapGestureRecognizer);
            }

            label.FormattedText.Spans.Add(span);
        };

        foreach (var c in example)
        {
            count++;

            if (char.IsLetter(c) || c == '-')
            {
                sb.Append(c);

                bool isEnd = count == total;

                if (isEnd)
                {
                    addSpan();
                }
            }
            else
            {
                addSpan();

                label.FormattedText.Spans.Add(new Span() { Text = c.ToString() });

                sb.Clear();
            }
        }
    }

    private async void TapGestureRecognizer_ExampleItemTapped(object sender, TappedEventArgs e)
    {
        string word = e.Parameter.ToString();

        int? wordId = (await DataProcessor.GetEnglishWordIdsByWords([word]))?.FirstOrDefault();

        if (wordId.HasValue && wordId > 0)
        {
            WordDetail wordDetail = (WordDetail)Activator.CreateInstance(typeof(WordDetail), wordId.Value);

            await Navigation.PushAsync(wordDetail);
        }
    } 
}