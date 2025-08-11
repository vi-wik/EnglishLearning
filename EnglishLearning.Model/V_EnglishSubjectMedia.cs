namespace EnglishLearning.Model
{
    public class V_EnglishSubjectMedia : V_EnglishMedia
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int Priority { get; set; }
    }
}
