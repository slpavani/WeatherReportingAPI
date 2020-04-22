using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace weatherReport.ExceptionHandling
{
    public class InvalidCityException : Exception
    {
        public InvalidCityException(string message)
        {
            throw new ValidationException(message);
        }


    }

    public class DuplicateReportForSameDay : Exception
    {
        public DuplicateReportForSameDay(string message)
        {
            throw new ValidationException(message);
        }
    }
}
