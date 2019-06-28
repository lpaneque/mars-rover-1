using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using MarsRoverPics.Proxies.Contracts;
using MarsRoverPics.Services.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarsRoverPics.Services
{
    /// <summary>
    /// Implementation of the picture service. It will download the images and cache them on storage at /downloads folder relative the wwwroot
    /// </summary>
    public class PictureService: IPictureService
    {
        private readonly INasaApiProxy _proxy;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PictureService> _logger;
        private readonly IFileSystem _fs;
        private readonly IHostingEnvironment _env;
        private readonly List<DateTime> _dates;
        private readonly string[] _rovers = new[] {"opportunity", "curiosity", "spirit"};
        private readonly FlurlClient _client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="cache"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="fs"></param>
        /// <param name="env"></param>
        public PictureService(INasaApiProxy proxy,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IOptions<PictureServiceSettings> options,
            ILogger<PictureService> logger,
            IFileSystem fs,
            IHostingEnvironment env)
        {
             
            _client = new FlurlClient
                {
                    Settings =
                    {
                        HttpClientFactory = httpClientFactory
                    }
                }
                .WithTimeout(TimeSpan.FromSeconds(30));
            _proxy = proxy;
            _cache = cache;
            _logger = logger;
            _fs = fs;
            _env = env;
            //parse dates
            var list = new List<DateTime>();
            foreach (var dateStr in options.Value.Dates)
                if (DateTime.TryParse(dateStr, out var d))
                    list.Add(d);
                else
                    logger?.LogWarning("Date invalid: " + dateStr);

            if (list.Count == 0)
                throw new InvalidOperationException("No valid dates were provided!");
            _dates = list;
             
        }

        ///<inheritdoc />
        public async Task<PictureModel> GetOneAsync()
        {
            var pm = await PerformRandomizeApiCall();

            //see if we have it downloaded yet.
            if (pm.Url != null)
                try
                {
                    var picName = pm.Url.Substring(pm.Url.LastIndexOf("/") + 1);

                    var path = Path.Combine(_env.WebRootPath, "downloads");
                    if (!_fs.File.Exists(Path.Combine(path, picName)))
                        await pm.Url.WithClient(_client)
                            .DownloadFileAsync(path);
                    pm.Url = "/downloads/" + picName;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Problem downloading a picture: " + pm.Url);
                    //just log and return the online url for the pic if something big happens
                }

            return pm;
        }

        /// <summary>
        /// Pefroms the api call, with random behavior
        /// </summary>
        /// <returns></returns>
        private async Task<PictureModel> PerformRandomizeApiCall()
        {
            var r = GetRandom(_rovers);
            var d = GetRandom(_dates);
            var key = r + "-" + d; //key for api call of rover and date
            //if we haven't called that url yet..
            if (!_cache.TryGetValue(key, out RoverResultModel result))
            {
                try
                {
                    result = await _proxy.GetPicturesAsync(r, d); //do it
                    _cache.Set(key, result, TimeSpan.FromHours(1)); //the pics dont change too often. this could be bigger
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Problem calling api");
                }
            }

            //select one random from that call/day
            var pm = new PictureModel() {Date = d, Rover = r, Camera = "N/A"};
            if (result?.photos?.Length > 0)
            {
                var photo = GetRandom(result.photos);
                pm.Url = photo.img_src;
                pm.Camera = photo.camera.full_name;
            }

            return pm;
        }

        #region static private function for random selection

        static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);
        /// <summary>
        /// Picks an element randomnly from an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        private static T GetRandom<T>(IList<T> list)
        {
            var v = Rnd.Next(0, list.Count);
            return list[v];
        }

        #endregion

    }

}
