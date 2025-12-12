namespace EnglishLearning.Model
{
    public class EnglishWordStructure
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public int TypeId { get; set; }
        public int? PrefixId { get; set; }
        public int? SuffixId { get; set; }
        public int? SubWordId { get; set; }
        public int? RootId { get; set; }
        public int? ConnectorId { get; set; }
        public int Priority { get; set; }
    }
}
