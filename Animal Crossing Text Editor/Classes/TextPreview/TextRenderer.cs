using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Animal_Crossing_Text_Editor.Classes.TextPreview
{
    public class TextRenderer
    {
        public readonly Spritesheet CharacterSheet;

        public TextRenderer(BitmapSource SpriteSheet, int CharacterWidth, int CharacterHeight, int CharacterSizeX, int CharacterSizeY)
        {
            CharacterSheet = new Spritesheet(SpriteSheet, CharacterWidth, CharacterHeight, CharacterSizeX, CharacterSizeY);
        }

        public BitmapSource RenderText(string Text, Dictionary<byte, string> CharacterMap, string[] KanjiMap0 = null, string[] KanjiMap1 = null, int Scale = 1)
        {
            // TODO: e+ Kanji SpriteSheets need to be added
            // TODO: Handle Color Commands properly (color text as it should be)
            // TODO: Line Size & Offset Parsing

            // Replace "<String XX>" with "Dummy XX"
            for (int i = 0; i < 20; i++)
            {
                Text = Text.Replace("<String " + i.ToString() + ">", "Dummy " + i.ToString("D2"));
            }

            // Replace "<Item String X>" with "Dummy ItemName X"
            for (int i = 0; i < 5; i++)
            {
                Text = Text.Replace("<Item String " + i.ToString() + ">", "Dummy ItemName " + i.ToString());
            }

            // Replace Year, Month, Day, Hour, Minute, & Second
            Text = Text.Replace("<Year>", DateTime.Now.Year.ToString("D4"));
            Text = Text.Replace("<Month>", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
            Text = Text.Replace("<Day>", TextRenderUtility.GetDayString(DateTime.Now.Day));
            Text = Text.Replace("<Day of Week>", DateTime.Now.DayOfWeek.ToString());
            Text = Text.Replace("<Hour>", DateTime.Now.Hour == 0 ? "12" : (DateTime.Now.Hour % 12).ToString());
            Text = Text.Replace("<Minute>", DateTime.Now.Minute.ToString("D2"));
            Text = Text.Replace("<Second>", DateTime.Now.Second.ToString("D2"));
            Text = Text.Replace("<AM/PM>", DateTime.Now.Hour > 11 ? "PM" : "AM");

            // Replace Catchphrase with a default catchphrase
            Text = Text.Replace("<Catchphrase>", "meow");

            // Replace Random Number with a random number 0-99
            Text = Text.Replace("<Random Number>", new Random().Next(0, 100).ToString());

            // Replace Player & Town & NPC Names
            Text = Text.Replace("<Player Name>", "Player");
            Text = Text.Replace("<Town Name>", "T-Town");
            Text = Text.Replace("<NPC Name>", "NPC");

            // Replace Gryroid Message
            Text = Text.Replace("<Gyroid Message>", "This is a test gyroid\nmessage.");

            Text = Text.Replace("<Island Name>", "Outset");
            Text = Text.Replace("<Last Choice Selected>", "Last Choice Selected");

            // Remove all command data (except for Color)
            Text = Text.Replace("\r", "");
            Regex CommandRegex = new Regex(@"<[^>]+>");
            string CommandStrippedText = CommandRegex.Replace(Text, "").TrimEnd();

            // Begin generating the entire window BitmapSource
            BitmapSource DialogWindow = null;
            int WindowWidth = 0, WindowHeight = CharacterSheet.SpriteHeight;

            // Calculate BitmapSource Dimensions
            int CurrentWidth = 0;
            foreach (char Character in CommandStrippedText)
            {
                if (Character == '\n')
                {
                    if (CurrentWidth > WindowWidth)
                    {
                        WindowWidth = CurrentWidth;
                    }

                    WindowHeight += CharacterSheet.SpriteHeight;
                    CurrentWidth = 0;
                }
                else
                {
                    CurrentWidth += CharacterSheet.SpriteWidth;
                }
            }

            if (CurrentWidth > WindowWidth)
            {
                WindowWidth = CurrentWidth;
            }

            if (WindowHeight == 0)
            {
                WindowHeight = CharacterSheet.SpriteHeight;
            }

            WindowWidth *= Scale;
            WindowHeight *= Scale;

            // If no characters are to be rendered, return null
            if (WindowWidth <= 0)
            {
                return null;
            }

            int BytesPerPixel = CharacterSheet.SpriteSheet.Format.BitsPerPixel / 8;

            // Allocate full text pixel buffer
            byte[] TextPixelData = new byte[WindowWidth * WindowHeight * BytesPerPixel];

            // Copy sprites into TextPixelData buffer
            int CurrentHeight = 0;
            CurrentWidth = 0;
            foreach (char Character in CommandStrippedText)
            {
                // Get character sprite
                if (!Character.Equals('\n'))
                {
                    BitmapSource CharacterSprite = null;
                    var KeyValPair = CharacterMap.FirstOrDefault(o => o.Value.Equals(Character.ToString()));
                    if (KeyValPair.Equals(default(KeyValuePair<byte, string>)))
                    {
                        // TODO: Implement Kanji SpriteSheets
                        if (KanjiMap0 != null && KanjiMap0.Contains(Character.ToString()))
                        {
                            CharacterSprite = CharacterSheet.GetSprite(Array.IndexOf(KanjiMap0, Character.ToString()));
                        }
                        else if (KanjiMap1 != null && KanjiMap1.Contains(Character.ToString()))
                        {
                            CharacterSprite = CharacterSheet.GetSprite(Array.IndexOf(KanjiMap1, Character.ToString()));
                        }
                    }
                    else
                    {
                        CharacterSprite = CharacterSheet.GetSprite(KeyValPair.Key);
                    }

                    // Copy character sprite into correct place in buffer
                    if (CharacterSprite != null)
                    {
                        CharacterSprite.CopyPixels(TextPixelData, WindowWidth * BytesPerPixel,
                            (CurrentWidth * BytesPerPixel) + (CurrentHeight * (WindowWidth * BytesPerPixel)));
                    }
                    CurrentWidth += CharacterSheet.SpriteSizeX;
                }
                else
                {
                    CurrentWidth = 0;
                    CurrentHeight += CharacterSheet.SpriteSizeY;
                }
            }

            // Set color to black (TODO: REMOVE THIS AFTER CORRECT COLOR IMPLEMENTATION EARLIER IN THE PROCESS)
            for (int i = 0; i < TextPixelData.Length; i += BytesPerPixel)
            {
                TextPixelData[i] = (byte)(255 - TextPixelData[i]);
                TextPixelData[i + 1] = (byte)(255 - TextPixelData[i + 1]);
                TextPixelData[i + 2] = (byte)(255 - TextPixelData[i + 2]);
            }

            // Create Dialog Window BitmapSource
            DialogWindow = BitmapSource.Create(WindowWidth, WindowHeight, 96, 96, CharacterSheet.SpriteSheet.Format, CharacterSheet.SpriteSheet.Palette,
                TextPixelData, WindowWidth * BytesPerPixel);

            return DialogWindow;
        }
    }
}
