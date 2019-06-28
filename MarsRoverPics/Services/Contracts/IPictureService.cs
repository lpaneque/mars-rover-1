using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsRoverPics.Services.Contracts
{
    /// <summary>
    /// Interface for a Picture Service
    /// </summary>
    public interface IPictureService
    {
        /// <summary>
        /// Gets one picture 
        /// </summary>
        /// <returns></returns>
        Task<PictureModel> GetOneAsync();
    }
}
