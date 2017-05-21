using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    public sealed class ControlCodeToken : Token
    {
        public short[] Arguments { get; }
        public ControlCode Code { get; }
        public override bool IsTerminator => Code.Flags.HasFlag(ControlCodeFlags.Terminate);

        public ControlCodeToken(ControlCode code, short[] arguments)
            : base(TokenType.ControlCode)
        {
            Arguments = arguments;
            Code = code;
        }

        public ControlCodeToken(ControlCode code)
            : this(code, new short[0]) { }
    }
}
