using CegautokAP.Controllers;
using CegautokAP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CegautokAP.Tests
{
    [TestClass]
    public class GepjarmuControllerTests
    {
        FlottaContext _context;
        GepjarmuController _controller;

        private FlottaContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<FlottaContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new FlottaContext(options);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _context = CreateInMemoryContext(nameof(TestInitialize));
            _controller = new GepjarmuController(_context);

            var gepjarmu1 = new Gepjarmu
            {
                Id = 1,
                Rendszam = "ABC-123",
                Marka = "Toyota",
                Tipus = "Sedan",
                Ulesek = 5
            };

            var gepjarmu2 = new Gepjarmu
            {
                Id = 2,
                Rendszam = "XYZ-789",
                Marka = "Honda",
                Tipus = "SUV",
                Ulesek = 7
            };

            _context.Gepjarmus.AddRange(gepjarmu1, gepjarmu2);
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            _controller = new GepjarmuController(_context);
        }

        private static Gepjarmu CreateSampleGepjarmu(int id = 1) => new Gepjarmu
        {
            Id = id,
            Rendszam = "ABC-123",
            Marka = "Toyota",
            Tipus = "Sedan",
            Ulesek = 5
        };

        [TestMethod]
        public void GetAllGepjarmusTest()
        {
            //Arrange
            //Act
            //Assert

            var result = _controller.GetAllGepjarmus();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOfType(okResult.Value, typeof(List<Gepjarmu>));
            List<Gepjarmu> gepjarmus = (List<Gepjarmu>)okResult.Value;
            Assert.IsNotNull(gepjarmus);
            Assert.AreEqual(2, gepjarmus.Count);
            Assert.AreEqual("ABC-123", gepjarmus[0].Rendszam);

        }


        [TestMethod]
        public void GetAllGepjarmus_ReturnsOk_WhenDatabaseIsEmpty()
        {
            using var context = CreateInMemoryContext(nameof(GetAllGepjarmus_ReturnsOk_WhenDatabaseIsEmpty));
            var controller = new GepjarmuController(context);

            var result = controller.GetAllGepjarmus();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            var list = ok.Value as List<Gepjarmu>;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void GetAllGepjarmus_ReturnsOk_WithAllRecords()
        {
            using var context = CreateInMemoryContext(nameof(GetAllGepjarmus_ReturnsOk_WithAllRecords));
            context.Gepjarmus.AddRange(CreateSampleGepjarmu(1), CreateSampleGepjarmu(2));
            context.SaveChanges();
            var controller = new GepjarmuController(context);

            var result = controller.GetAllGepjarmus();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            var list = ok.Value as List<Gepjarmu>;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
        }


        [TestMethod]
        public void GetGepjarmuById_ReturnsOk_WhenIdExists()
        {
            using var context = CreateInMemoryContext(nameof(GetGepjarmuById_ReturnsOk_WhenIdExists));
            var gepjarmu = CreateSampleGepjarmu(1);
            context.Gepjarmus.Add(gepjarmu);
            context.SaveChanges();
            var controller = new GepjarmuController(context);

            var result = controller.GetGepjarmuById(1);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.IsNotNull(ok.Value);
            Assert.AreEqual(1, ((Gepjarmu)ok.Value).Id);
        }

        [TestMethod]
        public void GetGepjarmuById_ReturnsBadRequest_WhenIdNotFound()
        {
            using var context = CreateInMemoryContext(nameof(GetGepjarmuById_ReturnsBadRequest_WhenIdNotFound));
            var controller = new GepjarmuController(context);

            var result = controller.GetGepjarmuById(99);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Nincs ilyen gépjármű", bad.Value);
        }

        [TestMethod]
        public void AddNewGepjarmu_ReturnsOk_AndPersistsRecord()
        {
            using var context = CreateInMemoryContext(nameof(AddNewGepjarmu_ReturnsOk_AndPersistsRecord));
            var controller = new GepjarmuController(context);
            var gepjarmu = CreateSampleGepjarmu(1);

            var result = controller.AddNewGepjarmu(gepjarmu);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Sikeres rögzítés", ok.Value);
            Assert.AreEqual(1, context.Gepjarmus.Count());
        }

        [TestMethod]
        public void AddNewGepjarmu_ReturnsOk_RendszamIsStoredCorrectly()
        {
            using var context = CreateInMemoryContext(nameof(AddNewGepjarmu_ReturnsOk_RendszamIsStoredCorrectly));
            var controller = new GepjarmuController(context);
            var gepjarmu = CreateSampleGepjarmu(1);

            controller.AddNewGepjarmu(gepjarmu);

            var stored = context.Gepjarmus.First();
            Assert.AreEqual("ABC-123", stored.Rendszam);
        }


        [TestMethod]
        public void ModifyGepjarmu_ReturnsOk_WhenGepjarmuExists()
        {
            using var context = CreateInMemoryContext(nameof(ModifyGepjarmu_ReturnsOk_WhenGepjarmuExists));
            var gepjarmu = CreateSampleGepjarmu(1);
            context.Gepjarmus.Add(gepjarmu);
            context.SaveChanges();

            // Detach the entity so we can simulate an update from outside
            context.Entry(gepjarmu).State = EntityState.Detached;

            var controller = new GepjarmuController(context);
            var updated = new Gepjarmu
            {
                Id = 1,
                Rendszam = "XYZ-999",
                Marka = "Honda",
                Tipus = "SUV",
                Ulesek = 7
            };

            var result = controller.ModifyGepjarmu(updated);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Sikeres módosítás!", ok.Value);
        }

        [TestMethod]
        public void ModifyGepjarmu_ReturnsBadRequest_WhenGepjarmuNotFound()
        {
            using var context = CreateInMemoryContext(nameof(ModifyGepjarmu_ReturnsBadRequest_WhenGepjarmuNotFound));
            var controller = new GepjarmuController(context);
            var nonExistent = CreateSampleGepjarmu(99);

            var result = controller.ModifyGepjarmu(nonExistent);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Nincs ilyen gépjűrmű!", bad.Value);
        }


        [TestMethod]
        public void DeleteGepjarmu_ReturnsOk_AndRemovesRecord()
        {
            using var context = CreateInMemoryContext(nameof(DeleteGepjarmu_ReturnsOk_AndRemovesRecord));
            context.Gepjarmus.Add(CreateSampleGepjarmu(1));
            context.SaveChanges();
            var controller = new GepjarmuController(context);

            var result = controller.DeleteGepjarmu(1);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Sikeres törlés!", ok.Value);
            Assert.AreEqual(0, context.Gepjarmus.Count());
        }

        [TestMethod]
        public void DeleteGepjarmu_ReturnsBadRequest_WhenIdNotFound()
        {
            using var context = CreateInMemoryContext(nameof(DeleteGepjarmu_ReturnsBadRequest_WhenIdNotFound));
            var controller = new GepjarmuController(context);

            var result = controller.DeleteGepjarmu(99);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Nincs ilyen gépjármű!", bad.Value);
        }
    }
}
