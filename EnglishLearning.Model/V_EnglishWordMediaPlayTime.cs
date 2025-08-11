namespace EnglishLearning.Model
{
    public class V_EnglishWordMediaPlayTime: EnglishMediaPlayTime
    {
        public int Id { get; set; }
        public int WordMediaId { get; set; }
        public int WordId { get; set; }
        public int MediaId { get; set; }
    }
}
