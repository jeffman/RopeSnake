using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    internal class StreamTokenReader : ITokenReader
    {
        public Block Source { get; }
        public int Position { get; set; }

        protected ICharacterMap _characterMap;
        protected IEnumerable<ControlCode> _controlCodes;
        protected CharacterContext _context;

        public StreamTokenReader(Block source, ICharacterMap characterMap,
            IEnumerable<ControlCode> controlCodes)
        {
            Source = source;
            _characterMap = characterMap;
            _controlCodes = controlCodes;
        }

        public virtual void Reset()
        {
            _context = CharacterContext.Normal;
        }

        public virtual Token Read()
        {
            int oldPosition = Position;
            short value = ReadShort();
            return OnRead(value, oldPosition);
        }

        protected Token OnRead(short value, int oldPosition)
        {
            if (value >= 0)
            {
                char decoded = _characterMap.Decode(value, _context);
                return new CharacterToken(decoded);
            }
            else
            {
                var code = _controlCodes.FirstOrDefault(c => c.Value == value);

                if (code == null)
                {
                    RLog.Warn($"Encountered unknown control code: 0x{code:X4} at position 0x{oldPosition:X}. Outputting as raw literal.");
                    return new RawToken(value);
                }

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
