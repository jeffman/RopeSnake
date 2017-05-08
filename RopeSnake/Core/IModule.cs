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
        event ModuleProgressEventHandler Progress;

        void ReadFromRom(Rom rom);
        void WriteToRom(Rom rom);
        void ReadFromProject(OpenResourceDelegate openResource);
        void WriteToProject(OpenResourceDelegate openResource);
    }

    public sealed class ModuleProgressEventArgs : EventArgs
    {
        public int CurrentIndex { get; }
        public int Total { get; }
        public float Fraction => (float)CurrentIndex / Total;

        public ModuleProgressEventArgs(int currentIndex, int total)
        {
            CurrentIndex = currentIndex;
            Total = total;
        }
    }
}
