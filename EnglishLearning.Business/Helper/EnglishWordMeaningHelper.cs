namespace EnglishLearning.Business.Helper
{
    public class EnglishWordMeaningHelper
    {
        public static List<string> SplitValue(string value)
        {
            var items = value.Split(';', '；');

            List<string> results = new List<string>();

            foreach (var item in items)
            {
                var subItems = ParseValue(item);

                results.AddRange(subItems);
            }

            return results;
        }

        public static IEnumerable<string> ParseValue(string value)
        {
            List<string> items = new List<string>();

            int squareBracketsCount = 0;
            int parenthesisCount = 0;
            int angleBracketsCount = 0;
            int angleBracketsCount2 = 0;

            int index = value.IndexOf('，');

            if (index >= 0)
            {
                int start = 0;
                int count = 0;

                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];

                    if (c == '[' || c == ']')
                    {
                        squareBracketsCount++;
                    }
                    else if (c == '（' || c == '）')
                    {
                        parenthesisCount++;
                    }
                    else if (c == '<' || c == '>')
                    {
                        angleBracketsCount++;
                    }
                    else if (c == '〈' || c == '〉')
                    {
                        angleBracketsCount2++;
                    }

                    if ((c == '，') && i > 0)
                    {
                        if (squareBracketsCount % 2 == 0 && parenthesisCount % 2 == 0 && angleBracketsCount % 2 == 0 && angleBracketsCount2 % 2 == 0)
                        {
                            items.Add(value.Substring(start, count));
                            start = i + 1;
                            count = 0;

                            int lastCommaIndex = value.LastIndexOf('，');

                            if (i == lastCommaIndex)
                            {
                                items.Add(value.Substring(i + 1));
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    count++;
                }
            }

            if (items.Count == 0)
            {
                items.Add(value);
            }

            return items.Select(item => GetCleanValueSubItem(item));
        }

        public static string GetCleanValueSubItem(string value)
        {
            int index = value.IndexOf(']');

            string str = value;

            if (index > 0)
            {
                str = value.Substring(index + 1);
            }
            else
            {
                index = value.IndexOf('>');

                if (index == -1)
                {
                    index = value.IndexOf('〉');
                }

                if (index > 0)
                {
                    str = value.Substring(index + 1);
                }
            }

            if (str.StartsWith("（"))
            {
                index = str.LastIndexOf('）');

                if (index > 0)
                {
                    return str.Substring(index + 1).Trim();
                }
            }
            else if (str.EndsWith("）"))
            {
                index = str.LastIndexOf('（');

                if (index >= 0)
                {
                    return str.Substring(0, index).Trim();
                }
            }

            return str.Trim();
        }      
    }
}
