using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Animal_Crossing_Text_Editor
{
    class TextEntry
    {
        public int Offset;
        public string Text;
        public int Length;
        public byte[] Data;

        public TextEntry(int offset, byte[] data)
        {
            Offset = offset;
            Text = TextUtility.Decode(data);
            Length = data.Length;
            Data = data;
        }
    }
}
