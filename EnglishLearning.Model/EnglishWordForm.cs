namespace EnglishLearning.Model
{
    public class EnglishWordForm
    {
        public int Id { get; set; }
        public int WordId { get; set; }      
        public int? TargetWordId { get; set; }
        public int Priority { get; set; }
        public int? RuleId { get; set; }
    }
}
