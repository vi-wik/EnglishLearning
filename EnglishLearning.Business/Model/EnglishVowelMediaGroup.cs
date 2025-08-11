using EnglishLearning.Model;

namespace EnglishLearning.Business.Model
{
    public class EnglishVowelMediaGroup : List<V_EnglishVowelMedia>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }

        public EnglishVowelMediaGroup(string name, string description, int priority, List<V_EnglishVowelMedia> medias) : base(medias)
        {
            this.Name = name;
            this.Description = description;
            this.Priority = priority; 
        }
    }
}
