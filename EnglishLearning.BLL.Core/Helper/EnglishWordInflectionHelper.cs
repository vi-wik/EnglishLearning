namespace EnglishLearning.BLL.Core.Helper
{
    public class EnglishWordInflectionHelper
    {
        public static string GetComparativeDegree(string word)
        {
            if (word.EndsWith("e"))
            {
                return word + "r";
            }
            else if (word.EndsWith("y"))
            {
                return word.Substring(0, word.Length - 1) + "ier";
            }
            else
            {
                return word + "er";
            }
        }

        public static string GetSuperlativeDegree(string word)
        {
            if (word.EndsWith("e"))
            {
                return word + "st";
            }
            else if (word.EndsWith("y"))
            {
                return word.Substring(0, word.Length - 1) + "iest";
            }
            else
            {
                return word + "est";
            }
        }
    }
}
