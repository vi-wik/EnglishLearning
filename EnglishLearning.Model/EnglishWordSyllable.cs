namespace EnglishLearning.Model
{
    public class EnglishWordSyllable
    {
        public int Id { get; set; }
        public int WordId { get; set;}
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public bool IsStressed { get; set; }
        public int Priority { get; set; }
    }
}
