using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    internal class StringTokenWriter : ITokenWriter
    {
        internal const char CodeOpen = '[';
        internal const char CodeClose = ']';

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
                        Output.Append(code.Tag ?? code.Value.ToString());
                        Output.Append(string.Concat(codeToken.Arguments.Select(a => $" {a}")));
                        Output.Append(CodeClose);

                        break;
                    }

                case RawToken rawToken:
                    {
                        Output.Append(CodeOpen);
                        Output.Append(rawToken.Value.ToString());
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
