using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    public sealed class CharacterToken : Token
    {
        public char DecodedCharacter { get; }
        public override bool IsTerminator => false;

        public CharacterToken(char decodedCharacter)
            : base(TokenType.Character)
        {
            DecodedCharacter = decodedCharacter;
        }
    }
}
