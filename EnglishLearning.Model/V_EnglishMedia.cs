using System.Collections.Generic;

namespace EnglishLearning.Model
{
    public class V_EnglishMedia
    {
        private string title;
        public int MediaId { get; set; }
        public string MediaTitle 
        { 
            get
            {
                return !string.IsNullOrEmpty(this.MediaTitleExt) ? this.MediaTitleExt : this.title;
            }
            set { this.title = value; }
        }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public int MediaTypeId { get; set; }
        public string MediaTypeName { get; set; }
        public int PlatformId { get; set; }
        public string PlatformName { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get;set; }
        public string Source { get; set; }
        public string Duration { get; set; }
        public string MediaDescription { get; set; }
        public string MediaTitleExt { get; set; }
        public string MediaDescriptionExt { get; set; }       

        public List<EnglishMediaPlayTime> PlayTimes { get; set; }
    }    
}
