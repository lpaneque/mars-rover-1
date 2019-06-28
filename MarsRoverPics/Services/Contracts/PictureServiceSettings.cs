using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MarsRoverPics.Services.Contracts
{
    /// <summary>
    /// Settings object for PictureService
    /// </summary>
    public class PictureServiceSettings: IOptions<PictureServiceSettings>
    {
        public string[] Dates { get; set; }
        public PictureServiceSettings Value { get; }

        public PictureServiceSettings()
        {
            this.Value = this;
        }
    }
}
