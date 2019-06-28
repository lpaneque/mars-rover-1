using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using MarsRoverPics.Proxies.Contracts;
using Microsoft.Extensions.Options;

namespace MarsRoverPics.Proxies
{
    /// <summary>
    /// Implementation of a proxy class to access NASA API
    /// </summary>
    public class NasaApiProxy: INasaApiProxy
    {
        private readonly FlurlClient _client;
        private readonly NasaApiSettings _settings;

        public NasaApiProxy(IHttpClientFactory httpClientFactory, IOptions<NasaApiSettings> options)
        {
            _settings = options.Value;
            _client = new FlurlClient
            {
                Settings =
                {
                    HttpClientFactory = httpClientFactory
                }
            }
            .WithTimeout(TimeSpan.FromSeconds(30));
        }

        /// <inheritdoc />
        public async Task<RoverResultModel> GetPicturesAsync(string rover, DateTime date)
        {
            return await _settings.Url.AppendPathSegment($"{rover}/photos")
                .SetQueryParam("earth_date",date.ToString("yyyy-M-d"))
                .SetQueryParam("api_key",_settings.ApiKey)
                .WithClient(_client)
                .GetJsonAsync<RoverResultModel>();
        }
    }
}
