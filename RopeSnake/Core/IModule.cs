using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public interface IModule
    {
        string Name { get; }
        void ReadFromRom(Rom rom);
        void WriteToRom(Rom rom);
        void ReadFromProject(OpenResourceDelegate openResource);
        void WriteToProject(OpenResourceDelegate openResource);
    }
}
