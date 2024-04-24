using Microsoft.EntityFrameworkCore;
using wheelzytest.Core.Interfaces;
using wheelzytest.Core.Services;
using wheelzytest.Domain.Interfaces;
using wheelzytest.Domain.Models;
using wheelzytest.Infrastructure.Context;
using wheelzytest.Infrastructure.DataInitialization;
using wheelzytest.Infrastructure.Repositories;

namespace WheelzyTest.UnitTests
{
    public class CarServiceTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public CarServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryCarDatabaseForServiceTest")
                .Options;

            // Inicialización de la base de datos con datos de prueba
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using var context = new AppDbContext(_dbContextOptions);

            // Asegúrate de que la base de datos esté vacía antes de sembrar datos de prueba
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            DatabaseInitializer.Initialize(context);

        }

        [Fact]
        public async Task GetCarInformationAsync_ShouldReturnCarWithBuyerQuoteAndStatus()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            ICarRepository carRepository = new CarRepository(context);
            ICarService carService = new CarService(carRepository);

            // Act
            var carInfo = await carService.GetCarInformationAsync();

            // Assert
            Assert.NotNull(carInfo);

        }
    }
}
