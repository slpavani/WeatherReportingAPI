using weatherReport.Models;
using FluentValidation;
using System;
using System.Globalization;

namespace weatherReport.Validators
{
    public class DetailValidator : AbstractValidator<Detail>
    {
        public DetailValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Date)
                .Must(x => IsDate(x.ToString("yyyy-MM-dd"), "yyyy-MM-dd"))
                .WithMessage("'Date Of weatherreport' must be a valid date 'yyyy-MM-dd'")
                .NotEmpty();
            RuleFor(x => x.MeanTemperature)
               .Must(x => x != 0)
               .WithMessage("Invalid Temeperature.").NotEmpty();

        }

        public bool IsDate(string source, string format)
        {
            return DateTime.TryParseExact(source, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue);
        }
    }
}
