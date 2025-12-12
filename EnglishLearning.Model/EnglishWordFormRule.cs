namespace EnglishLearning.Model
{
    public class EnglishWordFormRule
    {
        public int Id { get; set; }
        public string Expression { get; set; }  
        public bool IsInsertBefore { get; set; }
        public bool IsAppend { get; set; }
        public bool IsRepeatLastChar { get; set; }
        public bool IsChange { get; set; }
        public bool IsChangeEnd { get; set; }
        public bool IsKeepFirst { get; set; }
        public bool IsTrimEnd { get; set; }
        public string InsertBeforeContent { get; set; }
        public string AppendContent { get; set; }
        public string ChangeOldContent { get; set; }
        public string ChangeNewContent { get; set; }
        public string TrimEndContent { get; set; }
        public string Description { get; set; }
        public int? PrefixId { get; set; }
        public int? SuffixId { get; set; }
    }
}
