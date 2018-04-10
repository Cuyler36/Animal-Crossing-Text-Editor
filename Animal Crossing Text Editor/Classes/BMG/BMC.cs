using System;
using System.Linq;
using System.Text;

namespace Animal_Crossing_Text_Editor
{
    /// <summary>
    /// BMC Class
    /// The BMC Class contains an array of 8-bit RGBA Colors used by either BMG files or the games themselves.
    /// Format is R-G-B-A
    /// </summary>
    public class BMC
    {
        public string Identifier { get; private set; }
        public uint Length { get; set; }
        public uint Unknown { get; private set; }
        public byte[] Unknown2 { get; private set; }

        public CLTSection CLT_Section;

        public BMC()
        {
            Identifier = "MGCLbmc1";
            Length = 0;
            Unknown = 0;
            Unknown2 = new byte[0x10];
        }

        public BMC(byte[] Buffer)
        {
            if (Buffer.Length >= 0x20)
            {
                Identifier = Encoding.ASCII.GetString(Buffer, 0, 8);
                Length = BitConverter.ToUInt32(Buffer, 8).Reverse();
                Unknown = BitConverter.ToUInt32(Buffer, 0xC).Reverse();
                Unknown2 = Buffer.Skip(0x10).Take(0x10).ToArray();

                CLT_Section = new CLTSection(Buffer.Skip(0x20).ToArray());
            }
        }
    }

    public class BMGSection
    {
        public string Identifier { get; protected set; }
        public uint Length { get; set; }

        public void SetLength(uint length)
        {
            Length = length;
        }
    }

    public class CLTSection : BMGSection
    {
        public ushort Entries { get; set; }
        public ushort Unknown { get; set; }
        public uint[] Items;

        public CLTSection()
        {
            Identifier = "CLT1";
            Length = 0;
            Entries = 0;
            Unknown = 0;
        }

        public CLTSection(byte[] Buffer)
        {
            if (Buffer.Length > 0x0C)
            {
                Identifier = Encoding.ASCII.GetString(Buffer, 0, 4);
                Length = BitConverter.ToUInt32(Buffer, 4).Reverse();
                Entries = BitConverter.ToUInt16(Buffer, 8).Reverse();
                Unknown = BitConverter.ToUInt16(Buffer, 0xA).Reverse();

                Items = new uint[Entries];

                for (int i = 0; i < Entries; i++)
                {
                    Items[i] = BitConverter.ToUInt32(Buffer, 0x0C + i * 4).Reverse();
                    Items[i] = ((Items[i] & 0xFF) << 24) | ((Items[i] >> 8) & 0xFFFFFF); // Convert from RGBA to ARGB
                }
            }
        }
    }
}
