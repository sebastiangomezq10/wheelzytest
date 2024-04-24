using wheelzytest.Domain.DTOs;
using wheelzytest.Domain.Models;

namespace wheelzytest.Domain.Interfaces
{
    public interface ICarRepository
    {
        Task<IEnumerable<CarInfoDto>> GetCarInformationAsync();
    }
}