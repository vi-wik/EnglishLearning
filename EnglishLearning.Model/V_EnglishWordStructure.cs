namespace EnglishLearning.Model
{
    public class V_EnglishWordStructure : EnglishWordStructure
    {
        public string Word { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string SubWord { get; set; }
        public string Root { get; set; }     
        public string Connector { get; set; }

        public string ChangeEndOldContent { get; set; }
    }
}
