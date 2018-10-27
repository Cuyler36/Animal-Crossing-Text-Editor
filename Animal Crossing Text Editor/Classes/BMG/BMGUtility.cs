using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animal_Crossing_Text_Editor.Compression;

namespace Animal_Crossing_Text_Editor
{
    public static class BMGUtility
    {
        public static bool IsBuffBMG(byte[] Buffer)
        {
            return Encoding.ASCII.GetString(Buffer, 0, 8) == "MESGbmg1" ||
                   LZ77.IsLz77Compressed(Buffer) &&
                   MainWindow.SelectedCharacterSet == CharacterSet.WildWorld;
        }

        public static bool Write(BMG File, string Path)
        {
            try
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

                    // Write DAT data
                    for (int i = 0; i < File.INF_Section.Items.Length; i++)
                    {
                        Writer.Write(File.INF_Section.Items[i].Data);
                    }

                    Writer.Flush();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<BMG> Decode(string path, List<System.Drawing.Color> colors, Delegate reportProgressFunc = null)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                Stream stream = fileStream;

                // TODO: Remove the MainWindow static variable reference.
                if (MainWindow.SelectedCharacterSet == CharacterSet.WildWorld && LZ77.IsLz77Compressed(fileStream))
                {
                    Debug.WriteLine($"In file size: {fileStream.Length:X}");
                    stream = new LZ77().Decompress(fileStream);
                    stream.Seek(0, SeekOrigin.Begin);

                    // TEST
                    var recompress = new LZ77().Compress(stream);
                    Debug.WriteLine($"Recompressed file size: {recompress.Length:X}");
                    stream.Seek(0, SeekOrigin.Begin);

                    // Test write
                    /*using (var fStream = new FileStream(Path.Combine(Path.GetDirectoryName(path), "decompressTest.bmg"),
                        FileMode.Create))
                    {
                        var data = ((MemoryStream)recompress).ToArray();
                        fStream.Write(data, 0, data.Length);
                        recompress.Close();
                    }*/

                }

                using (var reader = new BinaryReader(stream))
                {
#if DEBUG
                    var watch = Stopwatch.StartNew();
                    watch.Start();
#endif
                    var bmg = new BMG
                    {
                        FileType = new string(reader.ReadChars(8)),
                        Size = reader.ReadUInt32().Reverse()
                    };

                    // Use size to determine endianness
                    if (bmg.Size != 0 && bmg.Size > reader.BaseStream.Length)
                    {
                        bmg.IsLittleEndian = true;
                    }

                    // Confirm Size isn't 0
                    if (bmg.Size == 0)
                    {
                        //Console.WriteLine("BMG Size was zero. Setting it to the filesize.");
                        //bmg.Size = (ulong)Reader.BaseStream.Length;
                    }

                    bmg.SectionCount = reader.ReadUInt32();
                    bmg.Encoding = reader.ReadUInt32();

                    if (!bmg.IsLittleEndian)
                    {
                        bmg.SectionCount = bmg.SectionCount.Reverse();
                        bmg.Encoding = bmg.Encoding.Reverse();
                    }

                    // Debug Lines
                    Console.WriteLine("BMG Section Count: " + bmg.SectionCount);
                    Console.WriteLine("UTF16: " + (bmg.Encoding == 2).ToString());

                    // Create our Text Info Section (INF)
                    reader.BaseStream.Position = 0x20;
                    bmg.INF_Section.SectionType = new string(reader.ReadChars(4));
                    bmg.INF_Section.Size = reader.ReadUInt32();
                    bmg.INF_Section.MessageCount = reader.ReadUInt16();
                    bmg.INF_Section.INF_Size = reader.ReadUInt16();
                    bmg.INF_Section.Unknown = reader.ReadUInt32();

                    if (!bmg.IsLittleEndian)
                    {
                        bmg.INF_Section.Size = bmg.INF_Section.Size.Reverse();
                        bmg.INF_Section.MessageCount = bmg.INF_Section.MessageCount.Reverse();
                        bmg.INF_Section.INF_Size = bmg.INF_Section.INF_Size.Reverse();
                        bmg.INF_Section.Unknown = bmg.INF_Section.Unknown.Reverse();
                    }

                    bmg.INF_Section.Items = new BMG_INF_Item[bmg.INF_Section.MessageCount];

                    //Debug Lines
                    Console.WriteLine("INF Size: 0x" + bmg.INF_Section.Size.ToString("X"));
                    Console.WriteLine("INF Message Count: " + bmg.INF_Section.MessageCount);
                    Console.WriteLine("INF CharSize: 0x" + bmg.INF_Section.INF_Size.ToString("X"));

                    // Load our Text Info Items
                    reader.BaseStream.Position = 0x30;
                    for (var i = 0; i < bmg.INF_Section.MessageCount; i++)
                    {
                        var item = new BMG_INF_Item
                        {
                            Text_Offset = bmg.IsLittleEndian ? reader.ReadUInt32() : reader.ReadUInt32().Reverse()
                        };

                        bmg.INF_Section.Items[i] = item;

                        if (bmg.INF_Section.INF_Size > 4)
                        {
                            // TODO: This is a hack. We should figure out what the additional size means
                            reader.BaseStream.Seek(bmg.INF_Section.INF_Size - 4, SeekOrigin.Current);
                        }
                    }

                    // Create our Text Data Section (DAT)
                    reader.BaseStream.Position =
                        bmg.Size == 0 ? bmg.INF_Section.Size : bmg.INF_Section.Size + 0x20; // + 0x20 for the bgm header

                    if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "DAT1")
                    {
                        var dat1Found = false;
                        while (!dat1Found)
                        {
                            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "DAT1") continue;

                            reader.BaseStream.Position -= 4;
                            Debug.WriteLine("Found DAT1: 0x" + reader.BaseStream.Position.ToString("X"));
                            dat1Found = true;
                        }
                    }
                    else
                    {
                        reader.BaseStream.Position -= 4;
                    }

                    bmg.DAT_Section.Offset = (int) reader.BaseStream.Position;
                    bmg.DAT_Section.SectionType = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    bmg.DAT_Section.Size = reader.ReadUInt32();

                    if (!bmg.IsLittleEndian)
                    {
                        bmg.DAT_Section.Size = bmg.DAT_Section.Size.Reverse();
                    }

                    bmg.DAT_Section.Strings = new string[bmg.INF_Section.MessageCount];

                    long stringStartOffset = bmg.DAT_Section.Offset + 0x8;

                    await Task.Run(() =>
                    {
                        var parser =
                            Parser.GetParser(MainWindow
                                .SelectedCharacterSet); // TODO: Move this static reference out of here.

                        for (var i = 0; i < bmg.INF_Section.MessageCount; i++)
                        {
                            reader.BaseStream.Position = stringStartOffset + bmg.INF_Section.Items[i].Text_Offset;

                            long endingOffset;
                            if (i == bmg.INF_Section.MessageCount - 1)
                            {
                                endingOffset = reader.BaseStream.Length;
                            }
                            else
                            {
                                endingOffset = stringStartOffset + bmg.INF_Section.Items[i + 1].Text_Offset;
                            }

                            var startingOffset = reader.BaseStream.Position;

                            // TODO: Wild World has a case where if the next INF entry is 0, the message id? or something is set to the next value after that
                            // This means that each entry is 0xC in size max.

                            var readSize = (int) (endingOffset - reader.BaseStream.Position);
                            if (readSize < 0)
                                Console.WriteLine($"Read size is less than 0 for entry {i:X4}");

                            bmg.INF_Section.Items[i].Data = reader.ReadBytes(readSize);
                            bmg.INF_Section.Items[i].Text =
                                parser.Decode(bmg.INF_Section.Items[i]
                                    .Data); //TextUtility.Decode(bmg.INF_Section.Items[i].Data, colors);
                            bmg.INF_Section.Items[i].Length = (uint) (endingOffset - startingOffset);

                            if (reportProgressFunc != null && i % 50 == 0)
                            {
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                    reportProgressFunc.DynamicInvoke(i, bmg.INF_Section.MessageCount)));
                            }
                        }

                        return bmg;
                    });

                    if (reportProgressFunc != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                            reportProgressFunc.DynamicInvoke(bmg.INF_Section.MessageCount,
                                bmg.INF_Section.MessageCount)));
                    }

#if DEBUG
                    watch.Stop();
                    Debug.WriteLine($"Decode time elapsed: {watch.ElapsedMilliseconds} ms");
#endif

                    return bmg;
                }
            }
        }

        public static bool UpdateEntry(BMG bmg, int entryIndex, byte[] data, string text)
        {
            try
            {
                var entry = bmg.INF_Section.Items[entryIndex];
                var sizeDelta = data.Length - (int) entry.Length;
                if (sizeDelta != 0)
                {
                    for (var i = entryIndex + 1; i < bmg.INF_Section.Items.Length; i++)
                    {
                        bmg.INF_Section.Items[i].Text_Offset =
                            (uint) (bmg.INF_Section.Items[i].Text_Offset + sizeDelta);
                    }
                }

                entry.Text = text;
                entry.Length = (uint)data.Length;
                entry.Data = data;

                bmg.INF_Section.Items[entryIndex] = entry;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async void SaveStrings(string FilePath)
        {
            BMG bmg = await Decode(FilePath, null);

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
