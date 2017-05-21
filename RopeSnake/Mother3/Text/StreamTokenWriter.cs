using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    internal class StreamTokenWriter : ITokenWriter
    {
        public Stream BaseStream { get; }
        protected ICharacterMap _characterMap;
        protected CharacterContext _context;

        public StreamTokenWriter(Stream stream, ICharacterMap characterMap)
        {
            BaseStream = stream;
            _characterMap = characterMap;
            Reset();
        }

        public void Write(Token token)
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
                        WriteShort(codeToken.Code.Value);

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
            BaseStream.WriteShort(value);
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
