namespace EnglishLearning.Model
{
    public class EnglishPlatform
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
    }

    public enum EnglishPlatformType
    {
        bilibili = 1,
        ximalaya = 2
    }
}
