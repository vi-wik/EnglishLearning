namespace EnglishLearning.Model
{
    public class V_EnglishWordForm
    {
        public int Id { get; set; }
        public string WordId { get; set; }
        public string Word {  get; set; }
        public int TargetWordId { get; set; }
        public string TargetWord { get; set; }      
        public int? RuleId { get; set; }
        public int Priority { get; set; }
        public string PartOfSpeech { get; set; }
        public string CommonMeaning { get; set; }

        public string Meaning 
        {
            get
            {
                return $"{this.PartOfSpeech}{(this.PartOfSpeech == null ? "" : ".")}{this.CommonMeaning}";
            }
        }
    }
}
