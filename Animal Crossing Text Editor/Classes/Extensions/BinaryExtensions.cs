using System;
using System.IO;
using System.Linq;

namespace Animal_Crossing_Text_Editor
{
    public static class BinaryReaderExtension
    {
        public static ushort ReadReversedUInt16(this BinaryReader Reader)
        {
            return Reader.ReadUInt16().Reverse();
        }

        public static short ReadReversedInt16(this BinaryReader Reader)
        {
            return Reader.ReadInt16().Reverse();
        }

        public static uint ReadReversedUInt32(this BinaryReader Reader)
        {
            return Reader.ReadUInt32().Reverse();
        }

        public static int ReadReversedInt32(this BinaryReader Reader)
        {
            return Reader.ReadInt32().Reverse();
        }
    }

    public static class BinaryWriterExtension
    {
        public static void WriteReversed(this BinaryWriter Writer, ushort Value)
        {
            Writer.Write(Value.Reverse());
        }

        public static void WriteReversed(this BinaryWriter Writer, short Value)
        {
            Writer.Write(Value.Reverse());
        }

        public static void WriteReversed(this BinaryWriter Writer, uint Value)
        {
            Writer.Write(Value.Reverse());
        }

        public static void WriteReversed(this BinaryWriter Writer, int Value)
        {
            Writer.Write(Value.Reverse());
        }
    }
}
