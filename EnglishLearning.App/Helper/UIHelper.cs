using EnglishLearning.Model;

namespace EnglishLearning.App.Helper
{
    public class UIHelper
    {
        public static string MakeupPunctuation(string content, bool isEnglish)
        {
            char lastChar = content.Last();

            if (!char.IsPunctuation(lastChar))
            {
                return isEnglish ? "." : "。";
            }

            return string.Empty;
        }

        public static void SetEnglishWordSyllableDisplayText(Label label, string word, IEnumerable<EnglishWordSyllable> syllables)
        {
            label.FormattedText = new FormattedString();           

            string cleanWord = word.Replace("-", "").Replace(" ", "");

            int count = syllables.Count();
            int i = 0;

            foreach (var syllable in syllables)
            {
                string text = cleanWord.Substring(syllable.StartIndex, syllable.Length);

                Span span = new Span() { Text = text };

                if (syllable.IsStressed)
                {
                    span.FontAttributes = FontAttributes.Bold;
                }

                label.FormattedText.Spans.Add(span);

                if (i < count - 1)
                {
                    label.FormattedText.Spans.Add(new Span() { Text = "·" });
                }

                i++;
            }
        }
    }
}
