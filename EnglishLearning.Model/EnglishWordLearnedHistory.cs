using System;
namespace EnglishLearning.Model
{
    public class EnglishWordLearnedHistory
    {
        public int Id { get; set; }     
        public int WordId { get; set; }
        public DateTime CreateTime { get; set; }      
    }
}
