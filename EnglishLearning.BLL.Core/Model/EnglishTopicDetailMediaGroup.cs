using EnglishLearning.Model;

namespace EnglishLearning.BLL.Core.Model
{
    public class EnglishTopicDetailMediaGroup:List<V_EnglishTopicDetailMedia>
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public EnglishTopicDetailMediaGroup(string name, List<V_EnglishTopicDetailMedia> medias) : base(medias)
        {
            Name = name;
        }
    }
}
