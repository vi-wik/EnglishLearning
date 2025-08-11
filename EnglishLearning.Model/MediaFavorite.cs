using System;

namespace EnglishLearning.Model
{
    public class MediaFavorite
    {
        public int Id { get; set; }
        public int MediaId { get; set; }      
        public int CategoryId { get; set; }    
      
        public DateTime CreateTime { get; set; }    
    }
}
