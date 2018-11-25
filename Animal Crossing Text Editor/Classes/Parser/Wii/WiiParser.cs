using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Animal_Crossing_Text_Editor.Classes.Parser.Wii
{
    public sealed class WiiParser : IParser
    {
        private const int MinimumMsgTagSize = 6;

        public string Decode(in byte[] data)
        {
            var message = "";

            // Parse each character searching for the 0x001A character.
            for (var i = 0; i < data.Length; i += 2)
            {
                var currentCharacter = BitConverter.ToUInt16(data, i).Reverse();

                // Check to see if character is the command character (0x001A) & handle command if it is.
                if (currentCharacter == 0x1A)
                {
                    var size = data[i + 2];
                    var group = data[i + 3];
                    var index = BitConverter.ToUInt16(data, i + 4).Reverse();
                    var arguments = data.Skip(i + 6).Take(size - MinimumMsgTagSize);

                    message += MessageTags.GetMessageTagString(group, index, arguments);

                    i += size - 2;
                }
                else
                {
                    message += Encoding.BigEndianUnicode.GetString(data, i, 2);
                }
            }

            return message.Replace("\0", "");
        }

        public byte[] Encode(string text)
        {
            return new byte[0]; // TODO: Stubbed.
        }
    }
}
