namespace EnglishLearning.Model
{
    public class EnglishPhrase
    {
        public int Id { get; set; }
        public string Phrase { get; set; }
        public string Abbreviation { get; set; }
        public string Meaning { get; set; }
        public int PhraseTypeId { get; set; }
        public string Synonym { get; set; }
    }
}
