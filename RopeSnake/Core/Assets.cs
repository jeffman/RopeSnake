using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public static class Assets
    {
        internal const string AssetsPath = "Assets";

        public static Stream Open(string name)
            => File.OpenRead(Path.Combine(AssetsPath, name));

        public static T Parse<T>(string name, Func<string, T> parser)
        {
            return parser(Path.Combine(AssetsPath, name));
        }
    }
}
