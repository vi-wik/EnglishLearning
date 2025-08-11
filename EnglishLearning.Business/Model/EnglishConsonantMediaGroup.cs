using EnglishLearning.Model;

namespace EnglishLearning.Business.Model
{
    public class EnglishConsonantMediaGroup : List<V_EnglishConsonantMedia>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }

        public EnglishConsonantMediaGroup(string name, string description, int priority, List<V_EnglishConsonantMedia> medias) : base(medias)
        {
            this.Name = name;
            this.Description = description;
            this.Priority = priority; 
        }
    }
}
