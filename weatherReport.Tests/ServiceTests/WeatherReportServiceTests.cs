
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using weatherReport.Services;
using weatherReport.Models;
using System;

namespace weatherReport.Tests
{
    [TestClass]
    public class WeatherReportServiceTests
    {
        private IConfiguration _config;
        private static SharedMemory _sharedMemory;
        private WeatherReport _weatherReport;
        private IWeatherReportService weatherReportService;

        [TestInitialize]
        public void Initalize()
        {
            var testHelper = new TestHelper();
            _config = testHelper.Configuration;
            _sharedMemory = new SharedMemory();
            _weatherReport = testHelper.weatherReport();
            weatherReportService = new WeatherReportService(_sharedMemory, _config);
            SaveCityReportTest();
        }

        //creating sample data with one city
        public void SaveCityReportTest()
        {
            try
            {
                var saveWeatherReport = weatherReportService.SaveWeatherReport(_weatherReport);
                _sharedMemory.weatherReports.Add(saveWeatherReport);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating test data, Model should not be empty. " + ex.Message);
            }
        }

        //Throws exception for adding Invalid date format for a day  in the city
        [TestMethod]
        public void InvalidDateFormatCityReportTest()
        {

            Exception expectedExcetpion = null;
            try
            {
                var detail = new Detail
                { 
                Date = DateTime.Parse("17-04-2020"),
                MeanTemperature = 17
                };
            _weatherReport.details.Add(detail);

            }
            catch (Exception ex)
            {
                expectedExcetpion = ex;
            }
            Assert.IsNotNull(expectedExcetpion);
            Assert.AreEqual(expectedExcetpion.Message, "String '17-04-2020' was not recognized as a valid DateTime.");
        }

        //Throws exception for adding duplicates date for same city
        [TestMethod]
        public void AddDuplicateDateForCityReportTest()
        {

            Exception expectedExcetpion = null;

            var detail = new Detail
            {
                Date = DateTime.Parse("2020-04-19"),
                MeanTemperature = 17
            };
            _weatherReport.details.Add(detail);

            try
            {
                var weatherReport = weatherReportService.SaveWeatherReport(_weatherReport);
            }
            catch (Exception ex)
            {
                expectedExcetpion = ex;
            }
            Assert.IsNotNull(expectedExcetpion);

        }

        // Adds new date for existing city
        [TestMethod]
        public void AddNewDateForExistingCityReportTest()
        {

            Exception expectedExcetpion = null;
            bool pass = false;
            var detail = new Detail
            {
                Date = DateTime.Parse("2020-04-23"),
                MeanTemperature = 17
            };
            _weatherReport.details.Add(detail);
            var count = _sharedMemory.weatherReports.Count;
            try
            {
                var weatherReport = weatherReportService.SaveWeatherReport(_weatherReport);
                if (_sharedMemory.weatherReports.Count > count)
                {
                    pass = true;
                }
                Assert.IsTrue(pass, "City not added");
            }
            catch (Exception ex)
            {
                expectedExcetpion = ex;
            }
        }

        // Adds new city for weather report
        [TestMethod]
        public void AddNewCity_SaveCityReportTest()
        {
            bool pass = false;
            _weatherReport.City = "seattle";
            var count = _sharedMemory.weatherReports.Count;
            var weatherReport = weatherReportService.SaveWeatherReport(_weatherReport);
            if (_sharedMemory.weatherReports.Count > count)
            {
                var cityAdded = _sharedMemory.weatherReports.FirstOrDefault(x => x.City == _weatherReport.City);
                Assert.AreEqual(cityAdded.City, _weatherReport.City);
                pass = true;
            }
            Assert.IsTrue(pass, "City not added");
        }

        // Fetches city report for city entered ignores case as well
        [TestMethod]
        public void GetCityReportTest()
        {
            bool pass = false;
            string city = "vanCoUver";
            var getWeatherReport = weatherReportService.GetCityData(city, "", "");
            if (getWeatherReport.City.Equals(city, StringComparison.InvariantCultureIgnoreCase))
            {
                pass = true;
            }
            Assert.IsTrue(pass, "City not found");
        }

        // Fetches city report for specific date range city entered
        [TestMethod]
        public void GetCityWithSpecificDate_GetCityReportTest()
        {
            bool pass = false;
            string city = "Vancouver";
            var getWeatherReport = weatherReportService.GetCityData(city, "2020-04-17", "2020-04-19");
            if (getWeatherReport.City.Equals(city, StringComparison.InvariantCultureIgnoreCase))
            {
                pass = true;
            }
            Assert.IsTrue(pass, "specific report for City not found ");
        }

        // Throws error for city report requested for not in range dates 
        [TestMethod]
        public void GetCityWithSpecificDateNotInRange_GetCityReportTest()
        {
            bool pass = false;
            Exception expectedException = null;
            string city = "vancouver";
            try
            {
                var getWeatherReport = weatherReportService.GetCityData(city, "2020-04-11", "2020-04-16");
                if (getWeatherReport.City.Equals(city, StringComparison.InvariantCultureIgnoreCase))
                {
                    pass = true;
                }
                Assert.IsTrue(pass, "specific report for City not found ");
            }
            catch (Exception ex)
            {
                expectedException = ex;

            }
            Assert.IsNotNull(expectedException);
        }

        // Throws error for city requested because end date is before start date range
        [TestMethod]
        public void GetCityWithEndDateLessThanStartDate_GetCityReportTest()
        {
            bool pass = false;
            Exception expectedException = null;
            string city = "vancouver";
            try
            {
                var getWeatherReport = weatherReportService.GetCityData(city, "2020-04-17", "2020-04-15");
                if (getWeatherReport.City.Equals(city, StringComparison.InvariantCultureIgnoreCase))
                {
                    pass = true;
                }
                Assert.IsTrue(pass, "specific report for City not found ");
            }
            catch (Exception ex)
            {
                expectedException = ex;

            }
            Assert.IsNotNull(expectedException);
        }

        // Throws error for City which is not in weather report
        [TestMethod]
        public void GetCityNotExist_GetCityReportTest()
        {
            bool pass = false;
            Exception expectedException = null;
            string city = "bellvue";
            try
            {
                var getWeatherReport = weatherReportService.GetCityData(city, "2020-04-11", "2020-04-16");
                if (getWeatherReport.City.Equals(city, StringComparison.InvariantCultureIgnoreCase))
                {
                    pass = true;
                }
                Assert.IsTrue(pass, "specific report for City not found ");
            }
            catch (Exception ex)
            {
                expectedException = ex;

            }
            Assert.IsNotNull(expectedException);
        }

        // Deletes requested  city report successfully
        [TestMethod]
        public void DeleteCityTest()
        {
            bool pass = false;
            Exception expectedException = null;
            string city = "Vancouver";
            try
            {
                var cityDeleted = weatherReportService.DeleteCityData(city);
                if (cityDeleted != null)
                {
                    pass = true;
                }
                Assert.IsTrue(pass, "specific report for City not found ");
            }
            catch (Exception ex)
            {
                expectedException = ex;

            }
        }

        // Throws error as invalid city is requested to delete
        [TestMethod]
        public void DeleteCityTestThrowsException()
        {
            bool pass = false;
            Exception expectedException = null;
            string city = "bellvue";
            try
            {
                var cityDeleted = weatherReportService.DeleteCityData(city);
                if (cityDeleted != null)
                {
                    pass = true;
                }
                Assert.IsTrue(pass, "specific report for City not found ");
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }
            Assert.IsNotNull(expectedException);
        }
    }
}
