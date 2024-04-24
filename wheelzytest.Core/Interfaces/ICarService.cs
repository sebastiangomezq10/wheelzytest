using wheelzytest.Domain.DTOs;
using wheelzytest.Domain.Models;

namespace wheelzytest.Core.Interfaces
{
    public interface ICarService
    {
        Task<IEnumerable<CarInfoDto>> GetCarInformationAsync();
    }
}