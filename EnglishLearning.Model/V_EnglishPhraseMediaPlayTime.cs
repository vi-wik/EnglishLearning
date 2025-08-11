namespace EnglishLearning.Model
{
    public class V_EnglishPhraseMediaPlayTime: EnglishMediaPlayTime
    {
        public int Id { get; set; }
        public int PhraseMediaId { get; set; }
        public int PhraseId { get; set; }
        public int MediaId { get; set; }
    }
}
