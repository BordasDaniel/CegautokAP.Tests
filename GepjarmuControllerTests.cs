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
        public void GetByIdTest()
        {
            int existingid = 1;
            int nonExistingId = 999;

            var result = _controller.GetGepjarmuById(existingid);
            var nonexistingResult = _controller.GetGepjarmuById(nonExistingId);

            Assert.IsNotNull(nonexistingResult);
            Assert.IsNotNull(result);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.IsInstanceOfType(nonexistingResult, typeof(BadRequestObjectResult));

            var okResult = (OkObjectResult)result;
            var badRequestResult = (BadRequestObjectResult)nonexistingResult;
            Assert.IsNotNull(badRequestResult);
            Assert.IsNotNull(okResult);

            Assert.IsInstanceOfType(okResult.Value, typeof(Gepjarmu));

            Gepjarmu gepjarmu = (Gepjarmu)okResult.Value;
            Assert.IsNotNull(gepjarmu);
            Assert.AreEqual("ABC-123", gepjarmu.Rendszam);
            Assert.AreEqual("Nincs ilyen gépjármű", badRequestResult.Value);
        }

        [TestMethod]
        public void AddNewGepjarmuTest()
        {
            var newGepjarmu = new Gepjarmu()
            {
                Id = 3,
                Rendszam = "DEF-456",
                Marka = "Ford",
                Tipus = "Hatchback",
                Ulesek = 5
            };
            var result = _controller.AddNewGepjarmu(newGepjarmu);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var okresult = result as ObjectResult;
            Assert.IsNotNull(okresult);
            Assert.AreEqual(200, okresult.StatusCode);
            Assert.AreEqual("Sikeres rögzítés", okresult.Value);

            // hibás adatokkal
            Gepjarmu invalidGepjarmu = null;
            var resultHibas = _controller.AddNewGepjarmu(invalidGepjarmu);
            Assert.IsNotNull(resultHibas);
            Assert.IsInstanceOfType(resultHibas, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)resultHibas;
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(string));
            Assert.IsTrue(((string)badRequestResult.Value).StartsWith("Hiba történt a felvétel során:"));

            // ha már létezik ilyen rendszámú gépjármű

            Gepjarmu duplicateRendszam = new Gepjarmu()
            {
                Id = 4,
                Rendszam = "ABC-123",
                Marka = "Nissan",
                Tipus = "Coupe",
                Ulesek = 4
            };

            var resultletezo = _controller.AddNewGepjarmu(duplicateRendszam);
            Assert.IsNotNull(duplicateRendszam);
            Assert.IsInstanceOfType(resultletezo, typeof(BadRequestObjectResult));
            Assert.IsNotNull(resultletezo);
            Assert.AreEqual("Már létezik ilyen rendszámú gépjármű!", ((BadRequestObjectResult)resultletezo).Value);

        }

        [TestMethod]
        public void UpdateJarmuTest()
        {
            Gepjarmu existingGepjarmu = new Gepjarmu()
            {
                Id = 1,
                Rendszam = "ABC-123",
                Marka = "Toyota",
                Tipus = "Hatchback",
                Ulesek = 4           
            };

            var result = _controller.ModifyGepjarmu(existingGepjarmu);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);

            var updatedDbGepjarmu = _context.Gepjarmus.Find(1);
            Assert.IsNotNull(updatedDbGepjarmu);
            Assert.AreEqual("Hatchback", updatedDbGepjarmu.Tipus);
            Assert.AreEqual(4, updatedDbGepjarmu.Ulesek);

            // jarmu == null
            var nullResult = _controller.ModifyGepjarmu(null);
            Assert.IsNotNull(nullResult);
            Assert.IsInstanceOfType(nullResult, typeof(BadRequestObjectResult));

            // jarmu.Id < 1
            Gepjarmu invalidIdGepjarmu = new Gepjarmu()
            {
                Id = 0,
                Rendszam = "INV-000",
                Marka = "Toyota",
                Tipus = "Sedan",
                Ulesek = 5
            };
            var invalidIdResult = _controller.ModifyGepjarmu(invalidIdGepjarmu);
            Assert.IsNotNull(invalidIdResult);
            Assert.IsInstanceOfType(invalidIdResult, typeof(BadRequestObjectResult));

            // (Id = 999)
            Gepjarmu nonExistentGepjarmu = new Gepjarmu()
            {
                Id = 999,
                Rendszam = "NNE-999",
                Marka = "BMW",
                Tipus = "Coupe",
                Ulesek = 2
            };
            var nonExistentResult = _controller.ModifyGepjarmu(nonExistentGepjarmu);
            Assert.IsNotNull(nonExistentResult);
            Assert.IsInstanceOfType(nonExistentResult, typeof(BadRequestObjectResult));
        }

       
    }
}
