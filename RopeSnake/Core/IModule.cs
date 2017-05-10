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

        /// <summary>
        /// Called when this IModule wishes to express a progress update.
        /// </summary>
        event ModuleProgressEventHandler Progress;

        /// <summary>
        /// Reset the module to a freshly-initialized state. All project data is released.
        /// </summary>
        void Reset();

        /// <summary>
        /// Read project data from a ROM.
        /// </summary>
        /// <param name="rom">ROM to read from</param>
        /// <returns>decompiled project data</returns>
        ProjectData ReadFromRom(Rom rom);

        /// <summary>
        /// Compile project data into blocks.
        /// </summary>
        /// <param name="data">data to compile</param>
        /// <param name="romType">ROM type associated with the project data</param>
        /// <returns>collection of static and allocatable blocks</returns>
        CompileResult Compile(ProjectData data, RomType romType);

        /// <summary>
        /// Write all compiled data to ROM and perform any ROM cleanup, i.e. updating ASM pointers.
        /// </summary>
        /// <param name="rom">ROM to write to</param>
        /// <param name="compileResult">result from calling Compile</param>
        /// <param name="allocationResult">summary of block allocations in ROM</param>
        void WriteToRom(Rom rom, CompileResult compileResult, AllocationResult allocationResult);

        /// <summary>
        /// Read all project data from disk.
        /// </summary>
        /// <param name="romType">ROM type associated with the project</param>
        /// <param name="openResource"></param>
        /// <returns>read project data</returns>
        ProjectData ReadFromProject(RomType romType, OpenResourceDelegate openResource);

        /// <summary>
        /// Write all project data to disk.
        /// </summary>
        /// <param name="data">data to write</param>
        /// <param name="romType">ROM type associated with the project data</param>
        /// <param name="openResource"></param>
        void WriteToProject(ProjectData data, RomType romType, OpenResourceDelegate openResource);
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
