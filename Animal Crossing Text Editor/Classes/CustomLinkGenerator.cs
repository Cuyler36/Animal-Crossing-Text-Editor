using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Animal_Crossing_Text_Editor
{
    public class CustomLinkGenerator : VisualLineElementGenerator
    {
        readonly static Regex MatchRegex = new Regex(@"\[[a-fA-F0-9]{4}\]");

        public CustomLinkGenerator() { }

        Match FindMatch(int startOffset)
        {
            // fetch the end offset of the VisualLine being generated
            int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
            TextDocument document = CurrentContext.Document;
            string relevantText = document.GetText(startOffset, endOffset - startOffset);

            return MatchRegex.Match(relevantText);
        }

        /// Gets the first offset >= startOffset where the generator wants to construct
        /// an element.
        /// Return -1 to signal no interest.
        public override int GetFirstInterestedOffset(int startOffset)
        {
            Match m = FindMatch(startOffset);
            return m.Success ? (startOffset + m.Index) : -1;
        }

        /// Constructs an element at the specified offset.
        /// May return null if no element should be constructed.
        public override VisualLineElement ConstructElement(int offset)
        {
            Match m = FindMatch(offset);
            // check whether there's a match exactly at offset
            if (m.Success && m.Index == 0)
            {
                return new CustomLinkVisualLineText(m.Value.Replace("[", "").Replace("]", ""), CurrentContext.VisualLine, m.Length);
            }
            return null;
        }
    }
}
