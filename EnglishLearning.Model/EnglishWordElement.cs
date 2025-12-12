namespace EnglishLearning.Model
{
    public class EnglishWordElement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExcludeName { get; set; }
        public bool UseStatisticForDetail { get; set; }
        public bool UseFormForDetail { get; set; }
    }

    public enum EnglishWordElementType
    {
        None = 0,
        Prefix = 1,
        Suffix = 2,
        WordRoot = 3
    }
}
