namespace EnglishLearning.Model
{
    public class EnglishWordAffix
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ExcludeName { get; set; }
        public bool Hidden { get; set; }
        public bool OnlyShowWithExamType { get; set; }
    }

    public class EnglishWordPrefix: EnglishWordAffix
    {
       
    }

    public class EnglishWordSuffix : EnglishWordAffix
    {

    }
}
