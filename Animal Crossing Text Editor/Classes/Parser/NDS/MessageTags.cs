using System;
using System.Collections.Generic;
using System.Linq;

namespace Animal_Crossing_Text_Editor
{
    public static class MessageTags
    {
        public static string GetMessageTagString(byte group, ushort index, in byte[] arguments = null)
        {
            if (Tags.ContainsKey(group) && Tags[group].ContainsKey(index))
            {
                return Tags[group][index](group, index, arguments);
            }

            var argsStr = arguments?.Aggregate("", (result, i) => result + i.ToString("X2") + " ").Trim() ?? "";
            return $"<Unknown MsgTag Group: {group:X2} | Index: {index:X4} | Arguments: {argsStr}>";
        }

        private static readonly Dictionary<byte, Dictionary<ushort, Func<byte, ushort, byte[], string>>> Tags =
            new Dictionary<byte, Dictionary<ushort, Func<byte, ushort, byte[], string>>>
            {
                { 0x00, new Dictionary<ushort, Func<byte, ushort, byte[], string>>
                    {
                        { 0x0000, (_, __, ___) => "<Heart Symbol>" },
                    }
                },

                { 0x01, new Dictionary<ushort, Func<byte, ushort, byte[], string>>
                    {
                        { 0x0000, (_, __, args) => $"<Pause [{BitConverter.ToUInt16(args, 0)}]>" },
                        { 0x0001, (_, __, ___) => "<Press Button>" }, // Not sure about this one.
                        { 0x0008, (_, __, ___) => "<Small Vibration>" },
                        { 0x0009, (_, __, ___) => "<Medium Vibration>" },
                        { 0x000A, (_, __, ___) => "<Small Vibration>" },
                    }
                },

                { 0x04, new Dictionary<ushort, Func<byte, ushort, byte[], string>>
                    {
                        { 0x0002, (_, __, ___) => "<Catchphrase>" },
                        { 0x0003, (_, __, ___) => "<Town Name>" },
                        { 0x000F, GetStringMessage },
                        { 0x0010, GetStringMessage },
                        { 0x001A, GetStringMessage }
                    }
                },

                { 0x06, new Dictionary<ushort, Func<byte, ushort, byte[], string>>
                    {
                        { 0x0000, (_, __, args) => $"<Select Random Dialog from [{args[0]:X2}] [{args[1]:X2}]>" },
                        { 0x0001, (_, __, args) => $"<Select Random Dialog from [{args[0]:X2}] [{args[1]:X2}] [{args[2]:X2}]>" }
                    }
                },

                { 0x07, new Dictionary<ushort, Func<byte, ushort, byte[], string>>
                    {
                        { 0x000B, (_, __, ___) => "<Expression [Wondering]>" },
                    }
                },

                { 0xFF, new Dictionary<ushort, Func<byte, ushort, byte[], string>>
                    {
                        { 0x0000, (_, __, args) => $"<Line Color [{args[0]}]>" }
                    }
                }
            };

        // TODO: Check for the first index
        private static string GetStringMessage(byte group, ushort index, byte[] arguments = null) => $"<String {index}>";
    }
}
