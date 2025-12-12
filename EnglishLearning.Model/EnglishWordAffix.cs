namespace EnglishLearning.Model
{
    public class EnglishWordAffix: EnglishWordElement
    {
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
