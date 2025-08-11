namespace EnglishLearning.Model
{
    public class EnglishWordAffixDetail
    {
        public int Id { get; set; }
        public int AffixId { get; set; }
        public string Content { get; set; }   
        public string ExcludeContent { get; set; }
        public int Priority { get; set; }
    }

    public class EnglishWordPrefixDetail: EnglishWordAffixDetail
    {
       
    }

    public class EnglishWordSuffixDetail : EnglishWordAffixDetail
    {

    }
}
