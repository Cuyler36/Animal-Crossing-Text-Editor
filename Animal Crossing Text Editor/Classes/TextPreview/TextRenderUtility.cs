using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Animal_Crossing_Text_Editor.Classes.TextPreview
{
    public static class TextRenderUtility
    {
        public static readonly byte[] AFeCharacterWidthAdjustments = new byte[256]
        {
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            08, 09, 07, 02, 02, 02, 04, 09, 07, 07, 03, 02, 08, 04, 08, 02,
            04, 07, 04, 04, 04, 04, 04, 05, 04, 04, 08, 03, 05, 02, 05, 05,
            03, 04, 05, 04, 05, 05, 06, 04, 05, 09, 07, 05, 05, 03, 05, 04,
            05, 04, 05, 05, 05, 05, 04, 02, 05, 05, 05, 03, 01, 02, 02, 04,
            02, 06, 06, 06, 06, 06, 07, 06, 06, 09, 07, 06, 09, 03, 06, 05,
            06, 06, 07, 06, 07, 06, 05, 03, 06, 06, 06, 03, 04, 03, 02, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
        };

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

        public static BitmapSource Convert(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public static Bitmap Convert(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static Bitmap Resize(Bitmap Input, float Scale)
        {
            var ScaledWidth = (int)(Input.Width * Scale);
            var ScaledHeight = (int)(Input.Height * Scale);

            var ScaledBitmap = new Bitmap(ScaledWidth, ScaledHeight);
            using (var g = Graphics.FromImage(ScaledBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                g.DrawImage(Input, 0, 0, ScaledWidth, ScaledHeight);
                g.Flush();
            }

            return ScaledBitmap;
        }

        public static BitmapSource Resize(BitmapSource Input, float Scale)
        {
            var InputBitmap = Convert(Input);
            var ScaledBitmap = Resize(InputBitmap, Scale);
            var ScaledBitmapSource = Convert(ScaledBitmap);

            InputBitmap.Dispose();
            ScaledBitmap.Dispose();

            return ScaledBitmapSource;
        }
    }
}
