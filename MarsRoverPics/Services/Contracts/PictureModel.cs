using System;

namespace MarsRoverPics.Services.Contracts
{
    /// <summary>
    /// Model used by <see cref="IPictureService"/>
    /// </summary>
    public class PictureModel
    {
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public string Rover { get; set; }
        public string Camera { get; set; }
    }
}