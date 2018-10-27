using System;
using System.Diagnostics;
using System.Linq;

namespace Animal_Crossing_Text_Editor
{
    public sealed class NDSParser : IParser
    {
        private const char CommandCharacter = '\x1A';

        public string Decode(in byte[] data)
        {
            var readPosition = 0;
            var dictionaryReference = TextUtility.WildWorldCharacterMap; // TODO: Make this an array after figuring out each character's hex code.
            var text = "";

            while (readPosition < data.Length)
            {
                var currentCharacter = data[readPosition];

                // Check if the current character is the text command character.
                if ((char) currentCharacter == CommandCharacter)
                {
                    if (readPosition + 1 >= data.Length) return text;

                    // TODO: I'm not fully sure if this format is correct. It seems to be similar to e+'s MsgTag system.
                    var size = data[readPosition + 1];
                    if (readPosition + size >= data.Length) return text;

                    var group = data[readPosition + 2];
                    var index = BitConverter.ToUInt16(data, readPosition + 3);
                    byte[] arguments = null;

                    if (size > 5)
                    {
                        arguments = data.Skip(readPosition + 5).Take(size - 5).ToArray();
                    }

                    Debug.WriteLine($"MsgTag found! Size: {size:X2} Group: {group:X2} Index: {index:X4}");
                    text += MessageTags.GetMessageTagString(group, index, arguments);

                    // Increment the read position by size - 1. The position will be incremented again by the end of the loop.
                    readPosition += size - 1;
                }
                else
                {
                    if (dictionaryReference.ContainsKey(currentCharacter))
                    {
                        text += dictionaryReference[currentCharacter];
                    }
                    else
                    {
                        Debug.WriteLine($"Character for {currentCharacter:X2} isn't present in the dictionary!");
                    }
                }

                readPosition++;
            }

            return text;
        }

        public byte[] Encode(string text)
        {
            throw new NotImplementedException();
        }
    }
}
