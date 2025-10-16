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
                    return 2;
                }
                else
                {
                    return 1;
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

        public int MeaningOrder
        {
            get
            {
                if(!string.IsNullOrEmpty(this.Meaning))
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
        }

        public int MeaningPriority { get; set; }
        
    }
}
