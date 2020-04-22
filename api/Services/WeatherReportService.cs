using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using weatherReport.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using weatherReport;
using weatherReport.ExceptionHandling;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace weatherReport.Services
{
    public interface IWeatherReportService
    {
        public Boolean ValidateUserToken(StringValues authorizationHeader);
        public WeatherReport SaveWeatherReport(WeatherReport weatherReport);
        public WeatherReport GetCityData(string city, string startDate, string endDate);
        public string DeleteCityData(string city);
    }
    public class WeatherReportService : IWeatherReportService
    {

        private SharedMemory _sharedMemory;
        private IConfiguration _config;
        public WeatherReportService(SharedMemory sharedMemory, IConfiguration config)
        {
            _sharedMemory = sharedMemory;
            _config = config;
        }

        //Validating User Access before allowing to access API
        public Boolean ValidateUserToken(StringValues authorizationHeader)
        {
            try
            {
                var token = authorizationHeader.Single().Split(" ").Last();
                LoginService loginservice = new LoginService(_config);
                string tokenUsername = loginservice.ValidateToken(token);
                //defining hardcoded value for demo purpose only
                if (!tokenUsername.Equals("kirang"))
                {
                    return false;
                }
                return true;
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("No Login token found" +ex.Message);
            }
        }

        //Save Weather Report
        public WeatherReport SaveWeatherReport(WeatherReport model)
        {
            if (model == null)
            {           
                throw new ValidationException("request body can not be empty.");
            }

            List<DateTime> duplicateDates = model.details.Select(x => x.Date).Distinct().ToList();
            if (duplicateDates.Count < model.details.Count)
            {
                throw new DuplicateReportForSameDay("Duplicate weather record for same day is found. Please check");
            }

            try
            {
                var isExists = _sharedMemory.weatherReports.Any(x => x.City == model.City);
                if (!isExists)
                {
                    AddNewCityReport(model);
                }
                else
                {
                    UpdateExistingCityReport(model);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("error updating existing city details \n" + ex.Message);
            }
            return _sharedMemory.weatherReports.FirstOrDefault(x => x.City == model.City);
        }

        //Get report for specific city with/without daterange provided
        public WeatherReport GetCityData(string city, string startDate, string endDate)
        {
            var cityData = _sharedMemory.weatherReports
                .FirstOrDefault(x => string.Compare(x.City, city, StringComparison.InvariantCultureIgnoreCase) == 0);
            try
            {
                if (cityData == null) throw new InvalidCityException("No city found");


                //get all city data if daterange not specified
                if (string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate)) return cityData;

                //get city data with in given daterange
                else
                {
                    var start = startDate.ToDateTime();
                    var end = endDate.ToDateTime().AddDays(1);
                    if (start > end)
                    {
                        throw new ValidationException("start date can not be greater than end date");
                    }

                    var details = new List<Detail>(cityData.details.Where(x => x.Date >= start && x.Date < end));
                    if (!details?.Any() == true)
                    {
                        throw new ValidationException("No data found with the given date range.");
                    }

                    var result = new WeatherReport()
                    {
                        City = cityData.City,
                        details = details.ToList()
                    };

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting specific city data \n" + ex.Message);
            }

        }

        //Delete city report
        public string DeleteCityData(string city)
        {
            var item = _sharedMemory.weatherReports
                    .FirstOrDefault(x => string.Compare(x.City, city, StringComparison.InvariantCultureIgnoreCase) == 0);
            try
            {
                if (item != null)
                {
                    _sharedMemory.weatherReports.Remove(item);
                    return item.City + " city weather report deleted";
                }
                else
                {

                    return new InvalidCityException("Invalid City").ToString();

                }
            }
            catch (InvalidCityException ex)
            {
                throw new InvalidCityException(ex.Message);
            }
        }

        //Add new City Report
        private void AddNewCityReport(WeatherReport model)
        {
            var newCityReport = new WeatherReport()
            {
                City = model.City,
                details = new List<Detail>()
            };
            foreach (var detail in model.details)
            {
                newCityReport.details.Add(detail);
            }

            _sharedMemory.weatherReports.Add(newCityReport);
        }

        //Updating existing city report
        private void UpdateExistingCityReport(WeatherReport model)
        {
            var existingCityDetails = _sharedMemory.weatherReports.FirstOrDefault(x => x.City == model.City);
            existingCityDetails = UpdateTemeperatureForExistingDay(existingCityDetails, model);
            existingCityDetails = UpdateExistingCityReportWithNewDays(existingCityDetails, model);
            existingCityDetails = RemoveStaleWeatherData(existingCityDetails, model);
        }

        //Removing stale data from city report
        private WeatherReport RemoveStaleWeatherData(WeatherReport existingCityDetails, WeatherReport model)
        {
            try
            {
                List<Detail> removedDetails = existingCityDetails.details.Where(x => !model.details.Any(y => y.Date == x.Date)).ToList();
                for (int i = existingCityDetails.details.Count - 1; i > -1; i--)
                {
                    Detail detail = existingCityDetails.details[i];
                    for (int j = 0; j <= removedDetails.Count() - 1; j++)
                    {
                        Detail removedDetail = removedDetails[j];
                        if (detail.Date == removedDetail.Date)
                        {
                            existingCityDetails.details.RemoveAt(i);
                        }
                    }
                }
                _sharedMemory.weatherReports.FirstOrDefault(x => x.City == model.City).details = existingCityDetails.details;

                return _sharedMemory.weatherReports.FirstOrDefault(x => x.City == model.City);

            }
            catch (Exception ex)
            {
                throw new Exception("Error removing stale data for existing city \n" + ex.Message);
            }
        }

        //Updating  existing City Report with new days
        private WeatherReport UpdateExistingCityReportWithNewDays(WeatherReport existingCityDetails, WeatherReport model)
        {
            try
            {
                List<Detail> updatedDetails = model.details.Where(x => !existingCityDetails.details.Any(y => y.Date == x.Date)).ToList();

                var updatedDetail = existingCityDetails
                 .details
                 .Concat(updatedDetails);
                _sharedMemory.weatherReports.FirstOrDefault(x => x.City == model.City).details = updatedDetail.ToList();
                return _sharedMemory.weatherReports.FirstOrDefault(x => x.City == model.City);

            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new days report for existing city \n" + ex.Message);
            }
        }

        //updating existing report for already present days
        private WeatherReport UpdateTemeperatureForExistingDay(WeatherReport existingCityDetails, WeatherReport model)
        {
            List<Detail> updatedTemperatureDetails;
            try
            {
                updatedTemperatureDetails = model.details.Where(x => existingCityDetails.details.Any(y => y.Date == x.Date && y.MeanTemperature != x.MeanTemperature)).ToList();
                if (updatedTemperatureDetails != null)
                {
                    for (int i = 0; i < updatedTemperatureDetails.Count; i++)
                    {
                        for (int j = 0; j < existingCityDetails.details.Count(); j++)
                        {

                            if (existingCityDetails.details[j].Date == updatedTemperatureDetails[i].Date)
                            {
                                existingCityDetails.details[j].MeanTemperature = updatedTemperatureDetails[i].MeanTemperature;
                            }
                        }
                    }
                }
                _sharedMemory.weatherReports.FirstOrDefault(x => x.City == model.City).details = existingCityDetails.details;
                return _sharedMemory.weatherReports.FirstOrDefault(x => x.City == model.City);
            }

            catch (Exception ex)
            {
                throw new Exception("Error updating existing temperature for the day \n" + ex.Message);
            }
        }


    }
}


