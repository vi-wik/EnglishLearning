namespace EnglishLearning.Model
{
    public class V_EnglishWord
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string US_Pronunciation { get; set; }
        public string UK_Pronunciation { get; set; }
        public string Meaning { get; set; }
    
        public int? ExamType { get; set; }    
        public bool? HasDegree { get; set; }
    }
}
