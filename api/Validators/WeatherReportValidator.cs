using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using weatherReport.Models;
using FluentValidation;


namespace weatherReport.Validators
{
    public class WeatherReportValidator : AbstractValidator<WeatherReport>
    {
        private readonly IEnumerable<WeatherReport> _weatherReports;

        public WeatherReportValidator(IEnumerable<WeatherReport> weatherReports)
        {
            _weatherReports = weatherReports;
            CascadeMode = CascadeMode.StopOnFirstFailure;
            RuleFor(x => x.City)
                .NotEmpty();
            RuleFor(x => x.City).Length(0, 100)
                .WithMessage(" City Name must not exceed 100 characters.");
            RuleFor(x => x.details)
                .ForEach(y => y.SetValidator(new DetailValidator()));

        }
    }
}
