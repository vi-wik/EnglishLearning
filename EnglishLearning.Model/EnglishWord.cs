namespace EnglishLearning.Model
{
    public class EnglishWord
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public int? ExamType { get; set; }       
        public bool? HasDegree { get; set; }
    }

    public class EnglishWordSuggestion : EnglishWord
    {
        public string Meaning { get; set; }
    }
}