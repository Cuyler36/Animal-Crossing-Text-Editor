using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace Animal_Crossing_Text_Editor.Classes.TextPreview
{
    public class TextRenderer
    {
        internal enum VerticalAlignment
        {
            Top, Center, Bottom
        }

        public readonly Spritesheet CharacterSheet;
        internal Color Color = Color.Black;
        internal float Scale = 1;
        internal float Offset = 0;
        internal VerticalAlignment Alignment = VerticalAlignment.Top;

        public TextRenderer(BitmapSource SpriteSheet, int CharacterWidth, int CharacterHeight, int CharacterSizeX, int CharacterSizeY)
        {
            CharacterSheet = new Spritesheet(SpriteSheet, CharacterWidth, CharacterHeight, CharacterSizeX, CharacterSizeY);
        }

        public void Reset(List<Color> Colors = null)
        {
            if (Colors != null)
            {
                Color = Colors[0];
            }
            else
            {
                Color = Color.Black;
            }

            Scale = 1;
            Offset = 0;
            Alignment = VerticalAlignment.Top;
        }

        public BitmapSource RenderText(string Text, Dictionary<byte, string> CharacterMap, string[] KanjiMap0 = null, string[] KanjiMap1 = null,
            List<Color> Colors = null, int CharacterScale = 1)
        {
            // TODO: Line Offset Parsing
            Spritesheet KanjiSheet0 = null;
            Spritesheet KanjiSheet1 = null;

            if (KanjiMap0 != null && KanjiMap1 != null)
            {
                KanjiSheet0 = new Spritesheet(TextRenderUtility.Convert(Properties.Resources.AFe__Kanji_Bank_0), 24, 32, 14, 32);
                KanjiSheet1 = new Spritesheet(TextRenderUtility.Convert(Properties.Resources.AFe__Kanji_Bank_1), 24, 32, 14, 32);
            }

            string CommandStrippedText = TextUtility.ReplaceCommands(Text);

            // Begin generating the entire window BitmapSource
            BitmapSource DialogWindow = null;
            int WindowWidth = 0, WindowHeight = CharacterSheet.SpriteHeight;

            // Calculate BitmapSource Dimensions
            int CurrentWidth = 0;
            float CurrentLineScale = 1; // 4 times the needed amount, just in case.
            for (int ParseIndex = 0; ParseIndex < CommandStrippedText.Length; ParseIndex++)
            {
                char Character = CommandStrippedText[ParseIndex];

                // Check for command
                if (Character == '<')
                {
                    int CommandEndingOffset = CommandStrippedText.IndexOf('>', ParseIndex);
                    if (CommandEndingOffset > -1)
                    {
                        string Command = CommandStrippedText.Substring(ParseIndex, CommandEndingOffset - ParseIndex + 1);
                        if (Command.Contains("Line Size"))
                        {
                            var Match = Regex.Match(Command, @"\d+");
                            if (Match.Success && int.TryParse(Match.Value, out int LineSize))
                            {
                                CurrentLineScale = (float)LineSize / 100;
                            }
                        }

                        // Set ParseIndex to the end of the command so we skip the rest of it.
                        ParseIndex = CommandEndingOffset;
                        continue;
                    }
                }

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
                    CurrentWidth += (int)(CharacterSheet.SpriteWidth * CurrentLineScale);
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

            // If no characters are to be rendered, return null
            if (WindowWidth <= 0)
            {
                return null;
            }

            // Add 400 to Window Width & Height just in case (this will prevent errors with scale)
            WindowWidth += 100;
            WindowHeight += 100;

            WindowWidth *= CharacterScale;
            WindowHeight *= CharacterScale;

            int BytesPerPixel = CharacterSheet.SpriteSheet.Format.BitsPerPixel / 8;

            // Allocate full text pixel buffer
            byte[] TextPixelData = new byte[WindowWidth * WindowHeight * BytesPerPixel];

            // Copy sprites into TextPixelData buffer
            int CurrentHeight = 0;
            CurrentWidth = 0;
            for (int ParseIndex = 0; ParseIndex < CommandStrippedText.Length; ParseIndex++)
            {
                char Character = CommandStrippedText[ParseIndex];

                // Check for command
                if (Character == '<')
                {
                    int CommandEndingOffset = CommandStrippedText.IndexOf('>', ParseIndex);
                    if (CommandEndingOffset > -1)
                    {
                        string Command = CommandStrippedText.Substring(ParseIndex, CommandEndingOffset - ParseIndex + 1);
                        if (Colors != null && Command.Contains("<Line Color Index"))
                        {
                            var Match = Regex.Match(Command, @"\d+");
                            if (Match.Success && byte.TryParse(Match.Value, out byte ColorIndex) && Colors.Count > ColorIndex)
                            {
                                Color = Colors[ColorIndex];
                            }
                        }
                        else if (Command.Contains("Line Size"))
                        {
                            var Match = Regex.Match(Command, @"\d+");
                            if (Match.Success && int.TryParse(Match.Value, out int LineSize))
                            {
                                Scale = (float)LineSize / 100; // TODO: fix weird bug with CopyPixels
                            }
                        }
                        else if (Command.Contains("Line Alignment"))
                        {
                            if (Command.Contains("Top"))
                            {
                                Alignment = VerticalAlignment.Top;
                            }
                            else if (Command.Contains("Center"))
                            {
                                Alignment = VerticalAlignment.Center;
                            }
                            else if (Command.Contains("Bottom"))
                            {
                                Alignment = VerticalAlignment.Bottom;
                            }
                        }
                        else if (Command.Contains("Line Type"))
                        {
                            var Match = Regex.Match(Command, @"\d+");
                            if (Match.Success && byte.TryParse(Match.Value, out byte LineType))
                            {
                                if (LineType < 3)
                                {
                                    Alignment = (VerticalAlignment)LineType;
                                }
                            }
                        }

                        // Set ParseIndex to the end of the command so we skip the rest of it.
                        ParseIndex = CommandEndingOffset;
                        continue;
                    }
                }

                // Get character sprite
                if (!Character.Equals('\n'))
                {
                    BitmapSource CharacterSprite = null;
                    int SizeX = CharacterSheet.SpriteSizeX;
                    var KeyValPair = CharacterMap.FirstOrDefault(o => o.Value.Equals(Character.ToString()));
                    if (KeyValPair.Equals(default(KeyValuePair<byte, string>)))
                    {
                        if (KanjiMap0 != null && KanjiMap0.Contains(Character.ToString()))
                        {
                            CharacterSprite = KanjiSheet0.GetSprite(Array.IndexOf(KanjiMap0, Character.ToString()), Color, out SizeX);
                        }
                        else if (KanjiMap1 != null && KanjiMap1.Contains(Character.ToString()))
                        {
                            CharacterSprite = KanjiSheet1.GetSprite(Array.IndexOf(KanjiMap1, Character.ToString()), Color, out SizeX);
                        }
                    }
                    else
                    {
                        CharacterSprite = CharacterSheet.GetSprite(KeyValPair.Key, Color, out SizeX,
                            TextRenderUtility.AFeCharacterWidthAdjustments[KeyValPair.Key]);
                    }

                    // Copy character sprite into correct place in buffer
                    if (CharacterSprite != null)
                    {
                        if (Scale != 1)
                        {
                            try
                            {
                                CharacterSprite = TextRenderUtility.Resize(CharacterSprite, Scale);
                            }
                            catch { }
                        }

                        int Height = CurrentHeight;
                        if (Alignment == VerticalAlignment.Center)
                        {
                            Height = CurrentHeight + (CharacterSheet.SpriteSizeY / 2) - (CharacterSprite.PixelHeight / 2);
                        }
                        else if (Alignment == VerticalAlignment.Bottom)
                        {
                            Height = CurrentHeight + CharacterSheet.SpriteSizeY - CharacterSprite.PixelHeight;
                        }

                        if (Height < 0)
                            Height = 0;

                        try
                        {
                            CharacterSprite.CopyPixels(TextPixelData, WindowWidth * BytesPerPixel,
                                (CurrentWidth * BytesPerPixel) + (Height * (WindowWidth * BytesPerPixel)));
                        }
                        catch (Exception e)
                        {
                            /*Console.WriteLine("CopyPixels Error Data:\n" + e.Message + "\n\n" + e.StackTrace);
                            Console.WriteLine(string.Format("CopyPixels error:\nTextPixelData Length: 0x{0}\nStride: 0x{1}\nOffset: 0x{2}",
                                TextPixelData.Length.ToString("X"), (WindowWidth * BytesPerPixel).ToString("X"), 
                                ((CurrentWidth * BytesPerPixel) + (CurrentHeight * (WindowWidth * BytesPerPixel))).ToString("X")));*/
                        }
                    }
                    CurrentWidth += (int)(SizeX * Scale);
                }
                else
                {
                    CurrentWidth = 0;
                    CurrentHeight += CharacterSheet.SpriteSizeY;
                    Alignment = VerticalAlignment.Top;
                }
            }

            // Create Dialog Window BitmapSource
            DialogWindow = BitmapSource.Create(WindowWidth, WindowHeight, 96, 96, CharacterSheet.SpriteSheet.Format, CharacterSheet.SpriteSheet.Palette,
                TextPixelData, WindowWidth * BytesPerPixel);

            return DialogWindow;
        }
    }
}
