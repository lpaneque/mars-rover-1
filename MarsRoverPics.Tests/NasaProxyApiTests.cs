using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using MarsRoverPics.Proxies;
using MarsRoverPics.Proxies.Contracts;
using Microsoft.Extensions.Options;
using Xunit;

namespace MarsRoverPics.Tests
{
    public class NasaProxyApiTests
    {
        private Fixture _fixture;
        private IOptions<NasaApiSettings> _settings;
        private NasaApiProxy _proxy;

        public NasaProxyApiTests()
        {
            _fixture = new Fixture();
            _settings = new NasaApiSettings
            {
                Url = "https://url.com",
                ApiKey = "key"
            };
            _proxy = new NasaApiProxy(new DefaultHttpClientFactory(), _settings);
        }
       

        [Fact]
        public async Task GetPicturesAsync()
        {
            
            // Arrange
            var content = _fixture.Create<RoverResultModel>();

            using (var httpTest = new HttpTest().RespondWithJson(content))
            {
                
                var rover = _fixture.Create<string>();
                var date = _fixture.Create<DateTime>();

                // Act
                var result = await _proxy.GetPicturesAsync(rover, date);

                // Assert
                httpTest.ShouldHaveCalled($"{_settings.Value.Url}/{rover}/photos")
                    .WithQueryParamValue("earth_date", date.ToString("yyyy-M-d"))
                    .WithQueryParamValue("api_key", _settings.Value.ApiKey)
                    .WithVerb(HttpMethod.Get);
                
                Assert.NotNull(result);
            }

           
        }
    }
}
