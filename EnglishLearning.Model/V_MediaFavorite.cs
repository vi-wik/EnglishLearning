using System;

namespace EnglishLearning.Model
{
    public class V_MediaFavorite : V_EnglishMedia
    {
        public int Id { get; set; }      
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
