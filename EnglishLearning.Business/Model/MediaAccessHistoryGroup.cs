using EnglishLearning.Model;
using System.Collections.ObjectModel;

namespace EnglishLearning.Business.Model
{
    public class MediaAccessHistoryGroup : ObservableCollection<EnglishMediaForEditing>
    {
        public string Name { get; set; }
    

        public MediaAccessHistoryGroup(string name, ObservableCollection<EnglishMediaForEditing> medias) : base(medias)
        {
            this.Name = name;          
        }
    }
}
