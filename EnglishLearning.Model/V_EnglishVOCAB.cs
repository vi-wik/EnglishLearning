using System;

namespace EnglishLearning.Model
{
    public class V_EnglishVOCAB:EnglishVOCAB
    {
        public string Name { get; set; }
        public string Meaning { get; set; }
    }

   
    public class V_EnglishWordVOCAB : V_EnglishVOCAB
    {       
        public int WordId { get; set; }
    }

    public class V_EnglishPhraseVOCAB : V_EnglishVOCAB
    {
        public int PhraseId { get; set; }
    }
}
