using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace RopeSnake.Core
{
    public static class Config
    {
        public static RopeSnakeSettings Settings { get; }

        static Config()
        {
            try
            {
                Settings = ConfigurationManager.GetSection("ropesnake") as RopeSnakeSettings;

                if (Settings == null)
                    throw new Exception("Settings not found");
            }
            catch (Exception ex)
            {
                RLog.Warn("Could not load RopeSnake settings. Using defaults.", ex);
                Settings = new RopeSnakeSettings();
            }
        }
    }

    public sealed class RopeSnakeSettings : ConfigurationSection
    {
        [ConfigurationProperty("multithreaded", DefaultValue = false, IsRequired = false)]
        public bool Multithreaded
        {
            get { return (bool)this["multithreaded"]; }
            set { this["multithreaded"] = value; }
        }

        [ConfigurationProperty("cache.lz77", DefaultValue = false, IsRequired = false)]
        public bool CacheLz77
        {
            get { return (bool)this["cache.lz77"]; }
            set { this["cache.lz77"] = value; }
        }

        public bool CacheEnabled => CacheLz77;
    }
}
