using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Animal_Crossing_Text_Editor.Classes.Parser.Wii
{
    public static class MessageTags
    {
        public static string GetMessageTagString(byte group, ushort index, in IEnumerable<byte> arguments = null)
        {
            if (Tags.ContainsKey(group) && Tags[group].ContainsKey(index))
            {
                return Tags[group][index](group, index, arguments);
            }

            var argsStr = arguments?.Aggregate("", (result, i) => result + i.ToString("X2") + " ").Trim() ?? "";
            return $"<Unknown MsgTag Group: {group:X2} | Index: {index:X4} | Arguments: {argsStr}>";
        }

        private static readonly Dictionary<byte, Dictionary<ushort, Func<byte, ushort, IEnumerable<byte>, string>>> Tags =
            new Dictionary<byte, Dictionary<ushort, Func<byte, ushort, IEnumerable<byte>, string>>>
            {
                { 0x00, new Dictionary<ushort, Func<byte, ushort, IEnumerable<byte>, string>>
                    {
                        { 0x0003, (_, __, ___) => "<Music Symbol>" },
                    }
                },

                { 0x05, new Dictionary<ushort, Func<byte, ushort, IEnumerable<byte>, string>>
                    {
                        { 0x0005, (_, __, ___) => "<Town Name>" },
                    }
                },
            };

        // TODO: Check for the first index
        private static string GetStringMessage(byte group, ushort index, byte[] arguments = null) => $"<String {index}>";
    }
}
