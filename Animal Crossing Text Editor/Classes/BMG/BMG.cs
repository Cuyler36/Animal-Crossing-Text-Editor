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
        public bool IsLittleEndian;
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
}
