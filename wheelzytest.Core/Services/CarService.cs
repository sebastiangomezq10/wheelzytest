using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using wheelzytest.Core.Interfaces;
using wheelzytest.Domain.DTOs;
using wheelzytest.Domain.Interfaces;
using wheelzytest.Domain.Models;

namespace wheelzytest.Core.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;

        public CarService(ICarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        public async Task<IEnumerable<CarInfoDto>> GetCarInformationAsync()
        {
            return await _carRepository.GetCarInformationAsync();
        }
    }
}
