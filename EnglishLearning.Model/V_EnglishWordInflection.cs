namespace EnglishLearning.Model
{
    public class V_EnglishWordInflection: EnglishWordInflection
    {
        public string TargetWord { get; set; }
        public string TypeName { get; set; }
        public int TypePriority { get; set; }
    }
}
