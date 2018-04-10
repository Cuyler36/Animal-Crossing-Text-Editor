using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Animal_Crossing_Text_Editor
{
    class GCNParser : IParserInterface
    {
        internal enum DataType
        {
            Byte,
            UShort,
            HexByte,
            HexUShort
        }

        internal enum MsgTagParamType
        {
            U8,
            U16,
            U8U16,
            U16U16,
            U8U16U16,
            U16U16U16,
            U16U16U16U16
        }

        Dictionary<string, DataType[]> ControlCodeDictionary = new Dictionary<string, DataType[]>();
        List<Dictionary<string, MsgTagParamType>> MessageTagDictionary = new List<Dictionary<string, MsgTagParamType>>();

        ParserState State = ParserState.None;

        protected internal sealed class MessageTag
        {
            public byte TagId = 0x80;
            public byte Size;
            public byte Group;
            public ushort Index;
            public byte[] TagData; // The rest of the data (dictated by the "Size" byte - 5)

            public MessageTag(byte[] InputData, int MessageTagStartIndex)
            {
                if (InputData[MessageTagStartIndex] == TagId)
                {
                    Size = InputData[MessageTagStartIndex + 1];
                    Group = InputData[MessageTagStartIndex + 2];
                    Index = (ushort)((InputData[MessageTagStartIndex + 3] << 8) | InputData[MessageTagStartIndex + 4]);
                    if (Size - 5 < 1)
                    {
                        TagData = new byte[0];
                    }
                    else
                    {
                        TagData = new byte[Size - 5];
                        for (int i = 0; i < TagData.Length; i++)
                        {
                            TagData[i] = InputData[MessageTagStartIndex + 5 + i];
                        }
                    }
                }
            }

            public static bool IsMessageTag(byte[] InputData, int Index) => InputData[Index] == 0x80;

            public byte GetParamU8() => TagData[0];
            public ushort GetParamU16() => (ushort)((TagData[0] << 8) | TagData[1]);

            public void GetParamU8U16(out byte U8, out ushort U16)
            {
                U8 = TagData[0];
                U16 = (ushort)((TagData[1] << 8) | TagData[2]);
            }

            public void GetParamU16U16(out ushort U16A, out ushort U16B)
            {
                U16A = (ushort)((TagData[0] << 8) | TagData[1]);
                U16B = (ushort)((TagData[2] << 8) | TagData[3]);
            }

            public void GetParamU8U16U16(out byte U8, out ushort U16A, out ushort U16B)
            {
                U8 = TagData[0];
                U16A = (ushort)((TagData[1] << 8) | TagData[2]);
                U16B = (ushort)((TagData[3] << 8) | TagData[4]);
            }

            public void GetParamU16U16U16(out ushort U16A, out ushort U16B, out ushort U16C)
            {
                U16A = (ushort)((TagData[0] << 8) | TagData[1]);
                U16B = (ushort)((TagData[2] << 8) | TagData[3]);
                U16C = (ushort)((TagData[4] << 8) | TagData[5]);
            }

            public void GetParamU16U16U16U16(out ushort U16A, out ushort U16B, out ushort U16C, out ushort U16D)
            {
                U16A = (ushort)((TagData[0] << 8) | TagData[1]);
                U16B = (ushort)((TagData[2] << 8) | TagData[3]);
                U16C = (ushort)((TagData[4] << 8) | TagData[5]);
                U16D = (ushort)((TagData[6] << 8) | TagData[7]);
            }
        }

        protected internal string GetDataString(int Input, DataType Type)
        {
            switch (Type)
            {
                case DataType.Byte:
                    return (Input & 0xFF).ToString();
                case DataType.HexByte:
                    return (Input & 0xFF).ToString("X2");
                case DataType.UShort:
                    return (Input & 0xFFFF).ToString();
                case DataType.HexUShort:
                    return (Input & 0xFFFF).ToString("X4");
                default:
                    return Input.ToString();
            }
        }

        protected internal string GetControlCodeString(byte ControlCode, byte[] ControlCodeData)
        {
            throw new NotImplementedException();
        }

        protected internal byte[] EncodeControlCode(string ControlCodeString)
        {
            throw new NotImplementedException();
        }

        protected internal string DecodeMessageTag(byte[] InputData, int Index)
        {
            var Tag = new MessageTag(InputData, Index);
            KeyValuePair<string, MsgTagParamType> MessageTagInfo = MessageTagDictionary.ElementAt(Tag.Group).ElementAt(Tag.Index);

            string Argument = "";
            switch (MessageTagInfo.Value)
            {
                case MsgTagParamType.U8:
                    Argument = GetDataString(Tag.GetParamU8(), DataType.HexByte);
                    break;
                case MsgTagParamType.U16:
                    Argument = GetDataString(Tag.GetParamU16(), DataType.HexUShort);
                    break;
                    // TODO: Rest of types
            }

            return string.Format(MessageTagInfo.Key, Argument); // TODO: Argument needs to be an array
        }

        public string Decode(byte[] Input)
        {
            throw new NotImplementedException();
        }

        public byte[] Encode(string Input)
        {
            throw new NotImplementedException();
        }
    }
}
