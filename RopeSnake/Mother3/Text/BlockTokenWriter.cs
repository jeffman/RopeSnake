using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    internal class BlockTokenWriter : ITokenWriter
    {
        public Block Destination { get; }
        public int Position { get; set; }

        protected ICharacterMap _characterMap;
        protected CharacterContext _context;

        public BlockTokenWriter(Block destination, ICharacterMap characterMap)
        {
            Destination = destination;
            _characterMap = characterMap;
            Reset();
        }

        public virtual void Write(Token token)
        {
            switch (token)
            {
                case CharacterToken charToken:
                    {
                        short encoded = _characterMap.Encode(charToken.DecodedCharacter, _context);
                        WriteShort(encoded);
                        break;
                    }

                case ControlCodeToken codeToken:
                    {
                        WriteCodeValue(codeToken);

                        foreach (short arg in codeToken.Arguments)
                            WriteShort(arg);

                        if (codeToken.Code.Flags.HasFlag(ControlCodeFlags.AlternateFont))
                            AlternateContext(ref _context);

                        break;
                    }

                case RawToken rawToken:
                    {
                        WriteShort(rawToken.Value);
                        break;
                    }

                case ContextToken contextToken:
                    {
                        _context = contextToken.Context;
                        break;
                    }

                default:
                    throw new Exception("Unrecognized token");
            }
        }

        public void Reset()
        {
            _context = CharacterContext.Normal;
        }

        protected virtual void WriteShort(short value)
        {
            Destination.WriteShort(Position, value);
            Position += 2;
        }

        protected virtual void WriteCodeValue(ControlCodeToken codeToken)
        {
            WriteShort(codeToken.Code.Value);
        }

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
