﻿using System;
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
        public readonly Spritesheet CharacterSheet;
        internal Color Color = Color.Black;
        internal float Scale = 1;
        internal float Offset = 0;

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
        }

        public BitmapSource RenderText(string Text, Dictionary<byte, string> CharacterMap, string[] KanjiMap0 = null, string[] KanjiMap1 = null,
            List<Color> Colors = null, int CharacterScale = 1)
        {
            // TODO: Line Size & Offset Parsing
            Spritesheet KanjiSheet0 = null;
            Spritesheet KanjiSheet1 = null;

            if (KanjiMap0 != null && KanjiMap1 != null)
            {
                KanjiSheet0 = new Spritesheet(TextRenderUtility.Convert(Properties.Resources.AFe__Kanji_Bank_0), 24, 32, 14, 32);
                KanjiSheet1 = new Spritesheet(TextRenderUtility.Convert(Properties.Resources.AFe__Kanji_Bank_1), 24, 32, 14, 32);
            }

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
            /*Regex CommandRegex = new Regex(@"<[^>]+>");
            string CommandStrippedText = CommandRegex.Replace(Text, "").TrimEnd();*/
            string CommandStrippedText = Text.TrimEnd();

            // Begin generating the entire window BitmapSource
            BitmapSource DialogWindow = null;
            int WindowWidth = 0, WindowHeight = CharacterSheet.SpriteHeight;

            // Calculate BitmapSource Dimensions
            int CurrentWidth = 0;
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
                        /*if (Command.Contains("<Line Color Index"))
                        {
                            var Match = Regex.Match(Command, @"\d+");
                            if (Match.Success && byte.TryParse(Match.Value, out byte ColorIndex) && Colors.Count > ColorIndex)
                            {
                                Color = Colors[ColorIndex];
                            }
                        }*/

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

            WindowWidth *= CharacterScale;
            WindowHeight *= CharacterScale;

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

                        // Set ParseIndex to the end of the command so we skip the rest of it.
                        ParseIndex = CommandEndingOffset;
                        continue;
                    }
                }

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
                            CharacterSprite = KanjiSheet0.GetSprite(Array.IndexOf(KanjiMap0, Character.ToString()), Color);
                        }
                        else if (KanjiMap1 != null && KanjiMap1.Contains(Character.ToString()))
                        {
                            CharacterSprite = KanjiSheet1.GetSprite(Array.IndexOf(KanjiMap1, Character.ToString()), Color);
                        }
                    }
                    else
                    {
                        CharacterSprite = CharacterSheet.GetSprite(KeyValPair.Key, Color);
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

            // Create Dialog Window BitmapSource
            DialogWindow = BitmapSource.Create(WindowWidth, WindowHeight, 96, 96, CharacterSheet.SpriteSheet.Format, CharacterSheet.SpriteSheet.Palette,
                TextPixelData, WindowWidth * BytesPerPixel);

            return DialogWindow;
        }
    }
}
