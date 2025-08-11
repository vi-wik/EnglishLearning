using System.Collections.Generic;

namespace EnglishLearning.Model
{
    public class EnglishTopicDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }       
        public string Description { get; set; }
        public int TopicId { get; set; }
        public int Priority { get; set; }
    }
}
