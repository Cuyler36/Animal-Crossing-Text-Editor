namespace Animal_Crossing_Text_Editor.Classes.TextPreview
{
    public static class TextRenderUtility
    {
        public static string GetDayString(int Day)
        {
            if (Day > 10 && Day < 14)
            {
                return Day.ToString() + "th";
            }
            else if (Day % 10 == 1)
            {
                return Day.ToString() + "st";
            }
            else if (Day % 10 == 2)
            {
                return Day.ToString() + "nd";
            }
            else if (Day % 10 == 3)
            {
                return Day.ToString() + "rd";
            }
            else
            {
                return Day.ToString() + "th";
            }
        }
    }
}
