namespace EnglishLearning.Model
{
    public class V_EnglishWordVariant
    {
        public int Id { get; set; }
        public string WordId { get; set; }
        public string Word {  get; set; }
        public int TargetWordId { get; set; }
        public string TargetWord { get; set; }      
        public int? RuleId { get; set; }
        public int Priority { get; set; }
    }
}
