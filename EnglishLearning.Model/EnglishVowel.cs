namespace EnglishLearning.Model
{
    public class EnglishVowel
    {
        public int Id { get; set; }
        public string Vowel { get; set; }
        public string UKVowel { get; set; }
        public string USVowel { get; set; }
        public string UKVowelAlt { get; set; }
        public string USVowelAlt { get; set; }
        public int TypeId { get; set; }
        public int VoiceTypeId { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
    }
}
