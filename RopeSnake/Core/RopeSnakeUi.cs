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
        internal static HashSet<Type> ModuleTypes;

        static RopeSnakeUi()
        {
            ModuleTypes = new HashSet<Type>(FindModuleTypes());
        }

        internal static IEnumerable<IModule> LoadModules(IEnumerable<Type> moduleTypes)
        {
            RLog.Debug($"Loading modules...");
            return ModuleTypes.Select(t => LoadModule(t)).ToArray();
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
