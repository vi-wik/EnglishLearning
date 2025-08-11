namespace EnglishLearning.Model
{
    public class EnglishTopic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Name_EN   { get; set; }
        public string Description { get; set; }
        public int SubjectId { get; set; }
        public int Priority { get; set; }
    }
}
