using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsRoverPics.Proxies.Contracts
{
    /// <summary>
    /// Inerface for a Nasa API proxy
    /// </summary>
    public interface INasaApiProxy
    {
        /// <summary>
        /// Retrieves full json object given a rover and a date (earth date)
        /// </summary>
        /// <param name="rover"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<RoverResultModel> GetPicturesAsync(string rover, DateTime date);
    }
}
