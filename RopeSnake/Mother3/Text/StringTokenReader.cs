using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    internal class StringTokenReader : ITokenReader
    {
        private string _baseString;
        public string BaseString
        {
            get => _baseString;
            set
            {
                _baseString = value;
                Position = 0;
            }
        }

        public int Position { get; set; }
        protected IEnumerable<ControlCode> _controlCodes;

        public StringTokenReader(IEnumerable<ControlCode> controlCodes)
        {
            Position = 0;
            _controlCodes = controlCodes;
        }

        public Token Read()
        {
            if (Position >= BaseString.Length)
                return null;

            int oldPosition = Position;
            char next = BaseString[Position++];

            if (next == StringTokenWriter.CodeOpen)
            {
                var codeBuilder = new StringBuilder();
                var chunks = new List<string>();

                bool openCode = true;

                while (openCode)
                {
                    int innerOldPosition = Position;
                    char inner = BaseString[Position++];

                    if (inner == StringTokenWriter.CodeClose)
                    {
                        openCode = false;
                    }
                    else if (inner == StringTokenWriter.CodeOpen)
                    {
                        throw new Exception($"Double opening bracket: position {innerOldPosition}");
                    }
                    else if (inner == ' ')
                    {
                        if (codeBuilder.Length > 0)
                            chunks.Add(codeBuilder.ToString());

                        codeBuilder.Clear();
                    }
                    else
                    {
                        codeBuilder.Append(inner);
                    }
                }

                if (codeBuilder.Length > 0)
                    chunks.Add(codeBuilder.ToString());

                // Order for matching codes:
                // 1) Check tag first
                // 2) If no codes have that tag, interpret it as a number
                //    a) If no codes have that number, interpret it as a raw literal
                if (chunks.Count == 0)
                    throw new Exception($"Empty code: position {oldPosition}");

                string firstChunk = chunks[0];
                var code = _controlCodes.FirstOrDefault(c => StringComparer.OrdinalIgnoreCase.Equals(c.Tag, firstChunk));

                if (code == null)
                {
                    if (!short.TryParse(firstChunk, out short codeValue))
                        throw new Exception($"Could not parse as a number: {firstChunk}");

                    code = _controlCodes.FirstOrDefault(c => c.Value == codeValue);

                    if (code == null)
                    {
                        RLog.Warn($"Unrecognized control code: {codeValue} at position {oldPosition}. Treating as raw value.");
                        return new RawToken(codeValue);
                    }
                }

                if (chunks.Count - 1 != code.Arguments)
                    throw new Exception($"Expected {code.Arguments} arguments but found {chunks.Count - 1}: position {oldPosition}");

                var args = new short[code.Arguments];

                for (int i = 0; i < args.Length; i++)
                {
                    string chunk = chunks[i + 1];

                    if (!short.TryParse(chunk, out short argValue))
                        throw new Exception($"Could not parse as a number: {chunk}");

                    args[i] = argValue;
                }

                return new ControlCodeToken(code, args);
            }
            else if (next == StringTokenWriter.CodeClose)
            {
                throw new Exception($"Closing bracket without opening bracket: position {oldPosition}");
            }
            else
            {
                return new CharacterToken(next);
            }
        }

        public void Reset() { }
    }
}
