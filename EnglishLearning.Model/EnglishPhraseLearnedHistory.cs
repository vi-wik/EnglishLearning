using System;

namespace EnglishLearning.Model
{
    public class EnglishPhraseLearnedHistory
    {
        public int Id { get; set; }
        public int PhraseId { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
