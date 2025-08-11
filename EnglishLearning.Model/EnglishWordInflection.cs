namespace EnglishLearning.Model
{
    public class EnglishWordInflection
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public string Word { get; set; }
        public int TypeId { get; set; }
        public int? TargetWordId { get; set; }
        public int Priority { get; set; }
        public int? RuleId { get; set; }
    }
}
