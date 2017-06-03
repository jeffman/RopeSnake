using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    public interface ICharacterMap
    {
        char Decode(short value, CharacterContext context);
        short Encode(char ch, CharacterContext context);
        CharacterContext GetContext(short value);
        bool IsSharedCharacter(char ch);
    }

    public enum CharacterContext
    {
        None,
        Normal,
        Saturn
    }
}
