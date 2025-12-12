namespace EnglishLearning.Model
{
    public class EnglishWordAffixStatistic
    {
        public int Id { get; set; }
        public int AffixId { get; set; }
        public string Content { get; set; }   
        public string ExcludeContent { get; set; }
        public int Priority { get; set; }
    }

    public class EnglishWordPrefixStatistic: EnglishWordAffixStatistic
    {
       
    }

    public class EnglishWordSuffixStatistic: EnglishWordAffixStatistic
    {

    }
}
