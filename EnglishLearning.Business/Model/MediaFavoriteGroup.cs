using EnglishLearning.Model;

namespace EnglishLearning.Business.Model
{
    public class MediaFavoriteGroup : List<V_MediaFavorite>
    {
        public string CategoryName { get; set; }
    

        public MediaFavoriteGroup(string categoryName, List<V_MediaFavorite> favorites) : base(favorites)
        {
            this.CategoryName = categoryName;          
        }
    }
}
