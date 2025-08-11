namespace EnglishLearning.Model
{
    public class DataSortInfo
    {
        public string FieldName { get; set; }
        public DataSortType SortType { get; set; }
    }

    public enum DataSortType
    {
        ASC = 0,
        DESC = 1      
    }
}
