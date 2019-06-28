using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsRoverPics.Models
{
    /// <summary>
    /// View model for home page. It returns the main object to display on that page
    /// </summary>
    public class HomeViewModel
    {
        public string Camera { get; set; }
        public string Rover { get; set; }
        public DateTime Date { get; set; }
        public string Url { get; set; }
    }
}
