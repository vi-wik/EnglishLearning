using System.Collections.Generic;
using System.Linq;

namespace EnglishLearning.Model
{
    public class UserData
    {
        public IEnumerable<MediaFavoriteCategory> MediaFavoriteCategories { get; set; } = Enumerable.Empty<MediaFavoriteCategory>();
        public IEnumerable<MediaFavorite> MediaFavorites { get; set; } = Enumerable.Empty<MediaFavorite>();
        public IEnumerable<MediaAccessHistory> MediaAccessHistories { get; set; } = Enumerable.Empty<MediaAccessHistory>();
        public IEnumerable<EnglishWordVOCAB> EnglishWordVOCABs { get; set; } = Enumerable.Empty<EnglishWordVOCAB>();
        public IEnumerable<EnglishPhraseVOCAB> EnglishPhraseVOCABs { get; set; } = Enumerable.Empty<EnglishPhraseVOCAB>();
        public IEnumerable<EnglishWordLearnedHistory> WordLearnedHistories { get; set; } = Enumerable.Empty<EnglishWordLearnedHistory>();
        public IEnumerable<EnglishPhraseLearnedHistory> PhraseLearnedHistories { get; set; } = Enumerable.Empty<EnglishPhraseLearnedHistory>();
    }
}
