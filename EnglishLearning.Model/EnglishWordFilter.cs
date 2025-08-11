namespace EnglishLearning.Model
{
    public class EnglishWordFilter
    {
        public string Keyword { get; set; }
        public bool FullMatch { get; set; }
        public bool NeedMeaning { get; set; }
        public bool IgnoreCase { get; set; } = true;
        public bool IsMatchPrefix { get; set; }
        public bool IsMatchSuffix { get; set; }
        public int? LimitCount { get; set; } = 100;
        public bool NoLimit { get; set; } = false;
        public string NotBeginWith { get; set; }
        public string NotEndWith { get; set; }
        public bool MustHaveMeaning { get; set; }
    }

    public class EnglishWordMeaningFilter
    {
        public bool ShowSpecialMeaning { get; set; }
    }
}
