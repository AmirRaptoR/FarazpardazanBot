using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Farazpardazan.ParkingBot
{
    public class Database
    {
        public string Filename { get; }

        private IDictionary<string, object> _data = new ConcurrentDictionary<string, object>();

        public Database(IConfiguration configuration)
        {
            Filename = configuration.GetValue<string>("database:filename");
            Reload().Wait();
        }

        public TData GetData<TData>(string name)
        {
            return _data.ContainsKey(name) ? JsonConvert.DeserializeObject<TData>(JsonConvert.SerializeObject(_data[name])) : default;
        }

        public void SetData<TData>(string name, TData data)
        {
            _data[name] = data;
        }

        public async Task Reload()
        {
            if (!File.Exists(Filename))
            {
                File.Create(Filename);
            }

            _data = JsonConvert.DeserializeObject<IDictionary<string, object>>(await File.ReadAllTextAsync(Filename, Encoding.UTF8)) ?? 
                new ConcurrentDictionary<string, object>();
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(Filename, JsonConvert.SerializeObject(_data), Encoding.UTF8);
        }
    }
}