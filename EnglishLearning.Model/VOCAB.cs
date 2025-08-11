using System;

namespace EnglishLearning.Model
{
    public class VOCAB
    {
        public int Id { get; set; }      
        public int? WordId { get; set; }
        public int? PhraseId { get; set; }
        public DateTime CreateTime { get; set; }      
    }
}
