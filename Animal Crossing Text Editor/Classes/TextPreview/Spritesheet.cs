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

        public BitmapSource GetSprite(int SpriteIndex, int Scale = 1)
        {
            int HorizontalSpriteCount = SpriteSheet.PixelWidth / SpriteWidth;
            int VerticalSpriteCount = SpriteSheet.PixelHeight / SpriteHeight;

            int SpriteXIndex = SpriteIndex % HorizontalSpriteCount;
            int SpriteYIndex = SpriteIndex / HorizontalSpriteCount;

            int SpriteStartXCoordinate = SpriteXIndex * SpriteWidth;
            int SpriteStartYCoordinate = SpriteYIndex * SpriteHeight;

            int BytesPerPixel = SpriteSheet.Format.BitsPerPixel / 8;
            byte[] SpriteData = new byte[SpriteWidth * SpriteHeight * BytesPerPixel];
            Int32Rect CopyRectangle = new Int32Rect(SpriteStartXCoordinate, SpriteStartYCoordinate, SpriteWidth, SpriteHeight);
            SpriteSheet.CopyPixels(CopyRectangle, SpriteData, SpriteWidth * BytesPerPixel, 0);

            BitmapSource Sprite = BitmapSource.Create(SpriteWidth, SpriteHeight, 96, 96, SpriteSheet.Format, SpriteSheet.Palette,
                SpriteData, BytesPerPixel * SpriteWidth);
            return Sprite;
        }
    }
}