namespace Animal_Crossing_Text_Editor
{
    public enum ParserState
    {
        None,
        DecodingCharacter,
        DecodingControlCode,
        DecodingMessageTag,
        EncodingCharacter,
        EncodingControlCode,
        EncodingMessageTag,
        Finished
    }

    public interface IParserInterface
    {
        string Decode(byte[] Input);
        byte[] Encode(string Input);
    }
}