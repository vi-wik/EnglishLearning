namespace EnglishLearning.Model
{
    public class V_EnglishPhrase
    {
        public int Id { get; set; }
        public string Phrase { get; set; }

        public string Meaning { get; set; }
        public string TypeName { get; set; }
        public string TypeName_EN { get; set; }
        public string Synonym { get; set; }
        public string Abbreviation { get; set; }
    }

    public enum EnglishPhraseTypeName
    {
        Phrase = 1,
        Idom = 2,
        Slang = 3,
        Proverb = 4
    }
}
