namespace EnglishLearning.Model
{
    public class V_EnglishWordMeaning: EnglishWordMeaning
    {
        public int? ExamType { get; set; }

        public int ExamTypeOrder
        {
            get
            {
                if (!this.ExamType.HasValue)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
