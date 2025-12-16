namespace EnglishLearning.Business.Model
{
    public class ControlDisplay
    {
        public static Dictionary<ExpanderDisplayMode, string> ExpanderDisplayModeNames = new Dictionary<ExpanderDisplayMode, string>()
        {
            { ExpanderDisplayMode.Expanded, "展开" },
            { ExpanderDisplayMode.Collapsed, "折叠" },
            { ExpanderDisplayMode.Hidden, "隐藏" }
        };

        public static Dictionary<WordPronunciationBracketDisplayMode, string> WordPronunciationBracketDisplayModeNames = new Dictionary<WordPronunciationBracketDisplayMode, string>()
        {
            { WordPronunciationBracketDisplayMode.Square, "中括号"},
            { WordPronunciationBracketDisplayMode.Slash, "斜杠"}
        };
    }

    public enum ExpanderDisplayMode
    {
        Expanded = 1,
        Collapsed = 2,
        Hidden = 3
    }

    public enum WordPronunciationBracketDisplayMode
    {
        Square=1,
        Slash =2
    }
}
