using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }
    }
}
