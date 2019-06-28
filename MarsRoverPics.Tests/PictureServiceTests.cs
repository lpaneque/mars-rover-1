using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using MarsRoverPics.Proxies.Contracts;
using MarsRoverPics.Services;
using MarsRoverPics.Services.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace MarsRoverPics.Tests
{
    public class PictureServiceTests
    {
        private Mock<INasaApiProxy> _proxy;
        private PictureService _service;
        private Mock<IFileSystem> _fs;
        private Mock<IHostingEnvironment> _env;
        private Fixture _fixture;
        private Mock<IFile> _file;
        private MemoryCache _memoryCache;
        private PictureServiceSettings _settings;

        public PictureServiceTests()
        {
            _fixture = new Fixture();
            _proxy = new Mock<INasaApiProxy>();
            _file = new Mock<IFile>();
            _fs = new Mock<IFileSystem>();
            _fs.Setup(s => s.File).Returns(_file.Object);
            _env = new Mock<IHostingEnvironment>();
            _env.Setup(e => e.WebRootPath).Returns("WebRootPath");
            _settings = new PictureServiceSettings() {Dates = new[] {"1/1/2019"}};
            //use memory cache in an "integration" test mode to save time mocking behavior
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _service = new PictureService(_proxy.Object, new DefaultHttpClientFactory(), _memoryCache,
                _settings, null, _fs.Object, _env.Object);
        }


        [Fact]
        public void ConstructorOK()
        {
            var settings = new PictureServiceSettings() {Dates = new[] {"1/1/2019"}};
            _service = new PictureService(_proxy.Object, new DefaultHttpClientFactory(), new MemoryCache(new MemoryCacheOptions()),
                settings, null, _fs.Object, _env.Object);
        }

        [Fact]
        public void ConstructorFail()
        {
            var settings = new PictureServiceSettings() {Dates = new[] {"not a date"}};
            Assert.ThrowsAny<InvalidOperationException>(() =>
                  new PictureService(_proxy.Object, new DefaultHttpClientFactory(), new MemoryCache(new MemoryCacheOptions()),
                    settings, null, _fs.Object, _env.Object));
        }

        /// <summary>
        /// Good scenario, api goes well, nothing is cached and file does not exist
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetOneAsync_AllOk()
        {
            //arrange
          
            var httpTest = new HttpTest();
            httpTest.RespondWith("soem content");
            var apiCallResult = _fixture.Create<RoverResultModel>();
            foreach (var photo in apiCallResult.photos)
            {
                photo.img_src = "http://path.com/" + photo.img_src; //so the "lastIndexOf" works
            }
            _proxy.Setup(p => p.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(apiCallResult); //good api call
            _file.Setup(f => f.Exists(It.IsAny<string>())).Returns(false); //file does not exist
           
            //act
            var result = await _service.GetOneAsync();

            //assert
            Assert.NotNull(result);
            //we called the api
            _proxy.Verify(s=>s.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>()),Times.Once());
            //we checked if the file exists
            var part = result.Url.Split('/', StringSplitOptions.None).Last();
            _file.Verify(f=>f.Exists(It.Is<string>(s=>s.EndsWith(part))), Times.Once);
            //and since it did not, we download it
            httpTest.ShouldHaveMadeACall();
            //since is random, check we picked one of the possibles
            Assert.Contains(apiCallResult.photos, s =>s.img_src.Contains(part));
        }

        /// <summary>
        /// In this test we verify that all goes good until the downloading phase.
        /// in this case the service class will return the original url (no local copy)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetOneAsync_FailedToDownload()
        {
            //arrange
          
            var httpTest = new HttpTest();
            httpTest.RespondWith( "error", status:500); //simulate failed download
            var apiCallResult = _fixture.Create<RoverResultModel>();
            foreach (var photo in apiCallResult.photos)
            {
                photo.img_src = "http://path.com/" + photo.img_src; //so the "lastIndexOf" works
            }
            _proxy.Setup(p => p.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(apiCallResult); //good api call
            _file.Setup(f => f.Exists(It.IsAny<string>())).Returns(false); //file does not exist
           
            //act
            var result = await _service.GetOneAsync();

            //assert
            Assert.NotNull(result);
            //we called the api
            _proxy.Verify(s=>s.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>()),Times.Once());
            //we check if we have the file on storage before downloading it
            _file.Verify(f=>f.Exists(It.IsAny<string>()), Times.Once);
            //we did a call to try and download
            httpTest.ShouldHaveCalled(result.Url); 
            //since is random, check we picked one of the possibles (because this is when we failed to download it, we use the original url
            Assert.Contains(apiCallResult.photos, s =>s.img_src==result.Url);
        }

        [Fact]
        public async Task GetOneAsync_ApiCallFailed()
        {
            //arrange
          
            var httpTest = new HttpTest();
            httpTest.RespondWith( "error", status:500); //simulate failed download
            var apiCallResult = _fixture.Create<RoverResultModel>();
            foreach (var photo in apiCallResult.photos)
            {
                photo.img_src = "http://path.com/" + photo.img_src; //so the "lastIndexOf" works
            }
            _proxy.Setup(p => p.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>())).ThrowsAsync(new Exception()); //bad api call
            
            //act
            var result = await _service.GetOneAsync();

            //assert
            Assert.NotNull(result);
            //we called the api
            _proxy.Verify(s=>s.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>()),Times.Once());
            //we check if we have the file on storage before downloading it
            _file.Verify(f=>f.Exists(It.IsAny<string>()), Times.Never);
            //we did a call to try and download
            httpTest.ShouldNotHaveMadeACall(); 
            Assert.Null(result.Url); //this will show "not found" in the ui
            
        }

        /// <summary>
        /// In this test we verify that if we have an api call cached, we do not call the api again
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetOneAsync_AllOk_CacheWorks()
        {
            //arrange
          
            var httpTest = new HttpTest();
            httpTest.RespondWith("some content");
            var apiCallResult = _fixture.Create<RoverResultModel>();
            foreach (var photo in apiCallResult.photos)
            {
                photo.img_src = "http://path.com/" + photo.img_src; //so the "lastIndexOf" works
            }
            //{"opportunity", "curiosity", "spirit"};
            var date = DateTime.Parse(_settings.Value.Dates[0]);
            _memoryCache.Set("opportunity-" +date, apiCallResult, TimeSpan.FromMinutes(5));
            _memoryCache.Set("curiosity-" +date, apiCallResult, TimeSpan.FromMinutes(5));
            _memoryCache.Set("spirit-" +date, apiCallResult, TimeSpan.FromMinutes(5));

            _proxy.Setup(p => p.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(apiCallResult); //good api call
            _file.Setup(f => f.Exists(It.IsAny<string>())).Returns(false); //file does not exist
           
            //act
            var result = await _service.GetOneAsync(); 

            //assert
            Assert.NotNull(result);
            //we never called the api (because it was cached)
            _proxy.Verify(s=>s.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>()),Times.Never);
            //we checked if the file exists
            _file.Verify(f=>f.Exists(It.Is<string>(s=>s.EndsWith(result.Url.Split('/', StringSplitOptions.None).Last()))), Times.Once);
            //and since it did not, we downlaod it
            httpTest.ShouldHaveMadeACall();
        }

        /// <summary>
        /// If the file is already downloaded, we dont do it again
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetOneAsync_AllOk_NoReDownload()
        {
            //arrange
          
            var httpTest = new HttpTest();
            httpTest.RespondWith("some content");
            var apiCallResult = _fixture.Create<RoverResultModel>();
            foreach (var photo in apiCallResult.photos)
            {
                photo.img_src = "http://path.com/" + photo.img_src; //so the "lastIndexOf" works
            }
            //{"opportunity", "curiosity", "spirit"};
            var date = DateTime.Parse(_settings.Value.Dates[0]);
            _memoryCache.Set("opportunity-" +date, apiCallResult, TimeSpan.FromMinutes(5));
            _memoryCache.Set("curiosity-" +date, apiCallResult, TimeSpan.FromMinutes(5));
            _memoryCache.Set("spirit-" +date, apiCallResult, TimeSpan.FromMinutes(5));

            _proxy.Setup(p => p.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(apiCallResult); //good api call
            _file.Setup(f => f.Exists(It.IsAny<string>())).Returns(true); //file DOEST 
           
            //act
            var result = await _service.GetOneAsync(); 

            //assert
            Assert.NotNull(result);
            //we never called the api (because it was cached)
            _proxy.Verify(s=>s.GetPicturesAsync(It.IsAny<string>(), It.IsAny<DateTime>()),Times.Never);
            //we checked if the file exists
            _file.Verify(f=>f.Exists(It.Is<string>(s=>s.EndsWith(result.Url.Split('/', StringSplitOptions.None).Last()))), Times.Once);
            //and since it did not, we downlaod it
            httpTest.ShouldNotHaveMadeACall();
        }
    }
}
