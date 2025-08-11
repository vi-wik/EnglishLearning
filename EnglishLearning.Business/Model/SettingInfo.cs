namespace EnglishLearning.Business.Model
{
    public class SettingInfo
    {
        public bool ShowWordsWhileInputing { get; set; } = true;
        public bool ShowWordMeaningWhenShowWordList { get; set; } = true;
        public bool ShowWordMeaningWhenShowVOCABs { get; set; }
        public bool EnableLog { get; set; }
        public int PronunciationBracketMode { get; set; } = 1;
        public bool ShowWordFullMeaning { get; set; }
        public string PronunciationFileRootFolder { get; set; }
    }
}
