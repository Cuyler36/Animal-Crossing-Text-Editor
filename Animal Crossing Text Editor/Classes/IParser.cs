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

    public interface IParser
    {
        string Decode(in byte[] input);
        byte[] Encode(string input);
    }
}