using System;

namespace EnglishLearning.Model
{
    public class V_MediaAccessHistory
    {
        public int Id { get; set; }
        public int MediaId { get; set; }
        public string MediaTitle { get; set; }
        public int PlatformId { get; set; }
        public string Url { get; set; }
        public string Source { get; set; }
        public string ImageUrl { get; set; }
        public string PositionTime { get; set; }
        public string Duration { get; set; }
        public DateTime LastAccessTime { get; set; }
    }
}
