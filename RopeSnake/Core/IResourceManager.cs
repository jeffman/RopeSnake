using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public interface IResourceManager
    {
        Stream Open(string name, string extension);
        Stream Create(string name, string extension);
        bool Delete(string name, string extension);
    }
}
