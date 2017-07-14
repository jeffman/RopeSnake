using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    internal class EnglishStreamTokenWriter : StreamTokenWriter
    {
        public bool IsCompressed { get; set; }
        public ScriptEncodingParameters EncodingParameters { get; internal set; }

        public EnglishStreamTokenWriter(Stream stream, ICharacterMap characterMap,
            bool isCompressed, ScriptEncodingParameters encodingParameters)
            : base(stream, characterMap)
        {
            IsCompressed = isCompressed;
            EncodingParameters = encodingParameters;
        }

        protected override void WriteShort(short value)
        {
            EncodeShort(value);
        }

        protected override void WriteCodeValue(ControlCodeToken codeToken)
        {
            if (IsCompressed)
            {
                switch (codeToken.Code.Value)
                {
                    case -1:
                        // End
                        EncodeByte(0xFF);
                        break;

                    case -257:
                        // Hot spring
                        EncodeByte(0xFE);
                        break;

                    case -4352:
                        // Article/custom
                        EncodeByte(0xEF);
                        EncodeByte((byte)codeToken.Arguments[0]);
                        break;

                    default:
                        EncodeByte((byte)(0xF0 | codeToken.Code.Arguments));
                        EncodeByte((byte)(codeToken.Code.Value & 0xFF));
                        break;
                }
            }
            else
            {
                base.WriteCodeValue(codeToken);
            }
        }

        internal void EncodeByte(byte value)
        {
            if (EncodingParameters != null)
            {
                int encodePosition = (int)BaseStream.Position + 0x8000000;
                long oldPosition = BaseStream.Position;
                bool even = (encodePosition & 1) == 0;

                if (even)
                {
                    BaseStream.Position = EncodingParameters.EvenPadAddress + ((encodePosition >> 1) % EncodingParameters.EvenPadModulus);
                    byte key = BaseStream.GetByte();
                    value = (byte)(((value - EncodingParameters.EvenOffset2) ^ key) - EncodingParameters.EvenOffset1);
                }
                else
                {
                    BaseStream.Position = EncodingParameters.OddPadAddress + ((encodePosition >> 1) % EncodingParameters.OddPadModulus);
                    byte key = BaseStream.GetByte();
                    value = (byte)(((value - EncodingParameters.OddOffset2) ^ key) - EncodingParameters.OddOffset1);
                }

                BaseStream.Position = oldPosition;
            }

            BaseStream.WriteByte(value);
        }

        internal void EncodeShort(short value)
        {
            EncodeByte((byte)(value & 0xFF));
            EncodeByte((byte)((value >> 8) & 0xFF));
        }
    }
}
