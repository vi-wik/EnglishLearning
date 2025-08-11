using System.Collections.Generic;
using System.Linq;

namespace EnglishLearning.Model
{
    public class UserData
    {
        public IEnumerable<MediaFavoriteCategory> MediaFavoriteCategories { get; set; } = Enumerable.Empty<MediaFavoriteCategory>();
        public IEnumerable<MediaFavorite> MediaFavorites { get; set; } = Enumerable.Empty<MediaFavorite>();
        public IEnumerable<MediaAccessHistory> MediaAccessHistories { get; set; } = Enumerable.Empty<MediaAccessHistory>();
        public IEnumerable<VOCAB> VOCABs { get; set; } = Enumerable.Empty<VOCAB>();
        public IEnumerable<EnglishWordLearnHistory> WordLearnHistories { get; set; } = Enumerable.Empty<EnglishWordLearnHistory>();
    }
}
