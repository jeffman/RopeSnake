﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    internal class EnglishStreamTokenReader : StreamTokenReader
    {
        public bool IsCompressed { get; set; }
        public ScriptEncodingParameters EncodingParameters { get; internal set; }
        internal Queue<short> _compressBuffer = new Queue<short>();
        internal short? _contextSwapBuffer = null;

        public EnglishStreamTokenReader(Stream stream, ICharacterMap characterMap,
            IEnumerable<ControlCode> controlCodes, bool isCompressed, ScriptEncodingParameters encodingParameters)
            : base(stream, characterMap, controlCodes)
        {
            IsCompressed = isCompressed;
            EncodingParameters = encodingParameters;
        }

        public override void Reset()
        {
            base.Reset();
            _compressBuffer.Clear();
            _contextSwapBuffer = null;
        }

        public override Token Read()
        {
            long oldPosition = BaseStream.Position;

            if (_contextSwapBuffer != null)
            {
                short bufferValue = _contextSwapBuffer.Value;
                _contextSwapBuffer = null;
                return OnRead(bufferValue, oldPosition);
            }

            short value = ReadShort();

            if (value >= 0)
            {
                var newContext = _characterMap.GetContext(value);

                if (newContext == CharacterContext.None)
                {
                    RLog.Warn($"Unrecognized character: 0x{value:X}");
                    return new CharacterToken('?');
                }

                if (newContext != _context)
                {
                    _contextSwapBuffer = value;
                    _context = newContext;
                    return new ContextToken(_context);
                }

                return OnRead(value, oldPosition);
            }
            else
            {
                return OnRead(value, oldPosition);
            }
        }

        protected override short ReadShort()
        {
            if (IsCompressed)
            {
                if (_compressBuffer.Count > 0)
                    return _compressBuffer.Dequeue();

                byte value = DecodeByte();
                if (value >= 0xF0)
                {
                    int args = value & 0xF;

                    if (args == 0xF)
                    {
                        // End code
                        return -1;
                    }
                    else if (args == 0xE)
                    {
                        // Hot spring
                        return -257;
                    }

                    byte codeValue = DecodeByte();
                    short code = (short)(codeValue | 0xFF00);

                    for (int i = 0; i < args; i++)
                    {
                        _compressBuffer.Enqueue(DecodeShort());
                    }

                    return code;
                }
                else if (value == 0xEF)
                {
                    // Article/custom code
                    short code = -4352;
                    _compressBuffer.Enqueue(DecodeByte());
                    return code;
                }
                else
                {
                    return value;
                }
            }
            else
            {
                return DecodeShort();
            }
        }

        internal byte DecodeByte()
        {
            int decodePosition = (int)BaseStream.Position + 0x8000000;
            byte value = BaseStream.GetByte();

            if (EncodingParameters != null)
            {
                long oldPosition = BaseStream.Position;
                bool even = (decodePosition & 1) == 0;

                if (even)
                {
                    BaseStream.Position = EncodingParameters.EvenPadAddress + ((decodePosition >> 1) % EncodingParameters.EvenPadModulus);
                    byte key = BaseStream.GetByte();
                    value = (byte)(((value + EncodingParameters.EvenOffset1) ^ key) + EncodingParameters.EvenOffset2);
                }
                else
                {
                    BaseStream.Position = EncodingParameters.OddPadAddress + ((decodePosition >> 1) % EncodingParameters.OddPadModulus);
                    byte key = BaseStream.GetByte();
                    value = (byte)(((value + EncodingParameters.OddOffset1) ^ key) + EncodingParameters.OddOffset2);
                }

                BaseStream.Position = oldPosition;
            }

            return value;
        }

        internal short DecodeShort() => (short)(DecodeByte() | (DecodeByte() << 8));
    }
}
