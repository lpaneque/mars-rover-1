using System;
using System.Threading.Tasks;
using AutoFixture;
using MarsRoverPics.Controllers;
using MarsRoverPics.Models;
using MarsRoverPics.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MarsRoverPics.Tests
{
    public class HomeControllerTests
    {
        private Mock<IPictureService> _service;
        private Fixture _fixture;


        public HomeControllerTests()
        {
            _service = new Mock<IPictureService>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Index()
        {
            //arrange
            var controller = new HomeController(_service.Object);
            var data = _fixture.Create<PictureModel>();
            _service.Setup(s => s.GetOneAsync()).ReturnsAsync(data);

            //act
            var result = await controller.Index() as ViewResult;

            //assert
            Assert.NotNull(result);
            var obj = (dynamic) result.Model;
            Assert.Equal(data.Url, obj.Url);
            Assert.Equal(data.Camera, obj.Camera);
            Assert.Equal(data.Rover, obj.Rover);
            Assert.Equal(data.Date, obj.Date);

        }
    }
}
