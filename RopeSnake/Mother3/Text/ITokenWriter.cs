using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    public interface ITokenWriter
    {
        void Write(Token token);
        void Reset();
    }
}
