namespace EnglishLearning.Model
{
    public class V_EnglishWordMedia : V_EnglishMedia
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public string Word { get; set; }
        public int Priority { get; set; }
    }
}
