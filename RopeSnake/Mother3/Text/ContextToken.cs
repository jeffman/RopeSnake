using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    public sealed class ContextToken : Token
    {
        public CharacterContext Context { get; }
        public override bool IsTerminator => false;

        public ContextToken(CharacterContext context) : base(TokenType.Context)
        {
            Context = context;
        }
    }
}
