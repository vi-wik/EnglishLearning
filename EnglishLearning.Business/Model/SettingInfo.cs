using EnglishLearning.Model;

namespace EnglishLearning.Business.Model
{
    public class SettingInfo
    {
        public bool ShowWordsWhileInputing { get; set; } = true;
        public bool ShowWordMeaningWhenShowWordList { get; set; } = true;
        public bool ShowWordMeaningWhenShowVOCABs { get; set; }
        public bool EnableLog { get; set; }       
        public bool ShowWordFullMeaning { get; set; }
        public ExpanderDisplayMode WordInflectionDisplayMode { get; set; } = ExpanderDisplayMode.Expanded;
        public ExpanderDisplayMode WordMediaDisplayMode { get; set; } = ExpanderDisplayMode.Expanded;
        public ExpanderDisplayMode WordExampleDisplayMode { get; set; } = ExpanderDisplayMode.Expanded;
        public ExpanderDisplayMode WordFormDisplayMode { get; set; } = ExpanderDisplayMode.Expanded;
        public ExpanderDisplayMode WordStructureDisplayMode { get; set; } = ExpanderDisplayMode.Expanded;
        public ExpanderDisplayMode WordVariantDisplayMode { get; set; } = ExpanderDisplayMode.Expanded;
        public WordPronunciationBracketDisplayMode WordPronunciationBracketMode { get; set; } = WordPronunciationBracketDisplayMode.Square;
        public bool ShowWordSyllable { get; set; } = true;
        public bool AutoPlayAudioWhenLearnWord { get; set; }
        public string PronunciationFileRootFolder { get; set; }
        public EnglishVOCABLearnSortMode WordVOCABLearnSortMode { get; set; }
        public EnglishVOCABLearnSortMode PhraseVOCABLearnSortMode { get; set; }
    }   
}
