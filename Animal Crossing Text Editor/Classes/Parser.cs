using System;
using Animal_Crossing_Text_Editor.Classes.Parser.Wii;

namespace Animal_Crossing_Text_Editor
{
    public static class Parser
    {
        public static IParser GetParser(CharacterSet gameType)
        {
            switch (gameType)
            {
                case CharacterSet.DoubutsuNoMori:    
                case CharacterSet.DoubutsuNoMoriPlus:
                case CharacterSet.AnimalCrossing:
                case CharacterSet.DoubutsuNoMoriEPlus:
                    return new GCNParser();

                case CharacterSet.DongwuSenlin:
                    throw new NotImplementedException();

                case CharacterSet.WildWorld:
                    return new NDSParser();

                case CharacterSet.CityFolk:
                    return new WiiParser();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
