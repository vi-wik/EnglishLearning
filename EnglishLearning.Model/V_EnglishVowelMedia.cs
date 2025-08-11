namespace EnglishLearning.Model
{
    public class V_EnglishVowelMedia : V_EnglishMedia
    {
        public int Id { get; set; }
        public int VowelId { get; set; }
        public string Vowel { get; set; }
        public int Priority { get; set; }
        public string SubcategoryName { get; set; }
        public string SubcategoryDescription { get; set; }
        public int SubcategoryPriority { get; set; }
    }
}
