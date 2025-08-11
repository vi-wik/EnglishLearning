namespace EnglishLearning.Model
{
    public class V_EnglishTopicDetailMedia : V_EnglishMedia
    {
        public int Id { get; set; }
        public int TopicId { get; set; }
        public int TopicDetailId { get; set; }
        public string TopicDetailName { get; set; }
        public int DetailPriority { get; set; }
        public int Priority { get; set; }
    }
}
