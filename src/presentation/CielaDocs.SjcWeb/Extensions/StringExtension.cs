namespace CielaDocs.SjcWeb.Extensions
{
    static class StringExtension
    {
        public static bool ContainsWord(this string s, string word)
        {
           
            string[] ar = s.Split(';');
           
                foreach (string str in ar)
                {
                    if (str.ToLower() == word.ToLower())
                        return true;
                }
            
            return false;
        }
        public static int IndexOfWholeWord(this string str, string word)
        {
            for (int j = 0; j < str.Length &&
                (j = str.IndexOf(word, j, StringComparison.Ordinal)) >= 0; j++)
                if ((j == 0 || !char.IsLetterOrDigit(str, j - 1)) &&
                    (j + word.Length == str.Length || !char.IsLetterOrDigit(str, j + word.Length)))
                    return j;
            return -1;
        }

    }
}
