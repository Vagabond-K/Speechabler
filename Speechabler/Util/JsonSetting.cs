using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler
{
    static class JsonSetting
    {
        static JsonSetting()
        {
            AppFileDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        private static string AppFileDirectory { get; }

        private static string GetSettingFilePath<TSetting>()
            => Path.Combine(AppFileDirectory, typeof(TSetting).Name + ".json");

        public static TSetting LoadSetting<TSetting>() where TSetting : class
            => File.Exists(GetSettingFilePath<TSetting>()) ? JsonConvert.DeserializeObject<TSetting>(File.ReadAllText(GetSettingFilePath<TSetting>())) : null;

        public static void SaveSetting<TSetting>(TSetting setting) where TSetting : class
            => File.WriteAllText(GetSettingFilePath<TSetting>(), JsonConvert.SerializeObject(setting));
    }
}
