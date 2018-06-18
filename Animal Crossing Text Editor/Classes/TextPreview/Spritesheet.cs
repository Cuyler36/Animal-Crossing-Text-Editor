using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Animal_Crossing_Text_Editor.Classes.TextPreview
{
    public class Spritesheet
    {
        public readonly BitmapSource SpriteSheet;
        public readonly int SpriteWidth;
        public readonly int SpriteHeight;
        public readonly int SpriteSizeX;
        public readonly int SpriteSizeY;

        public Spritesheet(BitmapSource spriteSheet, int spriteWidth, int spriteHeight, int spriteSizeX, int spriteSizeY)
        {
            SpriteSheet = spriteSheet;
            SpriteWidth = spriteWidth;
            SpriteHeight = spriteHeight;
            SpriteSizeX = spriteSizeX;
            SpriteSizeY = spriteSizeY;
        }

        public BitmapSource GetSprite(int SpriteIndex, Color FontColor, out int CopyWidth, int HorizontalScale = 0)
        {
            int HorizontalSpriteCount = SpriteSheet.PixelWidth / SpriteWidth;
            int VerticalSpriteCount = SpriteSheet.PixelHeight / SpriteHeight;

            int SpriteXIndex = SpriteIndex % HorizontalSpriteCount;
            int SpriteYIndex = SpriteIndex / HorizontalSpriteCount;

            int SpriteStartXCoordinate = SpriteXIndex * SpriteWidth;
            int SpriteStartYCoordinate = SpriteYIndex * SpriteHeight;

            int BytesPerPixel = SpriteSheet.Format.BitsPerPixel / 8;
            CopyWidth = SpriteWidth - HorizontalScale * 2;
            byte[] SpriteData = new byte[SpriteWidth * SpriteHeight * BytesPerPixel];
            Int32Rect CopyRectangle = new Int32Rect(SpriteStartXCoordinate, SpriteStartYCoordinate, CopyWidth, SpriteHeight);
            SpriteSheet.CopyPixels(CopyRectangle, SpriteData, CopyWidth * BytesPerPixel, 0);

            // Set Colors
            if (FontColor != null)
            {
                for (int i = 0; i < SpriteData.Length; i += BytesPerPixel)
                {
                    SpriteData[i + 2] = (byte)(((float)SpriteData[i + 2] / 255) * FontColor.R);
                    SpriteData[i + 1] = (byte)(((float)SpriteData[i + 1] / 255) * FontColor.G);
                    SpriteData[i + 0] = (byte)(((float)SpriteData[i + 0] / 255) * FontColor.B);
                }
            }

            BitmapSource Sprite = BitmapSource.Create(CopyWidth, SpriteHeight, 96, 96, SpriteSheet.Format, SpriteSheet.Palette,
                SpriteData, BytesPerPixel * CopyWidth);
            return Sprite;
        }
    }
}