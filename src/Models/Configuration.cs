using Devor.Framework.Services.Abstractions;
using System.Reflection;

namespace Devor.Framework.Models
{
    public class Configuration
    {
        public Assembly EntryAssembly { get; set; }
        public IConfigurationLogger Logger { get; set; }
        public string LogConnectionString { get; set; }

        public Configuration()
        {
            EntryAssembly = Assembly.GetEntryAssembly();
        }
    }
}
