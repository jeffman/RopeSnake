using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    internal class StringTokenWriter : ITokenWriter
    {
        public int Position
        {
            get => Output.Length;
            set => throw new InvalidOperationException("Cannot set the position of a StringBuilder");
        }

        internal const char CodeOpen = '[';
        internal const char CodeClose = ']';

        internal static Dictionary<CharacterContext, string> _contextToString =
            new Dictionary<CharacterContext, string>()
            {
                [CharacterContext.Normal] = "NORMAL",
                [CharacterContext.Saturn] = "SATURN"
            };

        internal StringBuilder Output { get; }

        public StringTokenWriter(StringBuilder output)
        {
            Output = output;
        }

        public void Write(Token token)
        {
            switch (token)
            {
                case CharacterToken charToken:
                    {
                        Output.Append(charToken.DecodedCharacter);
                        break;
                    }

                case ControlCodeToken codeToken:
                    {
                        var code = codeToken.Code;

                        Output.Append(CodeOpen);
                        Output.Append(code.Tag ?? code.Value.ToString("X4"));
                        Output.Append(string.Concat(codeToken.Arguments.Select(a => $" {a:X}")));
                        Output.Append(CodeClose);

                        break;
                    }

                case RawToken rawToken:
                    {
                        Output.Append(CodeOpen);
                        Output.Append(rawToken.Value.ToString("X4"));
                        Output.Append(CodeClose);

                        break;
                    }

                case ContextToken contextToken:
                    {
                        Output.Append(CodeOpen);
                        Output.Append(_contextToString[contextToken.Context]);
                        Output.Append(CodeClose);

                        break;
                    }

                default:
                    throw new Exception("Unrecognized token");
            }
        }

        public void Reset() { }
    }
}
