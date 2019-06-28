using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MarsRoverPics.Models;
using MarsRoverPics.Services.Contracts;

namespace MarsRoverPics.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPictureService _service;

        public HomeController(IPictureService service)
        {
            _service = service;
        }


        /// <summary>
        /// Index action will render the page with one random pic. The service class behind it, will download it and cache it
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
           var pic = await _service.GetOneAsync();
           //we could use automapper here also
           var viewModel = new HomeViewModel()
           {
               Camera = pic.Camera,
               Rover = pic.Rover,
               Date =  pic.Date,
               Url = pic.Url
           };
           return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
