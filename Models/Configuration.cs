using System.Reflection;
using Dorbit.Services.Abstractions;

namespace Dorbit.Models
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
