using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    public abstract class Token
    {
        public TokenType Type { get; }
        public abstract bool IsTerminator { get; }

        protected Token(TokenType type)
        {
            Type = type;
        }
    }

    public enum TokenType
    {
        None = 0,
        Character,
        ControlCode
    }
}
