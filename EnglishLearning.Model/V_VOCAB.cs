using System;

namespace EnglishLearning.Model
{
    public class V_VOCAB
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int WordId { get; set; }
        public int PhraseId { get; set; }
        public DateTime CreateTime { get; set; }
        public string Meaning { get; set; }

        public string TypeName
        {
            get
            {
                return this.Type == 1 ? "单词" : "短语";
            }
        }
    }
}
