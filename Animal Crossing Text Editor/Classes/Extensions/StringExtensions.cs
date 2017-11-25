using System.Collections.Generic;

namespace Animal_Crossing_Text_Editor
{
    public static class StringExtensions
    {
        // From https://stackoverflow.com/questions/14115503/measuring-the-length-of-string-containing-wide-characters
        public static IEnumerable<string> TextElements(this string s)
        {
            var en = System.Globalization.StringInfo.GetTextElementEnumerator(s);

            while (en.MoveNext())
            {
                yield return en.GetTextElement();
            }
        }

        public static int GetWideLength(this string s)
        {
            return new System.Globalization.StringInfo(s).LengthInTextElements;
        }
    }
}