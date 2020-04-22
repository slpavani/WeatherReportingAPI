using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using weatherReport.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using weatherReport;
using weatherReport.ExceptionHandling;
using weatherReport.Services;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;

namespace weatherReport.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherReportController : ControllerBase
    {
        private SharedMemory _sharedMemory;
        private readonly ILogger<WeatherReportController> _logger;
        private IConfiguration _config;

        public WeatherReportController(ILogger<WeatherReportController> logger, SharedMemory sharedMemory, IConfiguration config)
        {
            _logger = logger;
            _sharedMemory = sharedMemory;
            _config = config;

        }

        //Get WeatherReport for all cities
        // https://localhost:{port}/weatherreport/
        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<WeatherReport>> GetCompleteWeatherReport()
        {
            var authorizationHeader = Request.Headers["Authorization"];
            var weatherReportService = new WeatherReportService(_sharedMemory,_config);
            //validating user access
            bool validUser = weatherReportService.ValidateUserToken(authorizationHeader);
            if (!validUser)
            {
                _logger.LogError("Unauthorized/Invalid user detected while accessing all get complete report API.");
                return Forbid();
            }
            //Fetching all cities report
            if (!_sharedMemory.weatherReports?.Any() == true)
            {
                _logger.LogInformation("No Weather forecast yet!");
                return NotFound("No Weather forecast yet!");
            }
            return _sharedMemory.weatherReports;
        }


        //Save Weather Report
        //https://localhost:{port}/weatherreport
        [HttpPut]
        [Authorize]
        public ActionResult<WeatherReport> SaveWeatherReport(WeatherReport model)
        {

            var authorizationHeader = Request.Headers["Authorization"];
            var weatherReportService = new WeatherReportService(_sharedMemory, _config);
            //validating user access
            bool validUser = weatherReportService.ValidateUserToken(authorizationHeader);
            if (!validUser)
            {
                _logger.LogError("Unauthorized/Invalid user detected while accessing save report API.");
                return Forbid();
            }

            //saving weather rport
            if (model == null)
            {
                _logger.LogError("request body can not be empty.");
                throw new ValidationException("request body can not be empty.");
            }

            _logger.LogInformation("Calling weatherReportService to save weather report.");
            var weatherReport = weatherReportService.SaveWeatherReport(model);
            return weatherReport;
        }

        //https://localhost:{port}/weatherreport/{city}
        [HttpGet("{city}")]
        [Authorize]
        public ActionResult<WeatherReport> GetCityReport(string city, [FromQuery]string startDate, [FromQuery]string endDate)
        {
            var authorizationHeader = Request.Headers["Authorization"];
            var weatherReportService = new WeatherReportService(_sharedMemory, _config);

            //validating user access
            bool validUser = weatherReportService.ValidateUserToken(authorizationHeader);
            if (!validUser)
            {
                _logger.LogError("Unauthorized/Invalid user detected while accessing Get City Report API.");
                return Forbid();
            }

            // Fetching city report
            _logger.LogInformation("Calling weatherReportService to get specific city weather report.");
            try
            {
                var result = weatherReportService.GetCityData(city, startDate, endDate);
                if (result != null)
                    return result;
                else
                    return NotFound("Error finding Weather report for specific city");
            }
            catch(Exception ex)
            {
                return BadRequest("Invalid city or date range/format specified. Malformed data");
            }
        }

        // https://localhost:{port}/weatherreport/{city}
        [HttpPost("{city}")]
        [Authorize]
        public ActionResult<string> DeleteCityReport(string city)
        {
            var authorizationHeader = Request.Headers["Authorization"];
            var weatherReportService = new WeatherReportService(_sharedMemory, _config);
            //validating user access
            bool validUser = weatherReportService.ValidateUserToken(authorizationHeader);
            if (!validUser)
            {
                _logger.LogError("Unauthorized/Invalid user detected while accessing delete city report API.");
                return Forbid();
            }
           
            _logger.LogInformation("Calling weatherReportService to get specific city weather report.");

            try
            {
                var result = weatherReportService.DeleteCityData(city);
                return result;
            }
            catch (Exception ex) 
            {
                return NotFound("City Not found/doesnt exist");
            }
        }
    }
}
