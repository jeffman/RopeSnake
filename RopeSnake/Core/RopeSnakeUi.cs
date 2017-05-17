using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace RopeSnake.Core
{
    public static class RopeSnakeUi
    {
        internal static HashSet<Type> _moduleTypes;
        internal static IEnumerable<IModule> _modules;

        static RopeSnakeUi()
        {
            _moduleTypes = new HashSet<Type>(FindModuleTypes());
        }

        public static void DecompileRom(string romPath, string projectPath, IProgress<float> progress = null)
        {
            RLog.ResetCounts();

            var modules = LoadModules(_moduleTypes);

            var rom = new Rom();
            rom.ReadFromFile(romPath);

            var project = Project.CreateNew(projectPath, rom.Type);

            ModuleProgressEventHandler progressHandler = (s, e) => progress?.Report(e.Fraction);

            foreach (var module in modules.Where(m => m.IsCompatibleWith(rom.Type)))
            {
                RLog.Info($"Decompiling {module.Name}...");

                module.Progress += progressHandler;
                var data = module.ReadFromRom(rom);
                module.WriteToProject(data, rom.Type, (r, e) => project.Get(module.Name, r, e, FileMode.Create));
                module.Progress -= progressHandler;
            }

            project.Save(projectPath);

            RLog.Info($"Decompiled to {projectPath} with {RLog.WarnCount} warnings");
        }

        public static void CompileProject(string projectPath, string baseRomPath, string outputRomPath, IProgress<float> progress = null)
        {
            RLog.ResetCounts();

            var modules = LoadModules(_moduleTypes);

            var rom = new Rom();
            rom.ReadFromFile(baseRomPath);

            var project = Project.Load(projectPath);

            if (rom.Type != project.Type)
                RLog.Warn($"Project type ({project.Type}) does not match base ROM type ({rom.Type})");

            ModuleProgressEventHandler progressHandler = (s, e) => progress?.Report(e.Fraction);
            var compileResults = new Dictionary<IModule, CompileResult>();

            foreach (var module in modules.Where(m => m.IsCompatibleWith(rom.Type)))
            {
                foreach (var freeRange in module.GetFreeRanges(project.Type))
                    rom.Deallocate(freeRange);

                module.Progress += progressHandler;

                RLog.Info($"Compiling {module.Name}...");
                var data = module.ReadFromProject(project.Type, (r, e) => project.Get(module.Name, r, e, FileMode.Open));
                var result = module.Compile(data, project.Type);
                compileResults.Add(module, result);

                module.Progress -= progressHandler;
            }

            RLog.Info($"Allocating ROM space...");
            var allocator = new AggregateAllocator();
            var allocationResults = allocator.Allocate(compileResults, rom);

            foreach (var module in modules.Where(m => m.IsCompatibleWith(rom.Type)))
            {
                module.Progress += progressHandler;

                RLog.Info($"Writing {module.Name} to ROM...");
                module.WriteToRom(rom, compileResults[module], allocationResults[module]);

                module.Progress -= progressHandler;
            }

            rom.WriteToFile(outputRomPath);

            RLog.Info($"Compiled to {outputRomPath} with {RLog.WarnCount} warnings");
        }

        internal static IEnumerable<IModule> LoadModules(IEnumerable<Type> moduleTypes)
        {
            if (_modules != null)
                return _modules;

            RLog.Debug($"Loading modules...");
            _modules = _moduleTypes.Select(t => LoadModule(t)).ToArray();
            return _modules;
        }

        internal static IModule LoadModule(Type moduleType)
        {
            if (!typeof(IModule).IsAssignableFrom(moduleType))
                throw new InvalidOperationException("Not a valid module type");

            RLog.Debug($"Loading module {moduleType.FullName}...");
            return (IModule)Activator.CreateInstance(moduleType);
        }

        internal static Type[] FindModuleTypes()
        {
            RLog.Debug("Finding module types...");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => typeof(IModule).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToArray();

            RLog.Debug($"Found {types.Length} module types:");
            foreach (var type in types)
                RLog.Debug($"  {type.FullName}");

            return types;
        }
    }
}
