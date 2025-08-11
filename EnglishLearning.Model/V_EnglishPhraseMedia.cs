namespace EnglishLearning.Model
{
    public class V_EnglishPhraseMedia : V_EnglishMedia
    {
        public int Id { get; set; }
        public int PhraseId { get; set; }
        public string Phrase { get; set; }
        public int Priority { get; set; }
    }
}
