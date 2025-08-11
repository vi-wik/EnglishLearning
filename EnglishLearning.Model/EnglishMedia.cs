namespace EnglishLearning.Model
{
    public class EnglishMedia
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public int MediaTypeId { get; set; }      
        public int PlatformId { get; set; }       
        public int TeacherId { get; set; }      
        public string Source { get; set; }      
        public string Duration { get; set; }
    }    
}
