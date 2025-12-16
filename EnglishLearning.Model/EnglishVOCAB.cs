using System;

namespace EnglishLearning.Model
{
    public class EnglishVOCAB
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class EnglishWordVOCAB: EnglishVOCAB
    {
        public int WordId { get; set; }       
    }

    public class EnglishPhraseVOCAB:EnglishVOCAB
    {
        public int PhraseId { get; set; }
    }
}
