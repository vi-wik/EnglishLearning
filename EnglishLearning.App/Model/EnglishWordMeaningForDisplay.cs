using EnglishLearning.Model;

namespace EnglishLearning.App.Model
{
    public class EnglishWordMeaningForDisplay: EnglishWordMeaning
    {
        public object PartOfSpeechColumnWidth { get; set; }
        public new string Meaning { get; set; }
    }
}
