using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Animal_Crossing_Text_Editor.Compression
{
    public sealed class LZ77
    {
        private const int N = 4096;
        private const int F = 18;
        private const int threshold = 2;
        private int[] leftSon = new int[N + 1];
        private int[] rightSon = new int[N + 257];
        private int[] dad = new int[N + 1];
        private ushort[] textBuffer = new ushort[N + 17];
        private int matchPosition = 0, matchLength = 0;

        /// <summary>
        /// Lz77 Magic.
        /// </summary>
        public const uint Lz77Magic = 0x4c5a3737;

        #region Public Functions
        /// <summary>
        /// Checks whether a file is Lz77 compressed or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLz77Compressed(string file) => IsLz77Compressed(File.ReadAllBytes(file));

        /// <summary>
        /// Checks whether a file is Lz77 compressed or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLz77Compressed(byte[] file) => BitConverter.ToUInt32(file, 0).Reverse() == Lz77Magic;

        /// <summary>
        /// Checks whether a file is Lz77 compressed or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLz77Compressed(Stream file)
        {
            file.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[4];
            file.Read(buffer, 0, 4);
            return IsLz77Compressed(buffer);
        }



        /// <summary>
        /// Compresses a file using the Lz77 algorithm.
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        public void Compress(string inFile, string outFile)
        {
            Stream compressedFile;

            using (FileStream fsIn = new FileStream(inFile, FileMode.Open))
                compressedFile = compress(fsIn);

            byte[] output = new byte[compressedFile.Length];
            compressedFile.Read(output, 0, output.Length);

            if (File.Exists(outFile)) File.Delete(outFile);

            using (FileStream fs = new FileStream(outFile, FileMode.Create))
                fs.Write(output, 0, output.Length);
        }

        /// <summary>
        /// Compresses the byte array using the Lz77 algorithm.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] Compress(byte[] file)
        {
            return ((MemoryStream)compress(new MemoryStream(file))).ToArray();
        }

        /// <summary>
        /// Compresses the stream using the Lz77 algorithm.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Stream Compress(Stream file)
        {
            return compress(file);
        }

        /// <summary>
        /// Decompresses a file using the Lz77 algorithm.
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        public void Decompress(string inFile, string outFile)
        {
            Stream compressedFile;

            using (FileStream fsIn = new FileStream(inFile, FileMode.Open))
                compressedFile = decompress(fsIn);

            byte[] output = new byte[compressedFile.Length];
            compressedFile.Read(output, 0, output.Length);

            if (File.Exists(outFile)) File.Delete(outFile);

            using (FileStream fs = new FileStream(outFile, FileMode.Create))
                fs.Write(output, 0, output.Length);
        }

        /// <summary>
        /// Decompresses the byte array using the Lz77 algorithm.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] Decompress(byte[] file)
        {
            return ((MemoryStream)decompress(new MemoryStream(file))).ToArray();
        }

        public Stream Decompress(Stream file)
        {
            return decompress(file);
        }
        #endregion

        #region Private Functions
        private Stream decompress(Stream inFile)
        {
            inFile.Seek(0, SeekOrigin.Begin);

            uint flags = 8;
            uint decompressedSize;
            uint bufferSize;
            uint bitmap = 8;

            using (var reader = new BinaryReader(inFile))
            {
                inFile.Seek(0, SeekOrigin.Begin);

                if (reader.ReadUInt32().Reverse() != Lz77Magic)
                {
                    inFile.Dispose();
                    throw new Exception("Invaild Magic!");
                }

                var infoByte = reader.ReadByte();
                var lz77SegmentEnds = new List<ushort>();
                var headerOffset = 4; // Start at four to include the "LZ77" magic.

                if (infoByte == 0xF7)
                {
                    Debug.WriteLine("We have a segmented LZ77 compressed file!");

                    // Decompressed file size
                    decompressedSize = reader.ReadUInt16(); // Seems to be the case at least?
                    flags = reader.ReadByte(); // Is this right?
                    bitmap = 0;

                    // Now we read in unsigned shorts until we hit one that is equal to the length of the stream - current position?
                    // TODO: There may be a better way to do this!
                    var currentSegmentEnd = -1;

                    do
                    {
                        currentSegmentEnd = reader.ReadUInt16();
                        lz77SegmentEnds.Add((ushort)currentSegmentEnd);

                        // Check if the current segment end + current read pointer is equal to the length of the file.
                    } while (currentSegmentEnd + inFile.Position != inFile.Length);

                    headerOffset += (int)inFile.Position;
                    Debug.WriteLine($"End of LZ77 segments found! Offset: {currentSegmentEnd:X4}");

                    // Read and verify our compression type. The next byte should be our compression type.
                    if (reader.ReadByte() != 0x10)
                    {
                        inFile.Dispose();
                        throw new Exception("Unsupported Compression Type!");
                    }

                    bufferSize = reader.ReadUInt32(); // Size of the buffer to hold data in? We don't really care about this.
                }
                else if (infoByte == 0x10)
                {
                    Debug.WriteLine("We have only one LZ77 compressed segment.");
                    lz77SegmentEnds.Add((ushort)(inFile.Length - 4));
                    bufferSize = decompressedSize = reader.ReadUInt32() & 0xFFFFFF;
                    inFile.Seek(-1, SeekOrigin.Current);
                }
                else
                {
                    inFile.Dispose();
                    throw new ArgumentException($"Invalid info byte! Expected 0x10 or 0xF7 but got {infoByte:X2}!");
                }

                // Add the header size/offset to each segment end to get their absolute positions
                for (var x = 0; x < lz77SegmentEnds.Count; x++)
                {
                    lz77SegmentEnds[x] += (ushort)headerOffset;
                }

                var outFile = new MemoryStream();

                for (var x = 0; x < lz77SegmentEnds.Count; x++)
                {
                    // Seek to the beginning of the segment.
                    if (x > 0)
                    {
                        inFile.Seek(lz77SegmentEnds[x - 1], SeekOrigin.Begin);
                    }

                    Debug.WriteLine($"Begin decompression of LZ77 segment {x} at file offset: {inFile.Position:X}");

                    // Initialize decompression resources & start decompressing the segment.
                    uint currentSize = 0;
                    var endOffset = lz77SegmentEnds[x];

                    int i;
                    for (i = 0; i < N - F; i++)
                    {
                        textBuffer[i] = 0xdf;
                    }

                    var r = N - F;

                    if (x > 0)
                    {
                        flags = 8;
                        bitmap = 8;
                    }

                    do
                    {
                        int c;
                        if (bitmap == 8)
                        {
                            if ((c = inFile.ReadByte()) == -1)
                            {
                                break;
                            }

                            flags = (uint)c;
                            bitmap = 0;
                        }

                        if ((flags & 0x80) == 0)
                        {
                            if ((c = inFile.ReadByte()) == inFile.Length - 1)
                            {
                                break;
                            }

                            if (inFile.Position < endOffset && currentSize < bufferSize && currentSize < decompressedSize)
                            {
                                if (outFile.Position == 0x1000)
                                {
                                    Debug.WriteLine($"Writing {c:X2} to file offset {outFile.Position:X} which was read from {(inFile.Position - 1):X}");
                                }
                                outFile.WriteByte((byte)c);
                            }
                            else
                            {
                                break;
                            }

                            textBuffer[r++] = (byte)c;
                            r &= (N - 1);
                            currentSize++;
                        }
                        else
                        {
                            if ((i = inFile.ReadByte()) == -1)
                            {
                                break;
                            }

                            int j;
                            if ((j = inFile.ReadByte()) == -1)
                            {
                                break;
                            }

                            j = j | ((i << 8) & 0xf00);
                            i = ((i >> 4) & 0x0f) + threshold;

                            for (var k = 0; k <= i; k++)
                            {
                                c = textBuffer[(r - j - 1) & (N - 1)];
                                if (inFile.Position < endOffset && currentSize < bufferSize && currentSize < decompressedSize)
                                {
                                    if (outFile.Position == 0x1000)
                                    {
                                        Debug.WriteLine($"Writing {c:X2} to file offset {outFile.Position:X} which was read from textBuffer[{((r - j - 1) & (N - 1)):X}]");
                                    }
                                    outFile.WriteByte((byte)c);
                                }
                                else
                                {
                                    break;
                                }

                                textBuffer[r++] = (byte)c;
                                r &= (N - 1);
                                currentSize++;
                            }
                        }

                        flags <<= 1;
                        bitmap++;
                    } while (true);
                }

                // Verify the size is correct. This will clamp the size.
                outFile.SetLength(decompressedSize);
                return outFile;
            }
        }

        private Stream compress(Stream inFile)
        {
            if (IsLz77Compressed(inFile)) return inFile;
            inFile.Seek(0, SeekOrigin.Begin);

            var segments = inFile.Length / 0x1000;
            if ((inFile.Length & 0x0FFF) != 0)
            {
                segments++;
            }

            var textSize = 0;
            var codeSize = 0;

            int i, c, r, s, length, lastMatchLength, codeBufferPointer, mask;
            var codeBuffer = new int[17];

            if (segments > 1 && inFile.Length > 0xFFFF)
            {
                throw new ArgumentOutOfRangeException(
                    "The length of the stream was greater than the maximum allowed value of 0xFFFF!\n"
                    + $"Size received: {inFile.Length:X}");
            }

            var fileSize = segments == 1 ? (Convert.ToUInt32(inFile.Length) << 8) | 0x10 : (0x1000 << 8) | 0x10;
            var outFile = new MemoryStream();
            var compressedData = new MemoryStream();
            var compressionBuffer = new MemoryStream();

            using (var writer = new BinaryWriter(outFile, Encoding.ASCII, true))
            {

                writer.WriteReversed(Lz77Magic);
                if (segments > 1)
                {
                    writer.Write((byte) 0xF7);
                    writer.Write((ushort) inFile.Length);
                    writer.Write((byte) 0);
                }
                else
                {
                    writer.Write(fileSize);
                }

                for (var segment = 0; segment < segments; segment++)
                {
                    var bytesCompressed = 0;

                    InitTree();
                    codeBuffer[0] = 0;
                    codeBufferPointer = 1;
                    mask = 0x80;
                    s = 0;
                    r = N - F;

                    for (i = s; i < r; i++) textBuffer[i] = 0xffff;

                    for (length = 0; length < F && (c = (int) inFile.ReadByte()) != -1; length++, bytesCompressed++)
                        textBuffer[r + length] = (ushort) c;

                    if ((textSize = length) == 0) return inFile;

                    for (i = 1; i <= F; i++) InsertNode(r - i);
                    InsertNode(r);

                    do
                    {
                        if (matchLength > length) matchLength = length;

                        if (matchLength <= threshold)
                        {
                            matchLength = 1;
                            codeBuffer[codeBufferPointer++] = textBuffer[r];
                        }
                        else
                        {
                            codeBuffer[0] |= mask;

                            codeBuffer[codeBufferPointer++] = (char)
                                                              (((r - matchPosition - 1) >> 8) & 0x0f) |
                                                              ((matchLength - (threshold + 1)) << 4);

                            codeBuffer[codeBufferPointer++] = (char) ((r - matchPosition - 1) & 0xff);
                        }

                        if ((mask >>= 1) == 0)
                        {
                            for (i = 0; i < codeBufferPointer; i++)
                            {
                                compressionBuffer.WriteByte((byte) codeBuffer[i]);
                            }

                            codeSize += codeBufferPointer;
                            codeBuffer[0] = 0;
                            codeBufferPointer = 1;
                            mask = 0x80;
                        }

                        lastMatchLength = matchLength;
                        for (i = 0; i < lastMatchLength && (c = (int) inFile.ReadByte()) != -1; i++, bytesCompressed++)
                        {
                            if (bytesCompressed >= fileSize)
                            {
                                Debug.WriteLine("Max compression size reached in node loop!");
                            }

                            DeleteNode(s);

                            textBuffer[s] = (ushort) c;
                            if (s < F - 1) textBuffer[s + N] = (ushort) c;
                            s = (s + 1) & (N - 1);
                            r = (r + 1) & (N - 1);

                            InsertNode(r);
                        }

                        while (i++ < lastMatchLength)
                        {
                            DeleteNode(s);

                            s = (s + 1) & (N - 1);
                            r = (r + 1) & (N - 1);
                            if (--length != 0) InsertNode(r);
                        }
                    } while (length > 0 && bytesCompressed < (fileSize >> 8));

                    Debug.WriteLine($"Bytes compressed for this segment: {bytesCompressed:X}");

                    if (codeBufferPointer > 1)
                    {
                        for (i = 0; i < codeBufferPointer; i++) compressionBuffer.WriteByte((byte) codeBuffer[i]);
                        codeSize += codeBufferPointer;
                    }

                    if (codeSize % 4 != 0)
                        for (i = 0; i < 4 - (codeSize % 4); i++)
                            compressionBuffer.WriteByte(0x00);

                    Debug.WriteLine($"CompressionBuffer size: {compressionBuffer.Length:X}");
                    var data = compressionBuffer.ToArray();
                    compressedData.Write(data, 0, data.Length);

                    if (compressionBuffer.Length > 0xFFFF)
                    {
                        Debug.WriteLine($"CompressionBuffer had a size greater than 0xFFFF! Size: {compressionBuffer.Length:X}");
                    }

                    writer.Write((ushort) (compressedData.Length + 4)); // We add four to compensate for not adding the buffer size | compression type to the stream.
                    compressionBuffer.Close();
                    compressionBuffer = new MemoryStream();
                }

                if (segments > 1)
                {
                    writer.Write(fileSize);
                }

                writer.Write(compressedData.ToArray());
                compressedData.Close();
            }


            return outFile;
        }

        private void InitTree()
        {
            int i;
            for (i = N + 1; i <= N + 256; i++) rightSon[i] = N;
            for (i = 0; i < N; i++) dad[i] = N;
        }

        private void InsertNode(int r)
        {
            int i, p, cmp;
            cmp = 1;
            p = N + 1 + (textBuffer[r] == 0xffff ? 0 : textBuffer[r]);
            rightSon[r] = leftSon[r] = N; matchLength = 0;

            for (; ; )
            {
                if (cmp >= 0)
                {
                    if (rightSon[p] != N) p = rightSon[p];
                    else { rightSon[p] = r; dad[r] = p; return; }
                }
                else
                {
                    if (leftSon[p] != N) p = leftSon[p];
                    else { leftSon[p] = r; dad[r] = p; return; }
                }

                for (i = 1; i < F; i++)
                    if ((cmp = textBuffer[r + i] - textBuffer[p + i]) != 0) break;

                if (i > matchLength)
                {
                    matchPosition = p;
                    if ((matchLength = i) >= F) break;
                }
            }

            dad[r] = dad[p]; leftSon[r] = leftSon[p]; rightSon[r] = rightSon[p];
            dad[leftSon[p]] = r; dad[rightSon[p]] = r;

            if (rightSon[dad[p]] == p) rightSon[dad[p]] = r;
            else leftSon[dad[p]] = r;

            dad[p] = N;
        }

        private void DeleteNode(int p)
        {
            int q;

            if (dad[p] == N) return;

            if (rightSon[p] == N) q = leftSon[p];
            else if (leftSon[p] == N) q = rightSon[p];
            else
            {
                q = leftSon[p];

                if (rightSon[q] != N)
                {
                    do { q = rightSon[q]; } while (rightSon[q] != N);
                    rightSon[dad[q]] = leftSon[q]; dad[leftSon[q]] = dad[q];
                    leftSon[q] = leftSon[p]; dad[leftSon[p]] = q;
                }

                rightSon[q] = rightSon[p]; dad[rightSon[p]] = q;
            }

            dad[q] = dad[p];

            if (rightSon[dad[p]] == p) rightSon[dad[p]] = q;
            else leftSon[dad[p]] = q;

            dad[p] = N;
        }
        #endregion
    }
}
