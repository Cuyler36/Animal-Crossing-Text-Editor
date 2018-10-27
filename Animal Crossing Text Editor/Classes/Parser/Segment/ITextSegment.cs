using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Animal_Crossing_Text_Editor.Classes.Parser.Segment
{
    public interface ITextSegment
    {
        string Text { get; }
        string Commands { get; }
    }
}
