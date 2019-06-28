using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MarsRoverPics.Proxies.Contracts
{
    /// <summary>
    /// Configuration object to carry settings from appsettings.json file into the <see cref="NasaApiProxy"/> class
    /// </summary>
    public class NasaApiSettings: IOptions<NasaApiSettings>
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }

        public NasaApiSettings Value { get; }

        public NasaApiSettings()
        {
            this.Value = this;
        }
    }
}
