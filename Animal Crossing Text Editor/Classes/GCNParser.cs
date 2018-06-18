using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Animal_Crossing_Text_Editor
{
    class GCNParser : IParserInterface
    {
        static readonly internal byte[] ControlCodeSizeTable = new byte[0x7D]
        {
            0x02, 0x02, 0x02, 0x03, 0x02, 0x05, 0x02, 0x02, 0x05, 0x05, 0x05, 0x05, 0x05, 0x02, 0x04, 0x04,
            0x04, 0x04, 0x04, 0x06, 0x08, 0x0A, 0x06, 0x08, 0x0A, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x06, 0x03, 0x03, 0x03, 0x03, 0x02, 0x04, 0x04, 0x03, 0x03, 0x03, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x02, 0x02, 0x06, 0x03, 0x03, 0x04, 0x03, 0x02, 0x02, 0x06, 0x02, 0x02, 0x03, 0x03, 0x03,
            0x03, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x04, 0x04, 0x0C, 0x0E, 0x02, 0x03
        };

        static readonly internal byte[] UnknownControlCodeInfoTable = new byte[0x7D]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03,
            0x05, 0x00, 0x04, 0x04, 0x05, 0x00, 0x06, 0x06, 0x00, 0x07, 0x04, 0x07, 0x03, 0x03, 0x00, 0x00,
            0x00, 0x03, 0x00, 0x00, 0x08, 0x08, 0x08, 0x04, 0x08, 0x08, 0x00, 0x08, 0x08, 0x08, 0x08, 0x08,
            0x08, 0x02, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x09, 0x04
        };

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

        internal ParserState State = ParserState.None;

        protected internal abstract class MessageParamBase
        {
            public readonly byte Specifier;

            public MessageParamBase(byte Specifier)
            {
                this.Specifier = Specifier;
            }
        }

        protected internal sealed class ControlCode : MessageParamBase
        {
            public byte Id;
            public byte[] Data;

            public ControlCode(byte[] InputData, int ControlCodeStartIndex) : base(0x7F)
            {
                if (IsControlCode(InputData[ControlCodeStartIndex]))
                {
                    Id = InputData[ControlCodeStartIndex + 1];
                    if (Id > 0x7C)
                    {
                        throw new Exception("The Control Code index was greater than the maximum of 0x7C! Got: 0x" + Id.ToString("X2"));
                    }

                    int Size = GetSize();
                    Data = new byte[Size];
                    if (InputData.Length <= ControlCodeStartIndex + Size)
                    {
                        throw new Exception("The Control Code's data is out of bounds of the data array!");
                    }

                    for (int i = 0; i < Size; i++)
                    {
                        Data[i] = InputData[ControlCodeStartIndex + i];
                    }
                }
            }

            public int GetSize()
                => ControlCodeSizeTable[Id];
            public byte[] GetData()
                => Data;
            public byte GetParamU8(int Index = 0)
                => Data[Index + 2];
            public ushort GetParamU16(int Index = 0)
                => (ushort)((Data[Index + 2] << 8) | (Data[Index + 3]));
            public Tuple<byte, ushort> GetParamU8U16(int StartIndex = 0)
                => new Tuple<byte, ushort>(GetParamU8(StartIndex), GetParamU16(StartIndex + 1));
            public Tuple<ushort, ushort> GetParamU16U16(int StartIndex = 0)
                => new Tuple<ushort, ushort>(GetParamU16(StartIndex), GetParamU16(StartIndex + 2));
            public Tuple<ushort, ushort, ushort> GetParamU16U16U16(int StartIndex = 0)
                => new Tuple<ushort, ushort, ushort>(GetParamU16(StartIndex), GetParamU16(StartIndex + 2), GetParamU16(StartIndex + 4));
            public Tuple<byte, ushort, ushort> GetParamU8U16U16(int StartIndex = 0)
                => new Tuple<byte, ushort, ushort>(GetParamU8(StartIndex), GetParamU16(StartIndex + 1), GetParamU16(StartIndex + 3));
            public Tuple<ushort, ushort, ushort, ushort> GetParamU16U16U16U16(int StartIndex = 0)
                => new Tuple<ushort, ushort, ushort, ushort>(GetParamU16(StartIndex), GetParamU16(StartIndex + 2), GetParamU16(StartIndex + 4), GetParamU16(StartIndex + 6));

            public static bool IsControlCode(byte Char)
                => Char == 0x7F;
        }

        protected internal sealed class MessageTag : MessageParamBase
        {
            public byte Size;
            public byte Group;
            public ushort Index;
            public byte[] Data; // The rest of the data (dictated by the "Size" byte - 5)

            public MessageTag(byte[] InputData, int MessageTagStartIndex) : base(0x80)
            {
                if (InputData[MessageTagStartIndex] == Specifier)
                {
                    Size = InputData[MessageTagStartIndex + 1];
                    Group = InputData[MessageTagStartIndex + 2];
                    Index = (ushort)((InputData[MessageTagStartIndex + 3] << 8) | InputData[MessageTagStartIndex + 4]);
                    if (Size - 5 < 1)
                    {
                        Data = new byte[0];
                    }
                    else
                    {
                        Data = new byte[Size - 5];
                        for (int i = 0; i < Data.Length; i++)
                        {
                            Data[i] = InputData[MessageTagStartIndex + 5 + i];
                        }
                    }
                }
            }

            public static bool IsMessageTag(byte Char) => Char == 0x80;

            public byte GetParamU8() => Data[0];
            public ushort GetParamU16() => (ushort)((Data[0] << 8) | Data[1]);

            public void GetParamU8U16(out byte U8, out ushort U16)
            {
                U8 = Data[0];
                U16 = (ushort)((Data[1] << 8) | Data[2]);
            }

            public void GetParamU16U16(out ushort U16A, out ushort U16B)
            {
                U16A = (ushort)((Data[0] << 8) | Data[1]);
                U16B = (ushort)((Data[2] << 8) | Data[3]);
            }

            public void GetParamU8U16U16(out byte U8, out ushort U16A, out ushort U16B)
            {
                U8 = Data[0];
                U16A = (ushort)((Data[1] << 8) | Data[2]);
                U16B = (ushort)((Data[3] << 8) | Data[4]);
            }

            public void GetParamU16U16U16(out ushort U16A, out ushort U16B, out ushort U16C)
            {
                U16A = (ushort)((Data[0] << 8) | Data[1]);
                U16B = (ushort)((Data[2] << 8) | Data[3]);
                U16C = (ushort)((Data[4] << 8) | Data[5]);
            }

            public void GetParamU16U16U16U16(out ushort U16A, out ushort U16B, out ushort U16C, out ushort U16D)
            {
                U16A = (ushort)((Data[0] << 8) | Data[1]);
                U16B = (ushort)((Data[2] << 8) | Data[3]);
                U16C = (ushort)((Data[4] << 8) | Data[5]);
                U16D = (ushort)((Data[6] << 8) | Data[7]);
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
