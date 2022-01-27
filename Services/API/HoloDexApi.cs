using Holodex.NET;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Services.API
{
    public class HoloDexApi
    {
        static string holoDexApiKey;

        public HoloDexApi()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();
            holoDexApiKey = config.GetSection("HoloDexApiKey").ToString();
        }
        
        
    }
}
