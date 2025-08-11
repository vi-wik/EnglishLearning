namespace EnglishLearning.Model
{
    public class V_EnglishVowelMediaPlayTime: EnglishMediaPlayTime
    {
        public int Id { get; set; }
        public int VowelMediaId { get; set; }
        public int WordId { get; set; }
        public int MediaId { get; set; }
    }
}
