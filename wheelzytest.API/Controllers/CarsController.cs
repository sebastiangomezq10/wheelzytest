using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using wheelzytest.Core.Interfaces;

namespace wheelzytest.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCars()
        {
            var cars = await _carService.GetCarInformationAsync();
            return Ok(cars);
        }

        // Implementar más métodos CRUD aquí
    }
}
