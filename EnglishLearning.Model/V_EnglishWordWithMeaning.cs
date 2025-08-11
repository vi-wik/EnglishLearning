namespace EnglishLearning.Model
{
    public class V_EnglishWordWithMeaning
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string CommonMeaning { get; set; }
        public string SpecialMeaning { get; set; }
        public int? ExamType { get; set; }

        public int ExamTypeOrder 
        {
            get
            {
                if(!this.ExamType.HasValue)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string Meaning
        {
            get
            {
                return this.CommonMeaning + (string.IsNullOrEmpty(this.SpecialMeaning) ? "" : "；") + this.SpecialMeaning ?? "";
            }
        }
    }
}
