namespace EnglishLearning.Model
{
    public class V_EnglishConsonantMediaPlayTime: EnglishMediaPlayTime
    {
        public int Id { get; set; }
        public int ConsonantMediaId { get; set; }
        public int WordId { get; set; }
        public int MediaId { get; set; }
    }
}
