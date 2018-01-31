using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Animal_Crossing_Text_Editor
{
    public struct BMG
    {
        public string FileType;
        public uint Size;
        public uint SectionCount;
        public uint Encoding;
        public byte[] Padding;
        public BMG_Section_INF INF_Section;
        public BMG_Section_DAT DAT_Section;
    };

    public struct BMG_Section_INF
    {
        public string SectionType;
        public uint Size;
        public ushort MessageCount;
        public ushort INF_Size;
        public uint Unknown;
        public BMG_INF_Item[] Items;
    };

    public struct BMG_INF_Item
    {
        public uint Text_Offset;
        public string Text;
        public uint Length;
        public byte[] Data;
    };

    public struct BMG_Section_DAT
    {
        public string SectionType;
        public int Offset;
        public uint Size;
        public string[] Strings; // Will probably go unused
    };

    public struct BMG_Section_MID
    {
        public string SectionType;
        public uint Size;
        public ushort MessageCount;
        public ushort Unknown_1; // Usually 0x1000?
        public ushort Unknown_2; // Usually 0?
        public uint[] Message_IDs;
    };

    public static class BMGUtility
    {
        public static bool IsBuffBMG(byte[] Buffer)
        {
            return Encoding.ASCII.GetString(Buffer.Take(8).ToArray()) == "MESGbmg1";
        }

        public static void Write(BMG File, string Path)
        {
            if (System.IO.File.Exists(Path))
            {
                System.IO.File.Delete(Path);
            }

            using (BinaryWriter Writer = new BinaryWriter(new FileStream(Path, FileMode.OpenOrCreate)))
            {
                // Write BMG Header
                Writer.Write(Encoding.ASCII.GetBytes("MESGbmg1")); // File Identifier
                Writer.Write(BitConverter.GetBytes(File.Size).Reverse().ToArray());
                Writer.Write(BitConverter.GetBytes(File.SectionCount).Reverse().ToArray());
                Writer.Write(BitConverter.GetBytes(File.Encoding).Reverse().ToArray());
                Writer.Write(new byte[0xC]); // Padding

                // Write INF Header
                Writer.Write(Encoding.ASCII.GetBytes("INF1"));
                Writer.Write(BitConverter.GetBytes(File.INF_Section.Size).Reverse().ToArray());
                Writer.Write(BitConverter.GetBytes(File.INF_Section.MessageCount).Reverse().ToArray());
                Writer.Write(BitConverter.GetBytes(File.INF_Section.INF_Size).Reverse().ToArray());
                Writer.Write(BitConverter.GetBytes(File.INF_Section.Unknown).Reverse().ToArray());

                // Write INF data
                for (int i = 0; i < File.INF_Section.Items.Length; i++)
                {
                    Writer.Write(BitConverter.GetBytes(File.INF_Section.Items[i].Text_Offset).Reverse().ToArray());
                }

                // Add padding as needed
                Writer.Write(new byte[File.DAT_Section.Offset - Writer.BaseStream.Position]);

                // Write DAT Header
                Writer.Write(Encoding.ASCII.GetBytes("DAT1"));
                Writer.Write(BitConverter.GetBytes(File.DAT_Section.Size).Reverse().ToArray());

                // Write padding to data
                Writer.Write(new byte[File.INF_Section.Items[0].Text_Offset]);

                Dictionary<byte, string> Character_Map = MainWindow.Character_Set_Type == File_Type.Animal_Crossing ? TextUtility.Animal_Crossing_Character_Map
                    : TextUtility.Doubutsu_no_Mori_Plus_Character_Map;

                // Write DAT data
                for (int i = 0; i < File.INF_Section.Items.Length; i++)
                {
                    if (!string.IsNullOrEmpty(File.INF_Section.Items[i].Text)) // Skip writing duplicate entries
                    {
                        List<byte> Data = new List<byte>();
                        foreach (string Char in File.INF_Section.Items[i].Text.TextElements())
                        {
                            if (Character_Map.ContainsValue(Char))
                            {
                                Data.Add(Character_Map.First(o => o.Value == Char).Key);
                            }
                            else
                            {
                                Data.Add(Encoding.ASCII.GetBytes(Char)[0]);
                            }
                        }
                        byte[] String_Data = Data.ToArray();
                        Writer.Write(String_Data);
                    }
                    //if (i == File.INF_Section.Items.Length - 1)
                    //System.Windows.Forms.MessageBox.Show("Last Entry: " + File.INF_Section.Items[i].Text + " Position: 0x" + Writer.BaseStream.Position.ToString("X"));
                }

                Writer.Flush();
            }
        }

        public static BMG Decode(string Path)
        {
            using (BinaryReader Reader = new BinaryReader(new FileStream(Path, FileMode.Open)))
            {
                // Test Stuff
                // Create our BMG File
                BMG bmg = new BMG();
                bmg.FileType = new string(Reader.ReadChars(8));
                bmg.Size = BitConverter.ToUInt32(Reader.ReadBytes(4).Reverse().ToArray(), 0);
                // Confirm Size isn't 0
                if (bmg.Size == 0)
                {
                    //Console.WriteLine("BMG Size was zero. Setting it to the filesize.");
                    //bmg.Size = (ulong)Reader.BaseStream.Length;
                }
                bmg.SectionCount = BitConverter.ToUInt32(Reader.ReadBytes(4).Reverse().ToArray(), 0);
                bmg.Encoding = BitConverter.ToUInt32(Reader.ReadBytes(4).Reverse().ToArray(), 0);

                // Debug Lines
                Console.WriteLine("BMG Section Count: " + bmg.SectionCount);
                Console.WriteLine("UTF16: " + (bmg.Encoding == 2).ToString());

                // Create our Text Info Section (INF)
                Reader.BaseStream.Position = 0x20;
                bmg.INF_Section.SectionType = new string(Reader.ReadChars(4));
                bmg.INF_Section.Size = BitConverter.ToUInt32(Reader.ReadBytes(4).Reverse().ToArray(), 0);
                bmg.INF_Section.MessageCount = BitConverter.ToUInt16(Reader.ReadBytes(2).Reverse().ToArray(), 0);
                bmg.INF_Section.INF_Size = BitConverter.ToUInt16(Reader.ReadBytes(2).Reverse().ToArray(), 0);
                bmg.INF_Section.Unknown = BitConverter.ToUInt32(Reader.ReadBytes(4).Reverse().ToArray(), 0);
                bmg.INF_Section.Items = new BMG_INF_Item[bmg.INF_Section.MessageCount];

                //Debug Lines
                Console.WriteLine("INF Size: 0x" + bmg.INF_Section.Size.ToString("X"));
                Console.WriteLine("INF Message Count: " + bmg.INF_Section.MessageCount);
                Console.WriteLine("INF CharSize: 0x" + bmg.INF_Section.INF_Size.ToString("X"));

                // Load our Text Info Items
                Reader.BaseStream.Position = 0x30;
                for (int i = 0; i < bmg.INF_Section.MessageCount; i++)
                {
                    BMG_INF_Item Item = new BMG_INF_Item();
                    Item.Text_Offset = BitConverter.ToUInt32(Reader.ReadBytes(4).Reverse().ToArray(), 0);
                    //Console.WriteLine("Offset: 0x" + Item.Text_Offset.ToString("X"));
                    //Console.ReadLine();
                    bmg.INF_Section.Items[i] = Item;
                }

                // Create our Text Data Section (DAT)
                Reader.BaseStream.Position = bmg.Size == 0 ? bmg.INF_Section.Size : bmg.INF_Section.Size + 0x20; // + 0x20 for the bgm header

                bool DAT1_Found = true;
                if (Encoding.ASCII.GetString(Reader.ReadBytes(4)) != "DAT1")
                {
                    DAT1_Found = false;
                    while (!DAT1_Found)
                    {
                        //Reader.BaseStream.Position = bmg.INF_Section.Size + 0x20;
                        //System.Windows.MessageBox.Show("Can't find DAT1! Trying offset: 0x" + Reader.BaseStream.Position.ToString("X"));
                        if (Encoding.ASCII.GetString(Reader.ReadBytes(4)) == "DAT1")
                        {
                            Reader.BaseStream.Position -= 4;
                            Debug.WriteLine("Found DAT1: 0x" + Reader.BaseStream.Position.ToString("X"));
                            DAT1_Found = true;
                        }
                    }
                }
                else
                    Reader.BaseStream.Position -= 4;
                //Console.WriteLine((bmg.INF_Section.Size).ToString("X"));
                bmg.DAT_Section.Offset = (int)Reader.BaseStream.Position;
                bmg.DAT_Section.SectionType = Encoding.ASCII.GetString(Reader.ReadBytes(4));
                bmg.DAT_Section.Size = BitConverter.ToUInt32(Reader.ReadBytes(4).Reverse().ToArray(), 0);
                bmg.DAT_Section.Strings = new string[bmg.INF_Section.MessageCount];

                // Debug Lines
                //Console.WriteLine(bmg.DAT_Section.SectionType);
                //Console.ReadLine();

                // Load our strings
                long String_Start_Offset = bmg.DAT_Section.Offset + 0x8;

                //Console.WriteLine("Start Offset: 0x" + String_Start_Offset.ToString("X"));
                //Console.ReadLine();

                for (int i = 0; i < bmg.INF_Section.MessageCount; i++)
                {
                    Reader.BaseStream.Position = String_Start_Offset + bmg.INF_Section.Items[i].Text_Offset;
                    //bmg.DAT_Section.Strings[i] = Reader.BaseStream.Position.ToString("X") + " | ";

                    /*Console.WriteLine("Offset: 0x" + bmg.INF_Section.Items[i].Text_Offset.ToString("X") +
                        string.Format(" | Reading String {0} / {1} from: 0x", i + 1, bmg.INF_Section.MessageCount)
                        + (String_Start_Offset + bmg.INF_Section.Items[i].Text_Offset).ToString("X"));*/
                    
                    long Ending_Offset = 0;
                    if (i == bmg.INF_Section.MessageCount - 1)
                    {
                        Ending_Offset = Reader.BaseStream.Length;
                        //System.Windows.Forms.MessageBox.Show(string.Format("Starting Offset: 0x{0} | Ending Offset: 0x{1}", Reader.BaseStream.Position.ToString("X"), Ending_Offset.ToString("X")));
                    }
                    else
                    {
                        Ending_Offset = String_Start_Offset + bmg.INF_Section.Items[i + 1].Text_Offset;
                    }
                    /*if (i + 1 < bmg.INF_Section.Items.Length)
                        Console.WriteLine(string.Format("Start Offset: {2} | Ending Offset: {0} | Length: {1}",
                            (Ending_Offset - 1).ToString("X"), (bmg.INF_Section.Items[i + 1].Text_Offset - bmg.INF_Section.Items[i].Text_Offset).ToString("X"),
                            Reader.BaseStream.Position.ToString("X")));
                    //Console.WriteLine(string.Format("Current Offset: {0} | Next Offset: {1}", bmg.INF_Section.Items[i].Text_Offset, bmg.INF_Section.Items[i + 1].Text_Offset));
                    //Console.ReadKey();*/
                    bmg.INF_Section.Items[i].Data = Reader.ReadBytes((int)(Ending_Offset - Reader.BaseStream.Position));
                    bmg.INF_Section.Items[i].Text = TextUtility.Decode(bmg.INF_Section.Items[i].Data);
                    bmg.INF_Section.Items[i].Length = (uint)Ending_Offset - (uint)Reader.BaseStream.Position;
                }

                return bmg;
            }
        }

        public static void SaveStrings(string FilePath)
        {
            BMG bmg = Decode(FilePath);

            using (StreamWriter Writer = File.CreateText(Path.GetDirectoryName(FilePath) + "/" + Path.GetFileNameWithoutExtension(FilePath) + "_Output.txt"))
            {
                foreach (string Message in bmg.DAT_Section.Strings)
                {
                    Writer.WriteLine(Message);
                }
                Writer.Flush();
                Writer.Close();
            }

        }
    }
}
