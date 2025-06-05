namespace CielaDocs.SjcWeb.Extensions
{
    static class StringExtension
    {
        public static bool ContainsWord(this string s, string word)
        {

            //string[] ar = s.Split(';');

            //    foreach (string str in ar)
            //    {
            //        if (str.ToLower() == word.ToLower())
            //            return true;
            //    }

            //return false;
            if (string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(word))
                return false;

            return s.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Any(str => string.Equals(str, word, StringComparison.OrdinalIgnoreCase));
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
