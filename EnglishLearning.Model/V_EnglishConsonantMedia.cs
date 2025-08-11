namespace EnglishLearning.Model
{
    public class V_EnglishConsonantMedia : V_EnglishMedia
    {
        public int Id { get; set; }
        public int ConsonantId { get; set; }
        public string Consonant { get; set; }
        public int Priority { get; set; }
        public string SubcategoryName { get; set; }
        public string SubcategoryDescription { get; set; }
        public int SubcategoryPriority { get; set; }
    }
}
