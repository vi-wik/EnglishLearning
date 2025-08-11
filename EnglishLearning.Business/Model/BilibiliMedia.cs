namespace EnglishLearning.Business.Model
{
    public class BilibiliMediaDetailsInfo
    {
        public BilibiliMediaDetailsData data { get; set; }
    }

    public class BilibiliMediaDetailsData
    {
        public int timelength { get; set; }
        public BilibiliMediaDetailsDataUrl[] durl { get; set; }
    }

    public class BilibiliMediaDetailsDataUrl
    {
        public string url { get; set; }
    }
}
