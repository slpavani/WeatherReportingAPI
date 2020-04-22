using FluentValidation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace weatherReport.ExceptionHandling
{
    public static class DateTimeException
    {
        public static DateTime ToDateTime(this string source)
        {
            if (DateTime.TryParseExact(source, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue))
            {
                return DateTime.SpecifyKind(dateValue, DateTimeKind.Unspecified);
            }
            else
            {
                throw new ValidationException("Please select datetime format as 'yyyy-MM-dd'");
            }
        }
    }
}
