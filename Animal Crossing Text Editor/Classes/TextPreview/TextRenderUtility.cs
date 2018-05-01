using System.Drawing;
using System.Windows;
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
