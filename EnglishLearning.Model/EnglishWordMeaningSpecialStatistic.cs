namespace EnglishLearning.Model
{
    public class EnglishWordMeaningSpecialStatistic
    {
        public int SpecialRowCount { get; set; }
        public int SpecialColumnCount { get; set; }

        public bool HasSpecial
        {
            get
            {
                return this.SpecialRowCount>0 || this.SpecialColumnCount > 0;
            }
        }
    }
}
