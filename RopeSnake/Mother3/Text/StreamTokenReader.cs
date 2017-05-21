using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    internal class StreamTokenReader : ITokenReader
    {
        public Stream BaseStream { get; }
        protected ICharacterMap _characterMap;
        protected IEnumerable<ControlCode> _controlCodes;
        protected CharacterContext _context;

        public StreamTokenReader(Stream stream, ICharacterMap characterMap,
            IEnumerable<ControlCode> controlCodes)
        {
            BaseStream = stream;
            _characterMap = characterMap;
            _controlCodes = controlCodes;
        }

        public void Reset()
        {
            _context = CharacterContext.Normal;
        }

        public Token Read()
        {
            long oldPosition = BaseStream.Position;
            short value = ReadShort();

            if (value >= 0)
            {
                char decoded = _characterMap.Decode(value, _context);
                return new CharacterToken(decoded);
            }
            else
            {
                var code = _controlCodes.FirstOrDefault(c => c.Value == value);

                if (code == null)
                    throw new Exception($"Invalid control code: 0x{value:X4}, stream position 0x{oldPosition:X}");

                var args = new short[code.Arguments];
                for (int i = 0; i < args.Length; i++)
                    args[i] = ReadShort();

                if (code.Flags.HasFlag(ControlCodeFlags.AlternateFont))
                    AlternateContext(ref _context);

                return new ControlCodeToken(code, args);
            }
        }

        protected virtual short ReadShort()
            => BaseStream.ReadShort();

        protected virtual void AlternateContext(ref CharacterContext context)
        {
            switch (context)
            {
                case CharacterContext.Normal:
                    context = CharacterContext.Saturn;
                    break;

                case CharacterContext.Saturn:
                    context = CharacterContext.Normal;
                    break;

                default:
                    throw new Exception($"Unrecognized context: {context}");
            }
        }
    }
}
