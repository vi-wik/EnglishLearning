using System;
namespace EnglishLearning.Model
{
    public class EnglishWordLearnHistory
    {
        public int Id { get; set; }
        public int ExamTypeId { get; set; }
        public int WordId { get; set; }
        public DateTime CreateTime { get; set; }      
    }
}
