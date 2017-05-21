using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    public sealed class RawToken : Token
    {
        public short Value { get; }
        public override bool IsTerminator => Value == -1;

        public RawToken(short value) : base(TokenType.Raw)
        {
            Value = value;
        }
    }
}
