using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public abstract class ModuleBase : IModule
    {
        public abstract string Name { get; }
        public event ModuleProgressEventHandler Progress;

        public abstract CompileResult Compile(ProjectData data, RomType romType);
        public abstract bool IsCompatibleWith(RomType romType);
        public abstract IEnumerable<Range> GetFreeRanges(RomType romType);
        public abstract ProjectData ReadFromProject(RomType romType, OpenResourceDelegate openResource);
        public abstract ProjectData ReadFromRom(Rom rom);
        public abstract void WriteToProject(ProjectData data, RomType romType, OpenResourceDelegate openResource);
        public abstract void WriteToRom(Rom rom, CompileResult compileResult, AllocationResult allocationResult);

        public virtual void OnProgress(int current, int total)
        {
            Progress?.Invoke(this, new ModuleProgressEventArgs(current, total));
        }
    }
}
