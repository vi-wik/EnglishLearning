namespace EnglishLearning.Model
{
    public class EnglishWordMeaning
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public string Word { get; set; }
        public int? PartOfSpeechId { get; set; }
        public string PartOfSpeech { get; set; }
        public string CommonMeaning { get; set; }
        public string SpecialMeaning { get; set; }
        public bool IsFromWeb { get; set; }      
        public bool IsOld { get; set; }
        public bool Special { get; set; }
        public bool Informal { get; set; }
        public bool Obsolete { get; set; }
        public int Priority { get; set; }

        public string Meaning
        {
            get
            {
                return this.CommonMeaning + (string.IsNullOrEmpty(this.SpecialMeaning)? "": "；") + this.SpecialMeaning ?? "";
            }
        }
    }
}
